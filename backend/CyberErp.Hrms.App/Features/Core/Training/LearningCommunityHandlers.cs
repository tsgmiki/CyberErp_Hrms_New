using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    // ---- DTOs ---------------------------------------------------------------
    public class LearningCommunityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        /// <summary>Learning | InterestGroup | Club (HC208).</summary>
        public string Kind { get; set; } = string.Empty;
        public string? Tags { get; set; }
        public Guid? TrainingCourseId { get; set; }
        public string? CourseName { get; set; }
        public bool IsActive { get; set; }
        public Guid? CreatedByEmployeeId { get; set; }
        public string? CreatedByName { get; set; }
        public int MemberCount { get; set; }
        public int PostCount { get; set; }
        /// <summary>Whether the CALLER belongs to / moderates the community.</summary>
        public bool IsMember { get; set; }
        public bool IsModerator { get; set; }
    }

    public class SaveLearningCommunityDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        /// <summary>Learning | InterestGroup | Club.</summary>
        public string Kind { get; set; } = nameof(CommunityKind.Learning);
        /// <summary>Comma-separated tags (HC206-b).</summary>
        public string? Tags { get; set; }
        public Guid? TrainingCourseId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CommunityPostDto
    {
        public Guid Id { get; set; }
        public Guid LearningCommunityId { get; set; }
        public Guid EmployeeId { get; set; }
        public string? AuthorName { get; set; }
        public Guid? ParentPostId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; }
        public int LikeCount { get; set; }
        public bool LikedByMe { get; set; }
        public List<CommunityPostDto> Replies { get; set; } = [];
    }

    public class CommunityAnalyticsDto
    {
        public int Communities { get; set; }
        public int TotalMembers { get; set; }
        public int PostsLast30Days { get; set; }
        public int ActivePosters30Days { get; set; }
        public decimal ParticipationRatePercent { get; set; }
        public List<CommunityTopPosterDto> TopPosters { get; set; } = [];
        public List<CommunityTrendingTopicDto> TrendingTopics { get; set; } = [];
    }

    public class CommunityTopPosterDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public int Posts { get; set; }
    }

    public class CommunityTrendingTopicDto
    {
        public Guid PostId { get; set; }
        public string? CommunityName { get; set; }
        public string Excerpt { get; set; } = string.Empty;
        public int Replies { get; set; }
        public int Reactions { get; set; }
    }

    public class CreateCommunityPostDto
    {
        public string Content { get; set; } = string.Empty;
        public Guid? ParentPostId { get; set; }
    }

    public class SaveLearningCommunityDtoValidator : AbstractValidator<SaveLearningCommunityDto>
    {
        public SaveLearningCommunityDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(1000);
        }
    }

    public class CreateCommunityPostDtoValidator : AbstractValidator<CreateCommunityPostDto>
    {
        public CreateCommunityPostDtoValidator()
        {
            RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveLearningCommunity { Task<Guid> SaveAsync(SaveLearningCommunityDto dto); }
    public interface IDeleteLearningCommunity { Task DeleteAsync(Guid id); }
    public interface IJoinLearningCommunity { Task JoinAsync(Guid id); }
    public interface ILeaveLearningCommunity { Task LeaveAsync(Guid id); }
    public interface IGetAllLearningCommunities { Task<PaginatedResponse<LearningCommunityDto>> GetAsync(GetAllRequest request); }
    public interface IGetCommunityPosts { Task<PaginatedResponse<CommunityPostDto>> GetAsync(Guid communityId, GetAllRequest request); }
    public interface ICreateCommunityPost { Task<Guid> CreateAsync(Guid communityId, CreateCommunityPostDto dto); }
    public interface IDeleteCommunityPost { Task DeleteAsync(Guid postId); }
    /// <summary>HC207-b — toggles the caller's reaction; returns the new liked state.</summary>
    public interface IReactToCommunityPost { Task<bool> ToggleAsync(Guid postId); }
    /// <summary>HC208-b — forum engagement analytics for HR.</summary>
    public interface IGetCommunityAnalytics { Task<CommunityAnalyticsDto> GetAsync(); }

    internal static class LearningCommunityShared
    {
        internal static async Task<Guid> RequireEmployeeAsync(IPerformanceVisibilityService visibility)
        {
            var scope = await visibility.GetScopeAsync();
            return scope.EmployeeId
                ?? throw new ValidationException("employee", "Your account is not linked to an employee record.");
        }

        internal static async Task<bool> IsModeratorOrAdminAsync(
            IPerformanceVisibilityService visibility,
            IRepository<LearningCommunityMember> members,
            Guid communityId)
        {
            var scope = await visibility.GetScopeAsync();
            if (scope.IsAdmin) return true;
            if (!scope.EmployeeId.HasValue) return false;
            return await members.GetAll().AnyAsync(m => m.LearningCommunityId == communityId
                && m.EmployeeId == scope.EmployeeId.Value && m.IsModerator);
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveLearningCommunity(
        IRepository<LearningCommunity> repository,
        IRepository<LearningCommunityMember> memberRepository,
        IRepository<TrainingCourse> courseRepository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveLearningCommunityDto> validator,
        ILogger<SaveLearningCommunity> logger) : ISaveLearningCommunity
    {
        public async Task<Guid> SaveAsync(SaveLearningCommunityDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // HC199 — ANY employee can found a community; the founder moderates it.
            var myEmployeeId = await LearningCommunityShared.RequireEmployeeAsync(visibility);

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(LearningCommunity), nameof(dto.Name), dto.Name);
            if (dto.TrainingCourseId.HasValue &&
                !await courseRepository.GetAll().AnyAsync(c => c.Id == dto.TrainingCourseId.Value))
                throw new NotFoundException(nameof(TrainingCourse), dto.TrainingCourseId.Value.ToString());

            if (!Enum.TryParse<CommunityKind>(dto.Kind, true, out var kind))
                throw new ValidationException(nameof(dto.Kind), "Kind must be Learning, InterestGroup or Club.");
            if (dto.Tags?.Length > 300)
                throw new ValidationException(nameof(dto.Tags), "Tags are at most 300 characters.");

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(LearningCommunity), dto.Id.Value.ToString());
                if (!await LearningCommunityShared.IsModeratorOrAdminAsync(visibility, memberRepository, entity.Id))
                    throw new ValidationException(nameof(dto.Id), "Only a moderator or HR can edit the community.");

                entity.Update(dto.Name, dto.Description, dto.TrainingCourseId, dto.IsActive, kind, dto.Tags);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated LearningCommunity {Id}", entity.Id);
                return entity.Id;
            }

            var created = LearningCommunity.Create(dto.Name, dto.Description, dto.TrainingCourseId, myEmployeeId,
                dto.IsActive, kind, dto.Tags);
            await repository.AddAsync(created);
            var founder = LearningCommunityMember.Create(created.Id, myEmployeeId, DateTime.UtcNow.Date, isModerator: true);
            if (string.IsNullOrEmpty(founder.TenantId)) founder.TenantId = created.TenantId;
            await memberRepository.AddAsync(founder);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created LearningCommunity {Id} ({Name})", created.Id, created.Name);
            return created.Id;
        }
    }

    public class DeleteLearningCommunity(
        IRepository<LearningCommunity> repository,
        IRepository<LearningCommunityMember> memberRepository,
        IPerformanceVisibilityService visibility,
        ILogger<DeleteLearningCommunity> logger) : IDeleteLearningCommunity
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(LearningCommunity), id.ToString());
            if (!await LearningCommunityShared.IsModeratorOrAdminAsync(visibility, memberRepository, id))
                throw new ValidationException(nameof(id), "Only a moderator or HR can delete the community.");

            repository.Delete(entity); // members + posts cascade
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted LearningCommunity {Id}", id);
        }
    }

    public class JoinLearningCommunity(
        IRepository<LearningCommunity> repository,
        IRepository<LearningCommunityMember> memberRepository,
        IPerformanceVisibilityService visibility,
        ILogger<JoinLearningCommunity> logger) : IJoinLearningCommunity
    {
        public async Task JoinAsync(Guid id)
        {
            var myEmployeeId = await LearningCommunityShared.RequireEmployeeAsync(visibility);
            var community = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(LearningCommunity), id.ToString());
            if (!community.IsActive)
                throw new ValidationException(nameof(id), "The community is inactive.");
            if (await memberRepository.GetAll().AnyAsync(m => m.LearningCommunityId == id && m.EmployeeId == myEmployeeId))
                throw new ValidationException(nameof(id), "You are already a member.");

            await memberRepository.AddAsync(LearningCommunityMember.Create(id, myEmployeeId, DateTime.UtcNow.Date));
            await memberRepository.SaveChangesAsync();
            logger.LogInformation("Employee {Employee} joined community {Community}", myEmployeeId, id);
        }
    }

    public class LeaveLearningCommunity(
        IRepository<LearningCommunity> repository,
        IRepository<LearningCommunityMember> memberRepository,
        IPerformanceVisibilityService visibility,
        ILogger<LeaveLearningCommunity> logger) : ILeaveLearningCommunity
    {
        public async Task LeaveAsync(Guid id)
        {
            var myEmployeeId = await LearningCommunityShared.RequireEmployeeAsync(visibility);
            var founderId = await repository.GetAll().Where(x => x.Id == id)
                .Select(x => x.CreatedByEmployeeId).FirstOrDefaultAsync();
            if (founderId == myEmployeeId)
                throw new ValidationException(nameof(id), "The founder cannot leave — deactivate or delete the community instead.");

            var membership = await memberRepository.GetAll()
                .FirstOrDefaultAsync(m => m.LearningCommunityId == id && m.EmployeeId == myEmployeeId)
                ?? throw new ValidationException(nameof(id), "You are not a member of this community.");

            memberRepository.Delete(membership);
            await memberRepository.SaveChangesAsync();
            logger.LogInformation("Employee {Employee} left community {Community}", myEmployeeId, id);
        }
    }

    public class GetAllLearningCommunities(
        IRepository<LearningCommunity> repository,
        IRepository<LearningCommunityMember> memberRepository,
        IRepository<LearningCommunityPost> postRepository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetAllLearningCommunities
    {
        public async Task<PaginatedResponse<LearningCommunityDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var scope = await visibility.GetScopeAsync();
            var myEmp = scope.EmployeeId ?? Guid.Empty;

            // Everyone browses ACTIVE communities; HR also sees deactivated ones.
            var query = repository.GetAll().AsNoTracking();
            if (!scope.IsAdmin)
                query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);
            if (!string.IsNullOrWhiteSpace(request.Kind) &&
                Enum.TryParse<CommunityKind>(request.Kind, true, out var kind))
                query = query.Where(x => x.Kind == kind);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                // Tags participate in search (HC206-b navigation/searchability).
                query = query.Where(x => x.Name.Contains(term) || (x.Tags != null && x.Tags.Contains(term)));
            }

            var members = memberRepository.GetAll();
            var posts = postRepository.GetAll();
            var courses = courseRepository.GetAll();
            var employees = employeeRepository.GetAll();

            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.Name).Skip(skip).Take(take)
                .Select(x => new LearningCommunityDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Kind = x.Kind.ToString(),
                    Tags = x.Tags,
                    TrainingCourseId = x.TrainingCourseId,
                    CourseName = courses.Where(c => c.Id == x.TrainingCourseId).Select(c => c.Name).FirstOrDefault(),
                    IsActive = x.IsActive,
                    CreatedByEmployeeId = x.CreatedByEmployeeId,
                    CreatedByName = employees.Where(e => e.Id == x.CreatedByEmployeeId)
                        .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                        .FirstOrDefault(),
                    MemberCount = members.Count(m => m.LearningCommunityId == x.Id),
                    PostCount = posts.Count(p => p.LearningCommunityId == x.Id),
                    IsMember = members.Any(m => m.LearningCommunityId == x.Id && m.EmployeeId == myEmp),
                    IsModerator = members.Any(m => m.LearningCommunityId == x.Id && m.EmployeeId == myEmp && m.IsModerator)
                }).ToListAsync();

            return new PaginatedResponse<LearningCommunityDto> { Total = total, Data = data };
        }
    }

    public class GetCommunityPosts(
        IRepository<LearningCommunityPost> repository,
        IRepository<CommunityPostReaction> reactionRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetCommunityPosts
    {
        public async Task<PaginatedResponse<CommunityPostDto>> GetAsync(Guid communityId, GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var employees = employeeRepository.GetAll();
            var reactions = reactionRepository.GetAll();
            var scope = await visibility.GetScopeAsync();
            var myEmp = scope.EmployeeId ?? Guid.Empty;

            // Reading is open to every employee (HC198) — topics newest first, replies inline.
            var topics = repository.GetAll().AsNoTracking()
                .Where(x => x.LearningCommunityId == communityId && x.ParentPostId == null);

            var total = await topics.CountAsync();
            var page = await topics.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(take)
                .Select(x => new CommunityPostDto
                {
                    Id = x.Id,
                    LearningCommunityId = x.LearningCommunityId,
                    EmployeeId = x.EmployeeId,
                    AuthorName = employees.Where(e => e.Id == x.EmployeeId)
                        .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                        .FirstOrDefault(),
                    ParentPostId = x.ParentPostId,
                    Content = x.Content,
                    PostedAt = x.CreatedAt.ToDateTimeUtc(),
                    LikeCount = reactions.Count(r => r.LearningCommunityPostId == x.Id),
                    LikedByMe = reactions.Any(r => r.LearningCommunityPostId == x.Id && r.EmployeeId == myEmp)
                }).ToListAsync();

            // One bounded query fetches every reply for the page's topics.
            var topicIds = page.Select(p => p.Id).ToList();
            if (topicIds.Count > 0)
            {
                var replies = await repository.GetAll().AsNoTracking()
                    .Where(x => x.ParentPostId != null && topicIds.Contains(x.ParentPostId.Value))
                    .OrderBy(x => x.CreatedAt)
                    .Select(x => new CommunityPostDto
                    {
                        Id = x.Id,
                        LearningCommunityId = x.LearningCommunityId,
                        EmployeeId = x.EmployeeId,
                        AuthorName = employees.Where(e => e.Id == x.EmployeeId)
                            .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                            .FirstOrDefault(),
                        ParentPostId = x.ParentPostId,
                        Content = x.Content,
                        PostedAt = x.CreatedAt.ToDateTimeUtc(),
                        LikeCount = reactions.Count(r => r.LearningCommunityPostId == x.Id),
                        LikedByMe = reactions.Any(r => r.LearningCommunityPostId == x.Id && r.EmployeeId == myEmp)
                    }).ToListAsync();
                foreach (var topic in page)
                    topic.Replies = replies.Where(r => r.ParentPostId == topic.Id).ToList();
            }

            return new PaginatedResponse<CommunityPostDto> { Total = total, Data = page };
        }
    }

    public class ReactToCommunityPost(
        IRepository<LearningCommunityPost> postRepository,
        IRepository<CommunityPostReaction> reactionRepository,
        IRepository<LearningCommunityMember> memberRepository,
        IPerformanceVisibilityService visibility,
        ILogger<ReactToCommunityPost> logger) : IReactToCommunityPost
    {
        public async Task<bool> ToggleAsync(Guid postId)
        {
            var myEmployeeId = await LearningCommunityShared.RequireEmployeeAsync(visibility);
            var post = await postRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == postId)
                ?? throw new NotFoundException(nameof(LearningCommunityPost), postId.ToString());
            // Reactions are participation — members only, like posting (HC207-b).
            if (!await memberRepository.GetAll().AnyAsync(m => m.LearningCommunityId == post.LearningCommunityId
                    && m.EmployeeId == myEmployeeId))
                throw new ValidationException(nameof(postId), "Join the community to react.");

            var existing = await reactionRepository.GetAll()
                .FirstOrDefaultAsync(r => r.LearningCommunityPostId == postId && r.EmployeeId == myEmployeeId);
            if (existing is not null)
            {
                reactionRepository.Delete(existing);
                await reactionRepository.SaveChangesAsync();
                logger.LogInformation("Reaction removed from post {Post}", postId);
                return false;
            }

            await reactionRepository.AddAsync(CommunityPostReaction.Create(postId, myEmployeeId));
            await reactionRepository.SaveChangesAsync();
            logger.LogInformation("Reaction added to post {Post}", postId);
            return true;
        }
    }

    public class GetCommunityAnalytics(
        IRepository<LearningCommunity> communityRepository,
        IRepository<LearningCommunityMember> memberRepository,
        IRepository<LearningCommunityPost> postRepository,
        IRepository<CommunityPostReaction> reactionRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetCommunityAnalytics
    {
        public async Task<CommunityAnalyticsDto> GetAsync()
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("access", "Forum analytics are for HR administrators.");

            var since = SystemClockInstant(-30);
            var posts = postRepository.GetAll().AsNoTracking();
            var recent = posts.Where(p => p.CreatedAt >= since);

            var employees = employeeRepository.GetAll();
            var topPosters = await recent
                .GroupBy(p => p.EmployeeId)
                .Select(g => new { EmployeeId = g.Key, Posts = g.Count() })
                .OrderByDescending(x => x.Posts).Take(5)
                .Select(x => new CommunityTopPosterDto
                {
                    EmployeeId = x.EmployeeId,
                    Posts = x.Posts,
                    EmployeeName = employees.Where(e => e.Id == x.EmployeeId)
                        .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                        .FirstOrDefault()
                }).ToListAsync();

            var reactions = reactionRepository.GetAll();
            var communities = communityRepository.GetAll();
            var trending = await posts
                .Where(p => p.ParentPostId == null && p.CreatedAt >= since)
                .Select(p => new
                {
                    p.Id,
                    p.Content,
                    p.LearningCommunityId,
                    Replies = posts.Count(r => r.ParentPostId == p.Id),
                    Reactions = reactions.Count(r => r.LearningCommunityPostId == p.Id)
                })
                .OrderByDescending(x => x.Replies + x.Reactions).ThenByDescending(x => x.Replies)
                .Take(5)
                .Select(x => new CommunityTrendingTopicDto
                {
                    PostId = x.Id,
                    CommunityName = communities.Where(c => c.Id == x.LearningCommunityId).Select(c => c.Name).FirstOrDefault(),
                    Excerpt = x.Content.Length > 120 ? x.Content.Substring(0, 120) : x.Content,
                    Replies = x.Replies,
                    Reactions = x.Reactions
                }).ToListAsync();

            var activePosters = await recent.Select(p => p.EmployeeId).Distinct().CountAsync();
            var headcount = await employees.CountAsync();

            return new CommunityAnalyticsDto
            {
                Communities = await communityRepository.GetAll().CountAsync(c => c.IsActive),
                TotalMembers = await memberRepository.GetAll().Select(m => m.EmployeeId).Distinct().CountAsync(),
                PostsLast30Days = await recent.CountAsync(),
                ActivePosters30Days = activePosters,
                ParticipationRatePercent = headcount == 0 ? 0 : Math.Round(activePosters * 100m / headcount, 1),
                TopPosters = topPosters,
                TrendingTopics = trending
            };
        }

        private static NodaTime.Instant SystemClockInstant(int days) =>
            NodaTime.SystemClock.Instance.GetCurrentInstant().Plus(NodaTime.Duration.FromDays(days));
    }

    public class CreateCommunityPost(
        IRepository<LearningCommunityPost> repository,
        IRepository<LearningCommunity> communityRepository,
        IRepository<LearningCommunityMember> memberRepository,
        IRepository<RewardPointsTransaction> pointsRepository,
        IPerformanceVisibilityService visibility,
        IValidator<CreateCommunityPostDto> validator,
        ILogger<CreateCommunityPost> logger) : ICreateCommunityPost
    {
        public async Task<Guid> CreateAsync(Guid communityId, CreateCommunityPostDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var myEmployeeId = await LearningCommunityShared.RequireEmployeeAsync(visibility);
            var community = await communityRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == communityId)
                ?? throw new NotFoundException(nameof(LearningCommunity), communityId.ToString());
            if (!community.IsActive)
                throw new ValidationException(nameof(communityId), "The community is inactive.");

            // HC198 — posting needs membership (reading stays open).
            if (!await memberRepository.GetAll().AnyAsync(m => m.LearningCommunityId == communityId && m.EmployeeId == myEmployeeId))
                throw new ValidationException(nameof(communityId), "Join the community to post.");

            if (dto.ParentPostId.HasValue)
            {
                var parent = await repository.GetAll().AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == dto.ParentPostId.Value && p.LearningCommunityId == communityId)
                    ?? throw new NotFoundException(nameof(LearningCommunityPost), dto.ParentPostId.Value.ToString());
                if (parent.ParentPostId != null)
                    throw new ValidationException(nameof(dto.ParentPostId), "Reply to the topic, not to a reply (one level of threading).");
            }

            var created = LearningCommunityPost.Create(communityId, myEmployeeId, dto.Content.Trim(), dto.ParentPostId);
            await repository.AddAsync(created);
            // HC209 — gamification: forum participation earns engagement points on the reward ledger.
            var points = dto.ParentPostId.HasValue
                ? Engagement.EngagementPoints.ForumReply
                : Engagement.EngagementPoints.ForumTopic;
            await pointsRepository.AddAsync(RewardPointsTransaction.Create(myEmployeeId, points,
                RewardPointsSource.Engagement, DateTime.UtcNow.Date, created.Id,
                dto.ParentPostId.HasValue ? "Engagement: forum reply" : "Engagement: forum topic"));
            await repository.SaveChangesAsync();
            logger.LogInformation("Post {Id} created in community {Community}", created.Id, communityId);
            return created.Id;
        }
    }

    public class DeleteCommunityPost(
        IRepository<LearningCommunityPost> repository,
        IRepository<LearningCommunityMember> memberRepository,
        IPerformanceVisibilityService visibility,
        ILogger<DeleteCommunityPost> logger) : IDeleteCommunityPost
    {
        public async Task DeleteAsync(Guid postId)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == postId)
                ?? throw new NotFoundException(nameof(LearningCommunityPost), postId.ToString());

            var scope = await visibility.GetScopeAsync();
            var canModerate = await LearningCommunityShared.IsModeratorOrAdminAsync(
                visibility, memberRepository, entity.LearningCommunityId);
            if (!canModerate && entity.EmployeeId != scope.EmployeeId)
                throw new ValidationException(nameof(postId), "Only the author, a moderator or HR can delete a post.");

            // ParentPostId is FK-less — a topic takes its replies with it here.
            if (entity.ParentPostId == null)
            {
                var replies = await repository.GetAll().Where(x => x.ParentPostId == postId).ToListAsync();
                foreach (var reply in replies) repository.Delete(reply);
            }
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted post {Id}", postId);
        }
    }
}
