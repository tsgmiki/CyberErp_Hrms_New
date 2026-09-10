namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// What a question asks, which decides how it is graded.
///
/// <para>⚠️ EVERY KIND HERE IS AUTO-GRADABLE, and that is the boundary of this phase. Free text and
/// file upload are deliberately absent: they need a human grader, which means a grading queue, a
/// grader role, and a "submitted, awaiting marking" state on the attempt. Adding the enum value
/// without those would produce attempts that can never finish (logic §12.87).</para>
/// </summary>
public enum QuestionKind
{
    /// <summary>One correct option out of several.</summary>
    SingleChoice = 0,
    /// <summary>Several correct options — graded all-or-nothing. See <see cref="Assessment"/>.</summary>
    MultipleChoice = 1,
    /// <summary>Two options, True and False. A SingleChoice with a fixed option set.</summary>
    TrueFalse = 2
}

/// <summary>Where an attempt has got to.</summary>
public enum AttemptStatus
{
    /// <summary>Started and not yet submitted. At most one of these per (enrolment, assessment).</summary>
    InProgress = 0,
    /// <summary>Submitted and graded.</summary>
    Submitted = 1,
    /// <summary>Submitted after the time limit. Graded exactly like a Submitted attempt — see remarks.</summary>
    TimedOut = 2
}

/// <summary>
/// A reusable library of questions, so a compliance question written once can be used by every
/// course that needs it.
///
/// <para>⚠️ A bank's questions are TEMPLATES, and importing COPIES them into an assessment. Editing
/// a bank therefore never changes an assessment that already exists — which is the only behaviour
/// compatible with frozen published content: a learner who passed a quiz must be able to be shown
/// the exact questions they answered, years later (logic §12.87).</para>
/// </summary>
public class QuestionBank : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private QuestionBank() : base() { }

    public static QuestionBank Create(string name, string? description, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A bank needs a name.", nameof(name));
        return new QuestionBank { Name = name.Trim(), Description = description, IsActive = isActive };
    }

    public void Update(string name, string? description, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A bank needs a name.", nameof(name));
        Name = name.Trim();
        Description = description;
        IsActive = isActive;
        base.Update();
    }
}

/// <summary>
/// The quiz inside a <see cref="ContentModule"/> of kind <see cref="ContentModuleKind.Quiz"/>.
///
/// <para>⚠️ IT HANGS OFF THE MODULE, NOT THE COURSE, and that single choice buys everything else.
/// A module belongs to a <see cref="CourseVersion"/>, which freezes on publish — so a published
/// assessment is frozen too, with no separate rule to write and no way for the two to disagree.
/// It also means a quiz takes its place in the running order like any other module, and gates
/// completion through the ordinary "all required modules done" rule rather than a parallel one.</para>
/// </summary>
public class Assessment : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid ContentModuleId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Instructions { get; private set; }
    /// <summary>Percentage needed to pass, 0–100.</summary>
    public decimal PassMark { get; private set; } = 70m;
    /// <summary>Null means unlimited. A passed assessment is closed regardless — see remarks.</summary>
    public int? MaxAttempts { get; private set; }
    /// <summary>Null means untimed. Advisory — see <see cref="AssessmentAttempt"/>.</summary>
    public int? TimeLimitMinutes { get; private set; }
    public bool ShuffleQuestions { get; private set; }
    /// <summary>
    /// Whether the learner sees which answers were right after submitting.
    ///
    /// <para>⚠️ Even when true, answers are revealed only once the attempt can no longer be
    /// improved — passed, or out of attempts. Showing the key after a failed attempt with retakes
    /// left hands the learner the answers to the retake, which makes the pass mark meaningless.</para>
    /// </summary>
    public bool RevealAnswers { get; private set; } = true;

    private readonly List<Question> _questions = [];
    public IReadOnlyCollection<Question> Questions => _questions;

    private Assessment() : base() { }

    public static Assessment Create(Guid contentModuleId, string title, string? instructions,
        decimal passMark, int? maxAttempts, int? timeLimitMinutes, bool shuffleQuestions, bool revealAnswers)
    {
        if (contentModuleId == Guid.Empty)
            throw new ArgumentException("A module is required.", nameof(contentModuleId));
        var a = new Assessment { ContentModuleId = contentModuleId };
        a.Apply(title, instructions, passMark, maxAttempts, timeLimitMinutes, shuffleQuestions, revealAnswers);
        return a;
    }

    public void Apply(string title, string? instructions, decimal passMark, int? maxAttempts,
        int? timeLimitMinutes, bool shuffleQuestions, bool revealAnswers)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("An assessment needs a title.", nameof(title));
        if (passMark is < 0 or > 100)
            throw new ArgumentException("The pass mark must be between 0 and 100.", nameof(passMark));
        if (maxAttempts is < 1)
            throw new ArgumentException("Allow at least one attempt, or leave it unlimited.", nameof(maxAttempts));
        if (timeLimitMinutes is < 1)
            throw new ArgumentException("A time limit must be at least a minute.", nameof(timeLimitMinutes));

        Title = title.Trim();
        Instructions = instructions;
        PassMark = passMark;
        MaxAttempts = maxAttempts;
        TimeLimitMinutes = timeLimitMinutes;
        ShuffleQuestions = shuffleQuestions;
        RevealAnswers = revealAnswers;
        base.Update();
    }

    /// <summary>Replaces the ordered question list.</summary>
    public void SetQuestions(IEnumerable<QuestionSpec> specs)
    {
        _questions.Clear();
        var order = 1;
        foreach (var spec in specs)
            _questions.Add(Question.CreateForAssessment(Id, order++, spec));
        base.Update();
    }

    /// <summary>Total marks available — the denominator every score is a percentage of.</summary>
    public decimal TotalPoints => _questions.Sum(q => q.Points);
}

/// <summary>One question as posted from the authoring screen.</summary>
public record QuestionSpec(
    string Text,
    QuestionKind Kind,
    decimal Points,
    string? Explanation,
    IReadOnlyList<QuestionOptionSpec> Options);

/// <summary>One answer option as posted from the authoring screen.</summary>
public record QuestionOptionSpec(string Text, bool IsCorrect);

/// <summary>
/// A question, owned by EITHER a bank (a reusable template) or an assessment (a frozen copy).
///
/// <para>⚠️ Exactly one owner is set, enforced in the factories. One table rather than two because
/// a bank question and an assessment question are the same thing at different moments — a second
/// table would duplicate the option model and every validation rule with it.</para>
/// </summary>
public class Question : BaseEntity, IAggregateRoot
{
    /// <summary>Set when this is a reusable template in a bank.</summary>
    public Guid? QuestionBankId { get; private set; }
    /// <summary>Set when this is a copy living inside an assessment.</summary>
    public Guid? AssessmentId { get; private set; }
    public int SortOrder { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public QuestionKind Kind { get; private set; }
    /// <summary>Marks this question is worth. Weighting a hard question higher is the point.</summary>
    public decimal Points { get; private set; } = 1m;
    /// <summary>Shown with the answer key, when the assessment reveals it. Why, not just what.</summary>
    public string? Explanation { get; private set; }

    private readonly List<QuestionOption> _options = [];
    public IReadOnlyCollection<QuestionOption> Options => _options;

    private Question() : base() { }

    public static Question CreateForBank(Guid questionBankId, int sortOrder, QuestionSpec spec)
    {
        if (questionBankId == Guid.Empty)
            throw new ArgumentException("A bank is required.", nameof(questionBankId));
        var q = Build(sortOrder, spec);
        q.QuestionBankId = questionBankId;
        return q;
    }

    internal static Question CreateForAssessment(Guid assessmentId, int sortOrder, QuestionSpec spec)
    {
        var q = Build(sortOrder, spec);
        q.AssessmentId = assessmentId;
        return q;
    }

    /// <summary>Copies a bank question into an assessment — the import path.</summary>
    public QuestionSpec ToSpec() => new(Text, Kind, Points, Explanation,
        [.. _options.OrderBy(o => o.SortOrder).Select(o => new QuestionOptionSpec(o.Text, o.IsCorrect))]);

    public void Apply(int sortOrder, QuestionSpec spec)
    {
        var rebuilt = Build(sortOrder, spec);
        SortOrder = rebuilt.SortOrder;
        Text = rebuilt.Text;
        Kind = rebuilt.Kind;
        Points = rebuilt.Points;
        Explanation = rebuilt.Explanation;
        _options.Clear();
        _options.AddRange(rebuilt.Options);
        base.Update();
    }

    private static Question Build(int sortOrder, QuestionSpec spec)
    {
        if (string.IsNullOrWhiteSpace(spec.Text))
            throw new ArgumentException("A question needs text.", nameof(spec));
        if (spec.Points <= 0)
            throw new ArgumentException($"'{Trim(spec.Text)}' must be worth more than zero marks.", nameof(spec));

        var options = spec.Options ?? [];
        if (options.Count < 2)
            throw new ArgumentException($"'{Trim(spec.Text)}' needs at least two options.", nameof(spec));
        if (options.Any(o => string.IsNullOrWhiteSpace(o.Text)))
            throw new ArgumentException($"'{Trim(spec.Text)}' has an option with no text.", nameof(spec));

        // A question nobody can get right is the failure mode that survives review, because the
        // authoring screen looks finished. Caught here, once, for every path in.
        var correct = options.Count(o => o.IsCorrect);
        if (correct == 0)
            throw new ArgumentException($"'{Trim(spec.Text)}' has no correct answer.", nameof(spec));

        switch (spec.Kind)
        {
            case QuestionKind.SingleChoice when correct > 1:
                throw new ArgumentException(
                    $"'{Trim(spec.Text)}' is single-choice but has {correct} correct answers.", nameof(spec));
            case QuestionKind.TrueFalse when options.Count != 2:
                throw new ArgumentException(
                    $"'{Trim(spec.Text)}' is true/false and must have exactly two options.", nameof(spec));
            case QuestionKind.TrueFalse when correct != 1:
                throw new ArgumentException(
                    $"'{Trim(spec.Text)}' is true/false and must have exactly one correct answer.", nameof(spec));
            // Multiple-choice with a single correct answer is allowed: "select all that apply" where
            // only one does is a legitimate — and deliberately tricky — question.
        }

        var q = new Question
        {
            SortOrder = sortOrder,
            Text = spec.Text.Trim(),
            Kind = spec.Kind,
            Points = spec.Points,
            Explanation = spec.Explanation
        };
        var order = 1;
        foreach (var o in options)
            q._options.Add(QuestionOption.Create(q.Id, order++, o.Text, o.IsCorrect));
        return q;
    }

    private static string Trim(string text) => text.Length <= 40 ? text : text[..40] + "…";
}

/// <summary>One answer option. <see cref="IsCorrect"/> is the answer key and never leaves the server
/// for a learner — see the attempt handlers.</summary>
public class QuestionOption : BaseEntity
{
    public Guid QuestionId { get; private set; }
    public int SortOrder { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public bool IsCorrect { get; private set; }

    private QuestionOption() : base() { }

    internal static QuestionOption Create(Guid questionId, int sortOrder, string text, bool isCorrect) =>
        new() { QuestionId = questionId, SortOrder = sortOrder, Text = text.Trim(), IsCorrect = isCorrect };
}

/// <summary>
/// One learner's run at one assessment.
///
/// <para>⚠️ GRADING HAPPENS HERE, ON THE SERVER, and the client is never sent the answer key. That
/// is the whole security property of this phase: a quiz whose correct answers are in the page is a
/// quiz that certifies nothing.</para>
///
/// <para>⚠️ The time limit is ADVISORY. The clock runs in the browser and the server records the
/// elapsed time, marking a late submission <see cref="AttemptStatus.TimedOut"/> — but it still
/// grades the answers given. Discarding work because a network stalled would punish the wrong
/// person, and a server that hard-rejects cannot tell the two cases apart.</para>
/// </summary>
public class AssessmentAttempt : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid AssessmentId { get; private set; }
    public Guid TrainingEnrollmentId { get; private set; }
    /// <summary>1, 2, 3 … per (enrolment, assessment). What the retake limit counts.</summary>
    public int AttemptNumber { get; private set; }
    public AttemptStatus Status { get; private set; } = AttemptStatus.InProgress;
    public DateTime StartedOn { get; private set; }
    public DateTime? SubmittedOn { get; private set; }
    public decimal? PointsAwarded { get; private set; }
    public decimal? PointsPossible { get; private set; }
    /// <summary>0–100. What lands on the enrolment when this is the best attempt.</summary>
    public decimal? ScorePercent { get; private set; }
    public bool? Passed { get; private set; }

    private readonly List<AttemptAnswer> _answers = [];
    public IReadOnlyCollection<AttemptAnswer> Answers => _answers;

    private AssessmentAttempt() : base() { }

    public static AssessmentAttempt Start(Guid assessmentId, Guid trainingEnrollmentId, int attemptNumber)
    {
        if (assessmentId == Guid.Empty)
            throw new ArgumentException("An assessment is required.", nameof(assessmentId));
        if (trainingEnrollmentId == Guid.Empty)
            throw new ArgumentException("An enrolment is required.", nameof(trainingEnrollmentId));
        if (attemptNumber < 1)
            throw new ArgumentException("Attempts are numbered from 1.", nameof(attemptNumber));

        return new AssessmentAttempt
        {
            AssessmentId = assessmentId,
            TrainingEnrollmentId = trainingEnrollmentId,
            AttemptNumber = attemptNumber,
            StartedOn = DateTime.UtcNow
        };
    }

    /// <summary>Whether the browser clock overran, given the assessment's limit.</summary>
    public bool IsOverdue(int? timeLimitMinutes, DateTime now) =>
        timeLimitMinutes.HasValue && now > StartedOn.AddMinutes(timeLimitMinutes.Value + 1);

    /// <summary>
    /// Records the graded result. Called once — an attempt is submitted, never re-submitted, so a
    /// learner cannot walk a score upwards inside one attempt.
    /// </summary>
    public void Submit(IEnumerable<AttemptAnswer> answers, decimal pointsAwarded, decimal pointsPossible,
        decimal passMark, bool timedOut)
    {
        if (Status != AttemptStatus.InProgress)
            throw new InvalidOperationException($"This attempt is already {Status}.");

        _answers.Clear();
        _answers.AddRange(answers);
        PointsAwarded = pointsAwarded;
        PointsPossible = pointsPossible;
        // An assessment with no marks available scores zero rather than dividing by zero. It cannot
        // be published empty, so this only guards a malformed import.
        ScorePercent = pointsPossible <= 0 ? 0m : Math.Round(pointsAwarded * 100m / pointsPossible, 2);
        Passed = ScorePercent >= passMark;
        SubmittedOn = DateTime.UtcNow;
        Status = timedOut ? AttemptStatus.TimedOut : AttemptStatus.Submitted;
        base.Update();
    }
}

/// <summary>One question's worth of one attempt — what was chosen and what it earned.</summary>
public class AttemptAnswer : BaseEntity
{
    public Guid AssessmentAttemptId { get; private set; }
    public Guid QuestionId { get; private set; }
    public bool IsCorrect { get; private set; }
    public decimal PointsAwarded { get; private set; }

    private readonly List<AttemptAnswerOption> _selected = [];
    public IReadOnlyCollection<AttemptAnswerOption> Selected => _selected;

    private AttemptAnswer() : base() { }

    /// <summary>
    /// Built by the grader, which lives in the application layer because deciding whether an answer
    /// is right needs the question's options — and an attempt does not hold its questions.
    /// </summary>
    public static AttemptAnswer Create(Guid attemptId, Guid questionId, IEnumerable<Guid> selectedOptionIds,
        bool isCorrect, decimal pointsAwarded)
    {
        var a = new AttemptAnswer
        {
            AssessmentAttemptId = attemptId,
            QuestionId = questionId,
            IsCorrect = isCorrect,
            PointsAwarded = pointsAwarded
        };
        foreach (var id in selectedOptionIds.Distinct())
            a._selected.Add(AttemptAnswerOption.Create(a.Id, id));
        return a;
    }
}

/// <summary>
/// One option the learner ticked.
///
/// <para>A row per selection rather than a list of ids in a column: "which wrong answer do people
/// pick?" is the first question anyone asks of quiz data, and a delimited column cannot answer it.</para>
/// </summary>
public class AttemptAnswerOption : BaseEntity
{
    public Guid AttemptAnswerId { get; private set; }
    public Guid QuestionOptionId { get; private set; }

    private AttemptAnswerOption() : base() { }

    internal static AttemptAnswerOption Create(Guid attemptAnswerId, Guid questionOptionId) =>
        new() { AttemptAnswerId = attemptAnswerId, QuestionOptionId = questionOptionId };
}
