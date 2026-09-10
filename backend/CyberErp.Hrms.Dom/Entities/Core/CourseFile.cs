namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// A file belonging to a COURSE — the material a <see cref="ContentModuleKind.Document"/> module
/// serves.
///
/// <para>⚠️ THIS EXISTS BECAUSE <see cref="EmployeeDocument"/> COULD NOT DO IT. That table is scoped
/// to one employee, so a course handbook stored there is readable by exactly one person: the wrong
/// answer for material every learner on the course must open. Phase 3 left the Document kind
/// unreachable rather than ship a module nobody could read (logic §12.89).</para>
///
/// <para>⚠️ IMMUTABLE ONCE CREATED. There is no method to change the bytes, and that is what lets a
/// published course version stay frozen: its Document module points at a row that can never move
/// underneath the people who completed it. Replacing material means uploading a new file and
/// repointing the DRAFT — the same shape as every other revision in this module.</para>
///
/// <para>⚠️ Owned by the COURSE, not by a version. A revision that keeps the same handbook references
/// the same row rather than a copy of it, so the fifth revision of a course does not carry five
/// identical PDFs.</para>
///
/// <para>Bytes live inline, as they do for every other attachment in this product. That suits
/// documents — a 5 MB procedure read by three hundred people is still one 5 MB row, which is a
/// better profile than the per-employee tables already in use. VIDEO IS STILL A URL and deliberately
/// so: streaming media from SQL Server does not scale, and object storage remains an infrastructure
/// decision this product has not taken.</para>
/// </summary>
public class CourseFile : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid TrainingCourseId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = "application/octet-stream";
    public long FileSize { get; private set; }
    public byte[] Content { get; private set; } = [];
    /// <summary>What this file is, for an author picking from a list of a dozen PDFs.</summary>
    public string? Description { get; private set; }

    private CourseFile() : base() { }

    public static CourseFile Create(Guid trainingCourseId, string fileName, string contentType,
        byte[] content, string? description)
    {
        if (trainingCourseId == Guid.Empty)
            throw new ArgumentException("A course is required.", nameof(trainingCourseId));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("A file name is required.", nameof(fileName));
        if (content is null || content.Length == 0)
            throw new ArgumentException("The file is empty.", nameof(content));

        return new CourseFile
        {
            TrainingCourseId = trainingCourseId,
            // Path.GetFileName is applied by the handler; this is the last line of defence against a
            // name carrying a directory, which would make the download's filename a path traversal.
            FileName = System.IO.Path.GetFileName(fileName.Trim()),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            FileSize = content.LongLength,
            Content = content,
            Description = description
        };
    }

    /// <summary>
    /// Renames the file or retitles its description. The BYTES are deliberately not touchable — see
    /// the type remarks.
    /// </summary>
    public void Describe(string? description)
    {
        Description = description;
        base.Update();
    }
}
