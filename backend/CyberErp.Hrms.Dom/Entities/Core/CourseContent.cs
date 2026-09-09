namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Lifecycle of a course version.</summary>
public enum CourseVersionStatus
{
    /// <summary>Being authored — invisible to learners, freely editable.</summary>
    Draft = 0,
    /// <summary>Live: the version learners work through and are recorded against.</summary>
    Published = 1,
    /// <summary>Superseded. Existing progress stays valid; nobody new is put on it.</summary>
    Retired = 2
}

/// <summary>What a module actually is, which decides how the player renders it.</summary>
public enum ContentModuleKind
{
    /// <summary>Rich text authored in-place — no file, no hosting question.</summary>
    Text = 0,
    /// <summary>A document the learner opens (PDF, slides) — small enough to hold inline.</summary>
    Document = 1,
    /// <summary>Video held OUTSIDE this database and referenced by URL. See the entity remarks.</summary>
    Video = 2,
    /// <summary>Any other external resource — a provider's course page, an article.</summary>
    Link = 3
}

/// <summary>
/// A versioned snapshot of a course's content.
///
/// <para>⚠️ VERSIONING IS THE POINT, not bookkeeping. A learner who completed v1 of a compliance
/// course must not silently read as complete against v3: progress is recorded against the version
/// it was earned on, so "who is current on the latest revision?" stays answerable. Editing a
/// published course in place — which is what the module did before — makes that question
/// unanswerable for ever (logic §12.86).</para>
/// </summary>
public class CourseVersion : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid TrainingCourseId { get; private set; }
    /// <summary>1, 2, 3 … unique per course.</summary>
    public int VersionNumber { get; private set; }
    public CourseVersionStatus Status { get; private set; } = CourseVersionStatus.Draft;
    /// <summary>What changed in this revision — shown to learners asked to retake it.</summary>
    public string? ChangeNote { get; private set; }
    public DateTime? PublishedOn { get; private set; }
    public DateTime? RetiredOn { get; private set; }

    private readonly List<ContentModule> _modules = [];
    public IReadOnlyCollection<ContentModule> Modules => _modules;

    private CourseVersion() : base() { }

    public static CourseVersion Create(Guid trainingCourseId, int versionNumber, string? changeNote = null)
    {
        if (trainingCourseId == Guid.Empty)
            throw new ArgumentException("A course is required.", nameof(trainingCourseId));
        if (versionNumber < 1)
            throw new ArgumentException("Version numbers start at 1.", nameof(versionNumber));

        return new CourseVersion
        {
            TrainingCourseId = trainingCourseId,
            VersionNumber = versionNumber,
            ChangeNote = changeNote
        };
    }

    public void UpdateNote(string? changeNote)
    {
        EnsureDraft();
        ChangeNote = changeNote;
        base.Update();
    }

    /// <summary>
    /// Makes the version live.
    ///
    /// <para>⚠️ Refuses an EMPTY version. A published course with no modules would show a learner a
    /// player with nothing in it and — worse — complete instantly, since "all required modules done"
    /// is vacuously true of none.</para>
    /// </summary>
    public void Publish()
    {
        EnsureDraft();
        if (_modules.Count == 0)
            throw new InvalidOperationException("A version needs at least one module before it can be published.");
        Status = CourseVersionStatus.Published;
        PublishedOn = DateTime.UtcNow;
        base.Update();
    }

    /// <summary>Supersedes the version. Progress already recorded against it stays valid.</summary>
    public void Retire()
    {
        if (Status != CourseVersionStatus.Published)
            throw new InvalidOperationException($"Only a published version can be retired (current: {Status}).");
        Status = CourseVersionStatus.Retired;
        RetiredOn = DateTime.UtcNow;
        base.Update();
    }

    /// <summary>Replaces the ordered module list — draft only.</summary>
    public void SetModules(IEnumerable<ContentModuleSpec> specs)
    {
        EnsureDraft();
        _modules.Clear();
        var order = 1;
        foreach (var spec in specs)
            _modules.Add(ContentModule.Create(Id, order++, spec));
        base.Update();
    }

    /// <summary>
    /// Content is frozen once published, and this is the guard that makes versioning mean anything.
    /// Without it a "version" is just a label on a record that still changes underneath the people
    /// who completed it.
    /// </summary>
    private void EnsureDraft()
    {
        if (Status != CourseVersionStatus.Draft)
            throw new InvalidOperationException(
                $"A {Status} version cannot be edited — create a new version instead.");
    }
}

/// <summary>One module as posted from the authoring screen.</summary>
public record ContentModuleSpec(
    string Title,
    ContentModuleKind Kind,
    string? Body,
    string? ExternalUrl,
    Guid? DocumentId,
    int? EstimatedMinutes,
    bool IsRequired);

/// <summary>
/// One step of a course version — a page of text, a document to read, a video to watch, a link to
/// follow.
///
/// <para>⚠️ CONTENT IS REFERENCED, NOT NECESSARILY STORED. <see cref="DocumentId"/> points at an
/// <see cref="EmployeeDocument"/>, whose bytes live inline in SQL Server — fine for a PDF or a deck,
/// and the mechanism the product already uses. VIDEO is deliberately a <see cref="ExternalUrl"/>
/// only: course video inline in SQL Server would not scale, and object storage is an infrastructure
/// decision this product has not taken. Keeping the reference in the model means that decision can
/// be made later without reshaping content — a hosted-asset kind becomes an additional case, not a
/// migration (logic §12.86).</para>
/// </summary>
public class ContentModule : BaseEntity
{
    public Guid CourseVersionId { get; private set; }
    public int SortOrder { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public ContentModuleKind Kind { get; private set; }
    /// <summary>Authored rich text, for a Text module.</summary>
    public string? Body { get; private set; }
    /// <summary>Where a Video or Link module points.</summary>
    public string? ExternalUrl { get; private set; }
    /// <summary>The attached file, for a Document module.</summary>
    public Guid? DocumentId { get; private set; }
    /// <summary>Guides the learner and drives the version's total duration. Not enforced.</summary>
    public int? EstimatedMinutes { get; private set; }
    /// <summary>
    /// Optional modules are offered but do not gate completion — "all REQUIRED modules done" is what
    /// completes an enrolment.
    /// </summary>
    public bool IsRequired { get; private set; } = true;

    private ContentModule() : base() { }

    internal static ContentModule Create(Guid courseVersionId, int sortOrder, ContentModuleSpec spec)
    {
        if (string.IsNullOrWhiteSpace(spec.Title))
            throw new ArgumentException("A module needs a title.", nameof(spec));

        // Each kind has exactly one place its content can come from; a module whose content is
        // missing renders as an empty page the learner cannot complete.
        switch (spec.Kind)
        {
            case ContentModuleKind.Text when string.IsNullOrWhiteSpace(spec.Body):
                throw new ArgumentException($"'{spec.Title}' is a text module but has no content.", nameof(spec));
            case ContentModuleKind.Document when spec.DocumentId is null || spec.DocumentId == Guid.Empty:
                throw new ArgumentException($"'{spec.Title}' is a document module but no file is attached.", nameof(spec));
            case ContentModuleKind.Video or ContentModuleKind.Link when string.IsNullOrWhiteSpace(spec.ExternalUrl):
                throw new ArgumentException($"'{spec.Title}' needs a URL.", nameof(spec));
        }

        return new ContentModule
        {
            CourseVersionId = courseVersionId,
            SortOrder = sortOrder,
            Title = spec.Title.Trim(),
            Kind = spec.Kind,
            Body = spec.Body,
            ExternalUrl = spec.ExternalUrl?.Trim(),
            DocumentId = spec.DocumentId,
            EstimatedMinutes = spec.EstimatedMinutes,
            IsRequired = spec.IsRequired
        };
    }
}

/// <summary>
/// One learner's progress through one module — the record that replaces "HR types in whether they
/// finished".
///
/// <para>Keyed on the ENROLMENT rather than the employee, so a person who takes the same course
/// twice has two independent records, and progress is automatically scoped to the version the
/// enrolment is working through.</para>
/// </summary>
public class ModuleProgress : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid TrainingEnrollmentId { get; private set; }
    public Guid ContentModuleId { get; private set; }
    public DateTime StartedOn { get; private set; }
    public DateTime? CompletedOn { get; private set; }
    /// <summary>Seconds spent, accumulated across visits — evidence behind a completion claim.</summary>
    public int SecondsSpent { get; private set; }
    /// <summary>Where to resume: a video offset in seconds, a page number, whatever the kind means.</summary>
    public int? LastPosition { get; private set; }

    public bool IsComplete => CompletedOn.HasValue;

    private ModuleProgress() : base() { }

    public static ModuleProgress Start(Guid trainingEnrollmentId, Guid contentModuleId)
    {
        if (trainingEnrollmentId == Guid.Empty)
            throw new ArgumentException("An enrolment is required.", nameof(trainingEnrollmentId));
        if (contentModuleId == Guid.Empty)
            throw new ArgumentException("A module is required.", nameof(contentModuleId));

        return new ModuleProgress
        {
            TrainingEnrollmentId = trainingEnrollmentId,
            ContentModuleId = contentModuleId,
            StartedOn = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Records a visit. Time ACCUMULATES and completion is one-way — a learner reopening a finished
    /// module to look something up must not un-complete it, and must not lose the time already
    /// credited.
    /// </summary>
    public void Record(int addSeconds, int? lastPosition, bool complete)
    {
        if (addSeconds < 0)
            throw new ArgumentException("Time cannot be negative.", nameof(addSeconds));

        SecondsSpent += addSeconds;
        if (lastPosition.HasValue) LastPosition = lastPosition;
        if (complete && !CompletedOn.HasValue) CompletedOn = DateTime.UtcNow;
        base.Update();
    }
}
