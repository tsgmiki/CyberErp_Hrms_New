using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Lifecycle of a survey / poll (HC204).</summary>
public enum SurveyStatus
{
    Draft = 0,
    Open = 1,
    Closed = 2
}

/// <summary>
/// An employee survey, questionnaire or quick poll (HC204). Questions live as a JSON document
/// (rating / choice / text — the dynamic-form pattern, not EAV); a poll is a single-question survey.
/// Anonymous surveys collect answers with NO employee link (HC207 discipline).
/// </summary>
public class Survey : BaseEntity, IAggregateRoot, IAuditable
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    /// <summary>A quick poll — exactly one question.</summary>
    public bool IsPoll { get; private set; }
    /// <summary>Responses carry no employee link; completion is tracked separately (HC207).</summary>
    public bool IsAnonymous { get; private set; }
    /// <summary>JSON array of {key, text, type, options[], required}.</summary>
    public string QuestionsJson { get; private set; } = "[]";
    public SurveyStatus Status { get; private set; } = SurveyStatus.Draft;
    public DateTime? OpensOn { get; private set; }
    public DateTime? ClosesOn { get; private set; }

    private Survey() : base() { }

    public static Survey Create(string title, string? description, bool isPoll, bool isAnonymous,
        string questionsJson, DateTime? opensOn, DateTime? closesOn)
    {
        Guard(title, questionsJson, opensOn, closesOn);
        return new Survey
        {
            Title = title,
            Description = description,
            IsPoll = isPoll,
            IsAnonymous = isAnonymous,
            QuestionsJson = questionsJson,
            OpensOn = opensOn,
            ClosesOn = closesOn
        };
    }

    /// <summary>Editable while DRAFT only — an open survey's questions are frozen.</summary>
    public void UpdateDraft(string title, string? description, bool isPoll, bool isAnonymous,
        string questionsJson, DateTime? opensOn, DateTime? closesOn)
    {
        if (Status != SurveyStatus.Draft)
            throw new InvalidOperationException($"Only a draft survey can be edited (current: {Status}).");
        Guard(title, questionsJson, opensOn, closesOn);
        Title = title;
        Description = description;
        IsPoll = isPoll;
        IsAnonymous = isAnonymous;
        QuestionsJson = questionsJson;
        OpensOn = opensOn;
        ClosesOn = closesOn;
        base.Update();
    }

    public void Open()
    {
        if (Status != SurveyStatus.Draft)
            throw new InvalidOperationException($"Only a draft survey can open (current: {Status}).");
        Status = SurveyStatus.Open;
        base.Update();
    }

    public void Close()
    {
        if (Status != SurveyStatus.Open)
            throw new InvalidOperationException($"Only an open survey can close (current: {Status}).");
        Status = SurveyStatus.Closed;
        base.Update();
    }

    /// <summary>Open AND inside the publish window.</summary>
    public bool AcceptsResponsesOn(DateTime day) =>
        Status == SurveyStatus.Open
        && (OpensOn == null || OpensOn <= day)
        && (ClosesOn == null || ClosesOn >= day);

    private static void Guard(string title, string questionsJson, DateTime? opensOn, DateTime? closesOn)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("A title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(questionsJson))
            throw new ArgumentException("Questions are required.", nameof(questionsJson));
        if (opensOn.HasValue && closesOn.HasValue && closesOn.Value < opensOn.Value)
            throw new ArgumentException("The window cannot close before it opens.", nameof(closesOn));
    }
}

/// <summary>
/// One submitted answer set (HC204). For anonymous surveys the row carries NO employee id, gets its
/// CreatedBy stamp cleared, and the entity is not IAuditable — the same discipline as suggestions.
/// The one-response guard lives on <see cref="SurveyCompletion"/> instead, so answers stay unlinkable.
/// </summary>
public class SurveyResponse : BaseEntity, IAggregateRoot
{
    public Guid SurveyId { get; private set; }
    /// <summary>Null on anonymous surveys — by design nothing to join back to.</summary>
    public Guid? EmployeeId { get; private set; }
    /// <summary>JSON object: question key → answer value.</summary>
    public string AnswersJson { get; private set; } = "{}";
    public DateTime SubmittedOn { get; private set; }

    private SurveyResponse() : base() { }

    public static SurveyResponse Create(Guid surveyId, Guid? employeeId, string answersJson, DateTime submittedOn)
    {
        if (surveyId == Guid.Empty)
            throw new ArgumentException("A survey is required.", nameof(surveyId));
        if (string.IsNullOrWhiteSpace(answersJson))
            throw new ArgumentException("Answers are required.", nameof(answersJson));
        return new SurveyResponse
        {
            SurveyId = surveyId,
            EmployeeId = employeeId,
            AnswersJson = answersJson,
            SubmittedOn = submittedOn
        };
    }
}

/// <summary>
/// Marks that an employee completed a survey — the one-response-per-employee guard. Kept apart from
/// the answers so anonymous responses stay unlinkable while double voting is still impossible.
/// </summary>
public class SurveyCompletion : BaseEntity
{
    public Guid SurveyId { get; private set; }
    public Guid EmployeeId { get; private set; }

    private SurveyCompletion() : base() { }

    public static SurveyCompletion Create(Guid surveyId, Guid employeeId)
    {
        if (surveyId == Guid.Empty)
            throw new ArgumentException("A survey is required.", nameof(surveyId));
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        return new SurveyCompletion
        {
            SurveyId = surveyId,
            EmployeeId = employeeId
        };
    }
}
