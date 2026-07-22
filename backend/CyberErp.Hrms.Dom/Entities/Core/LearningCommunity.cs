using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>What a community gathers around (HC199/HC208): learning, shared interests or a club.</summary>
public enum CommunityKind
{
    Learning = 0,
    InterestGroup = 1,
    Club = 2
}

/// <summary>
/// A community (HC199/HC208): learning circle, interest group or club for peer exchange, optionally
/// anchored to a catalog course and tagged for navigation (HC206-b). Any employee can found one;
/// the founder moderates.
/// </summary>
public class LearningCommunity : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public CommunityKind Kind { get; private set; } = CommunityKind.Learning;
    /// <summary>Comma-separated tags for navigation / search (HC206-b).</summary>
    public string? Tags { get; private set; }
    /// <summary>Optional course the community gathers around.</summary>
    public Guid? TrainingCourseId { get; private set; }
    public bool IsActive { get; private set; } = true;
    /// <summary>The founder — audit snapshot, intentionally no FK.</summary>
    public Guid? CreatedByEmployeeId { get; private set; }

    private LearningCommunity() : base() { }

    public static LearningCommunity Create(string name, string? description, Guid? trainingCourseId,
        Guid? createdByEmployeeId, bool isActive = true,
        CommunityKind kind = CommunityKind.Learning, string? tags = null)
    {
        Guard(name);
        return new LearningCommunity
        {
            Name = name,
            Description = description,
            Kind = kind,
            Tags = NormalizeTags(tags),
            TrainingCourseId = trainingCourseId,
            CreatedByEmployeeId = createdByEmployeeId,
            IsActive = isActive
        };
    }

    public void Update(string name, string? description, Guid? trainingCourseId, bool isActive,
        CommunityKind kind = CommunityKind.Learning, string? tags = null)
    {
        Guard(name);
        Name = name;
        Description = description;
        Kind = kind;
        Tags = NormalizeTags(tags);
        TrainingCourseId = trainingCourseId;
        IsActive = isActive;
        base.Update();
    }

    private static string? NormalizeTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags)) return null;
        var parts = tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => t.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        return parts.Count == 0 ? null : string.Join(",", parts);
    }

    private static void Guard(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Community name cannot be empty.", nameof(name));
    }
}

/// <summary>
/// One employee's reaction (a "like") on a post (HC207-b) — at most one per employee per post;
/// reacting again toggles it off.
/// </summary>
public class CommunityPostReaction : BaseEntity
{
    public Guid LearningCommunityPostId { get; private set; }
    public Guid EmployeeId { get; private set; }

    private CommunityPostReaction() : base() { }

    public static CommunityPostReaction Create(Guid postId, Guid employeeId)
    {
        if (postId == Guid.Empty)
            throw new ArgumentException("A post is required.", nameof(postId));
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        return new CommunityPostReaction
        {
            LearningCommunityPostId = postId,
            EmployeeId = employeeId
        };
    }
}

/// <summary>One employee's membership in a community (HC199). The founder is a moderator.</summary>
public class LearningCommunityMember : BaseEntity
{
    public Guid LearningCommunityId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public bool IsModerator { get; private set; }
    public DateTime JoinedOn { get; private set; }

    private LearningCommunityMember() : base() { }

    public static LearningCommunityMember Create(Guid learningCommunityId, Guid employeeId,
        DateTime joinedOn, bool isModerator = false)
    {
        if (learningCommunityId == Guid.Empty)
            throw new ArgumentException("A community is required.", nameof(learningCommunityId));
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        return new LearningCommunityMember
        {
            LearningCommunityId = learningCommunityId,
            EmployeeId = employeeId,
            JoinedOn = joinedOn,
            IsModerator = isModerator
        };
    }
}

/// <summary>
/// A discussion post (HC198): top-level topic or a single-level reply. Reading is open to every
/// employee; posting needs membership. ParentPostId is intentionally NOT a foreign key so the
/// community's cascade delete never fights the self-reference — reply cleanup is handler-driven.
/// </summary>
public class LearningCommunityPost : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid LearningCommunityId { get; private set; }
    public Guid EmployeeId { get; private set; }
    /// <summary>Null = a topic; set = a reply to that topic (one level of threading).</summary>
    public Guid? ParentPostId { get; private set; }
    public string Content { get; private set; } = string.Empty;

    private LearningCommunityPost() : base() { }

    public static LearningCommunityPost Create(Guid learningCommunityId, Guid employeeId,
        string content, Guid? parentPostId = null)
    {
        if (learningCommunityId == Guid.Empty)
            throw new ArgumentException("A community is required.", nameof(learningCommunityId));
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An author is required.", nameof(employeeId));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Post content cannot be empty.", nameof(content));
        return new LearningCommunityPost
        {
            LearningCommunityId = learningCommunityId,
            EmployeeId = employeeId,
            Content = content,
            ParentPostId = parentPostId
        };
    }
}
