using System.Text.Json;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Engagement
{
    // ---- DTOs ---------------------------------------------------------------
    public class SurveyQuestionDto
    {
        public string Key { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        /// <summary>Rating (1–5) | Choice | Text.</summary>
        public string Type { get; set; } = "Rating";
        public List<string> Options { get; set; } = [];
        public bool Required { get; set; } = true;
    }

    public class SurveyDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPoll { get; set; }
        public bool IsAnonymous { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? OpensOn { get; set; }
        public DateTime? ClosesOn { get; set; }
        public int QuestionCount { get; set; }
        public int ResponseCount { get; set; }
        public bool HasResponded { get; set; }
        public List<SurveyQuestionDto> Questions { get; set; } = [];
    }

    public class SaveSurveyDto
    {
        public Guid? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPoll { get; set; }
        public bool IsAnonymous { get; set; }
        public DateTime? OpensOn { get; set; }
        public DateTime? ClosesOn { get; set; }
        public List<SurveyQuestionDto> Questions { get; set; } = [];
    }

    public class SubmitSurveyResponseDto
    {
        /// <summary>Question key → answer value (rating number / chosen option / free text).</summary>
        public Dictionary<string, string> Answers { get; set; } = [];
    }

    public class SurveyQuestionResultDto
    {
        public string Key { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Answered { get; set; }
        public decimal? Average { get; set; }
        /// <summary>Rating value / option → count.</summary>
        public Dictionary<string, int> Counts { get; set; } = [];
        public List<string> TextAnswers { get; set; } = [];
    }

    public class SurveyResultsDto
    {
        public Guid SurveyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsAnonymous { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ResponseCount { get; set; }
        public int EligibleCount { get; set; }
        public decimal CompletionRatePercent { get; set; }
        public List<SurveyQuestionResultDto> Questions { get; set; } = [];
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveSurvey { Task<Guid> SaveAsync(SaveSurveyDto dto); }
    public interface IOpenSurvey { Task OpenAsync(Guid id); }
    public interface ICloseSurvey { Task CloseAsync(Guid id); }
    public interface IDeleteSurvey { Task DeleteAsync(Guid id); }
    public interface IGetAllSurveys { Task<PaginatedResponse<SurveyDto>> GetAsync(GetAllRequest request); }
    /// <summary>Open surveys the CALLER can take right now, with their completion flag.</summary>
    public interface IGetSurveyFeed { Task<PaginatedResponse<SurveyDto>> GetAsync(GetAllRequest request); }
    public interface IGetSurveyById { Task<SurveyDto> GetAsync(Guid id); }
    public interface ISubmitSurveyResponse { Task SubmitAsync(Guid id, SubmitSurveyResponseDto dto); }
    public interface IGetSurveyResults { Task<SurveyResultsDto> GetAsync(Guid id); }

    internal static class SurveyShared
    {
        internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
        internal const int RatingMax = 5;

        internal static List<SurveyQuestionDto> ParseQuestions(string questionsJson) =>
            JsonSerializer.Deserialize<List<SurveyQuestionDto>>(questionsJson, Json) ?? [];

        /// <summary>Normalizes + validates the question set (keys, types, choice options, poll arity).</summary>
        internal static string ValidateAndSerializeQuestions(List<SurveyQuestionDto> questions, bool isPoll)
        {
            if (questions.Count == 0)
                throw new ValidationException("questions", "At least one question is required.");
            if (isPoll && questions.Count != 1)
                throw new ValidationException("questions", "A poll is exactly one question.");

            var index = 1;
            foreach (var q in questions)
            {
                if (string.IsNullOrWhiteSpace(q.Text))
                    throw new ValidationException("questions", $"Question {index} needs its text.");
                if (q.Text.Length > 500)
                    throw new ValidationException("questions", $"Question {index} is too long (500 max).");
                if (q.Type is not ("Rating" or "Choice" or "Text"))
                    throw new ValidationException("questions", $"Question {index}: type must be Rating, Choice or Text.");
                if (q.Type == "Choice" && q.Options.Count(o => !string.IsNullOrWhiteSpace(o)) < 2)
                    throw new ValidationException("questions", $"Question {index}: a choice question needs at least two options.");
                if (q.Type != "Choice") q.Options = [];
                else q.Options = q.Options.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()).ToList();
                q.Key = string.IsNullOrWhiteSpace(q.Key) ? $"q{index}" : q.Key.Trim();
                index++;
            }
            if (questions.Select(q => q.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count() != questions.Count)
                throw new ValidationException("questions", "Question keys must be unique.");
            return JsonSerializer.Serialize(questions, Json);
        }

        internal static async Task EnsureAdminAsync(IPerformanceVisibilityService visibility)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("access", "Only HR administrators can manage surveys.");
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveSurvey(
        IRepository<Survey> repository,
        IPerformanceVisibilityService visibility,
        ILogger<SaveSurvey> logger) : ISaveSurvey
    {
        public async Task<Guid> SaveAsync(SaveSurveyDto dto)
        {
            await SurveyShared.EnsureAdminAsync(visibility);
            if (string.IsNullOrWhiteSpace(dto.Title) || dto.Title.Length > 300)
                throw new ValidationException(nameof(dto.Title), "A title (up to 300 characters) is required.");
            if (dto.Description?.Length > 1000)
                throw new ValidationException(nameof(dto.Description), "The description is at most 1000 characters.");

            var questionsJson = SurveyShared.ValidateAndSerializeQuestions(dto.Questions, dto.IsPoll);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(Survey), dto.Id.Value.ToString());
                if (entity.Status != SurveyStatus.Draft)
                    throw new ValidationException(nameof(dto.Id), "Only a draft survey can be edited — its questions are frozen once open.");
                entity.UpdateDraft(dto.Title.Trim(), dto.Description, dto.IsPoll, dto.IsAnonymous,
                    questionsJson, dto.OpensOn, dto.ClosesOn);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated survey {Id}", entity.Id);
                return entity.Id;
            }

            var created = Survey.Create(dto.Title.Trim(), dto.Description, dto.IsPoll, dto.IsAnonymous,
                questionsJson, dto.OpensOn, dto.ClosesOn);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created survey {Id} ({Kind})", created.Id, dto.IsPoll ? "poll" : "survey");
            return created.Id;
        }
    }

    public class OpenSurvey(
        IRepository<Survey> repository,
        IPerformanceVisibilityService visibility,
        ILogger<OpenSurvey> logger) : IOpenSurvey
    {
        public async Task OpenAsync(Guid id)
        {
            await SurveyShared.EnsureAdminAsync(visibility);
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(Survey), id.ToString());
            if (entity.Status != SurveyStatus.Draft)
                throw new ValidationException(nameof(id), $"Only a draft survey can open (current: {entity.Status}).");
            entity.Open();
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Survey {Id} opened", id);
        }
    }

    public class CloseSurvey(
        IRepository<Survey> repository,
        IPerformanceVisibilityService visibility,
        ILogger<CloseSurvey> logger) : ICloseSurvey
    {
        public async Task CloseAsync(Guid id)
        {
            await SurveyShared.EnsureAdminAsync(visibility);
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(Survey), id.ToString());
            if (entity.Status != SurveyStatus.Open)
                throw new ValidationException(nameof(id), $"Only an open survey can close (current: {entity.Status}).");
            entity.Close();
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Survey {Id} closed", id);
        }
    }

    public class DeleteSurvey(
        IRepository<Survey> repository,
        IRepository<SurveyResponse> responseRepository,
        IPerformanceVisibilityService visibility,
        ILogger<DeleteSurvey> logger) : IDeleteSurvey
    {
        public async Task DeleteAsync(Guid id)
        {
            await SurveyShared.EnsureAdminAsync(visibility);
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(Survey), id.ToString());
            if (await responseRepository.GetAll().AnyAsync(r => r.SurveyId == id))
                throw new ValidationException(nameof(id), "A survey with responses cannot be deleted — close it instead.");
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted survey {Id}", id);
        }
    }

    public class GetAllSurveys(
        IRepository<Survey> repository,
        IRepository<SurveyResponse> responseRepository,
        IPerformanceVisibilityService visibility) : IGetAllSurveys
    {
        public async Task<PaginatedResponse<SurveyDto>> GetAsync(GetAllRequest request)
        {
            await SurveyShared.EnsureAdminAsync(visibility);

            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<SurveyStatus>(request.Status, true, out var status))
                query = query.Where(x => x.Status == status);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Title.Contains(request.SearchText.Trim()));

            var responses = responseRepository.GetAll();
            var total = await query.CountAsync();
            var rows = await query.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(take)
                .Select(x => new
                {
                    Entity = x,
                    ResponseCount = responses.Count(r => r.SurveyId == x.Id)
                }).ToListAsync();

            var data = rows.Select(x => new SurveyDto
            {
                Id = x.Entity.Id,
                Title = x.Entity.Title,
                Description = x.Entity.Description,
                IsPoll = x.Entity.IsPoll,
                IsAnonymous = x.Entity.IsAnonymous,
                Status = x.Entity.Status.ToString(),
                OpensOn = x.Entity.OpensOn,
                ClosesOn = x.Entity.ClosesOn,
                QuestionCount = SurveyShared.ParseQuestions(x.Entity.QuestionsJson).Count,
                ResponseCount = x.ResponseCount
            }).ToList();

            return new PaginatedResponse<SurveyDto> { Total = total, Data = data };
        }
    }

    public class GetSurveyFeed(
        IRepository<Survey> repository,
        IRepository<SurveyCompletion> completionRepository,
        IPerformanceVisibilityService visibility) : IGetSurveyFeed
    {
        public async Task<PaginatedResponse<SurveyDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var scope = await visibility.GetScopeAsync();
            var myEmp = scope.EmployeeId ?? Guid.Empty;
            var today = DateTime.UtcNow.Date;

            var completions = completionRepository.GetAll();
            var query = repository.GetAll().AsNoTracking()
                .Where(x => x.Status == SurveyStatus.Open
                    && (x.OpensOn == null || x.OpensOn <= today)
                    && (x.ClosesOn == null || x.ClosesOn >= today));

            var total = await query.CountAsync();
            var rows = await query.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(take)
                .Select(x => new
                {
                    Entity = x,
                    HasResponded = completions.Any(c => c.SurveyId == x.Id && c.EmployeeId == myEmp)
                }).ToListAsync();

            var data = rows.Select(x => new SurveyDto
            {
                Id = x.Entity.Id,
                Title = x.Entity.Title,
                Description = x.Entity.Description,
                IsPoll = x.Entity.IsPoll,
                IsAnonymous = x.Entity.IsAnonymous,
                Status = x.Entity.Status.ToString(),
                OpensOn = x.Entity.OpensOn,
                ClosesOn = x.Entity.ClosesOn,
                QuestionCount = SurveyShared.ParseQuestions(x.Entity.QuestionsJson).Count,
                HasResponded = x.HasResponded,
                Questions = SurveyShared.ParseQuestions(x.Entity.QuestionsJson)
            }).ToList();

            return new PaginatedResponse<SurveyDto> { Total = total, Data = data };
        }
    }

    public class GetSurveyById(
        IRepository<Survey> repository,
        IRepository<SurveyResponse> responseRepository,
        IRepository<SurveyCompletion> completionRepository,
        IPerformanceVisibilityService visibility) : IGetSurveyById
    {
        public async Task<SurveyDto> GetAsync(Guid id)
        {
            var entity = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(Survey), id.ToString());

            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin && entity.Status == SurveyStatus.Draft)
                throw new ValidationException(nameof(id), "The survey is not open yet.");

            return new SurveyDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                IsPoll = entity.IsPoll,
                IsAnonymous = entity.IsAnonymous,
                Status = entity.Status.ToString(),
                OpensOn = entity.OpensOn,
                ClosesOn = entity.ClosesOn,
                Questions = SurveyShared.ParseQuestions(entity.QuestionsJson),
                QuestionCount = SurveyShared.ParseQuestions(entity.QuestionsJson).Count,
                ResponseCount = await responseRepository.GetAll().CountAsync(r => r.SurveyId == id),
                HasResponded = scope.EmployeeId.HasValue &&
                    await completionRepository.GetAll().AnyAsync(c => c.SurveyId == id && c.EmployeeId == scope.EmployeeId.Value)
            };
        }
    }

    public class SubmitSurveyResponse(
        IRepository<Survey> repository,
        IRepository<SurveyResponse> responseRepository,
        IRepository<SurveyCompletion> completionRepository,
        IRepository<RewardPointsTransaction> pointsRepository,
        IPerformanceVisibilityService visibility,
        ILogger<SubmitSurveyResponse> logger) : ISubmitSurveyResponse
    {
        public async Task SubmitAsync(Guid id, SubmitSurveyResponseDto dto)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.EmployeeId.HasValue)
                throw new ValidationException(nameof(id), "Your account is not linked to an employee record.");

            var survey = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(Survey), id.ToString());
            if (!survey.AcceptsResponsesOn(DateTime.UtcNow.Date))
                throw new ValidationException(nameof(id), "The survey is not accepting responses.");
            if (await completionRepository.GetAll().AnyAsync(c => c.SurveyId == id && c.EmployeeId == scope.EmployeeId.Value))
                throw new ValidationException(nameof(id), "You have already responded to this survey.");

            // Validate the answer set against the question definitions.
            var questions = SurveyShared.ParseQuestions(survey.QuestionsJson);
            var answers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (key, value) in dto.Answers)
                if (!string.IsNullOrWhiteSpace(value)) answers[key.Trim()] = value.Trim();

            foreach (var q in questions)
            {
                var has = answers.TryGetValue(q.Key, out var value);
                if (!has)
                {
                    if (q.Required)
                        throw new ValidationException(q.Key, $"\"{q.Text}\" requires an answer.");
                    continue;
                }
                switch (q.Type)
                {
                    case "Rating":
                        if (!int.TryParse(value, out var rating) || rating < 1 || rating > SurveyShared.RatingMax)
                            throw new ValidationException(q.Key, $"\"{q.Text}\" takes a rating between 1 and {SurveyShared.RatingMax}.");
                        break;
                    case "Choice":
                        if (!q.Options.Contains(value, StringComparer.OrdinalIgnoreCase))
                            throw new ValidationException(q.Key, $"\"{q.Text}\" only accepts its listed options.");
                        break;
                    case "Text":
                        if (value!.Length > 2000)
                            throw new ValidationException(q.Key, $"\"{q.Text}\" answers are at most 2000 characters.");
                        break;
                }
            }
            var unknown = answers.Keys.FirstOrDefault(k => !questions.Any(q => q.Key.Equals(k, StringComparison.OrdinalIgnoreCase)));
            if (unknown != null)
                throw new ValidationException(unknown, "The answer set references an unknown question.");

            var response = SurveyResponse.Create(id,
                survey.IsAnonymous ? null : scope.EmployeeId,
                JsonSerializer.Serialize(answers, SurveyShared.Json), DateTime.UtcNow.Date);
            await responseRepository.AddAsync(response);
            // HC207 discipline — clear the CreatedBy stamp on anonymous answers BEFORE saving;
            // the response entity is not IAuditable, so no audit row exists either. The completion
            // marker (separate row) enforces one-vote without linking to the answers.
            if (survey.IsAnonymous) response.Create(null);
            await completionRepository.AddAsync(SurveyCompletion.Create(id, scope.EmployeeId.Value));
            // HC209 — the points ride the COMPLETION (not the answers), so anonymous voters still
            // earn without linking their answer row.
            await pointsRepository.AddAsync(RewardPointsTransaction.Create(scope.EmployeeId.Value,
                EngagementPoints.SurveyResponse, RewardPointsSource.Engagement, DateTime.UtcNow.Date,
                id, "Engagement: survey response"));
            await responseRepository.SaveChangesAsync();
            logger.LogInformation("Survey {Id} received a response ({Mode})", id, survey.IsAnonymous ? "anonymous" : "named");
        }
    }

    public class GetSurveyResults(
        IRepository<Survey> repository,
        IRepository<SurveyResponse> responseRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetSurveyResults
    {
        public async Task<SurveyResultsDto> GetAsync(Guid id)
        {
            await SurveyShared.EnsureAdminAsync(visibility);

            var survey = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(Survey), id.ToString());
            var questions = SurveyShared.ParseQuestions(survey.QuestionsJson);

            // Bounded by headcount — answer sets aggregate in memory per question.
            var answerSets = (await responseRepository.GetAll().AsNoTracking()
                    .Where(r => r.SurveyId == id).Select(r => r.AnswersJson).ToListAsync())
                .Select(json => JsonSerializer.Deserialize<Dictionary<string, string>>(json, SurveyShared.Json) ?? [])
                .Select(d => new Dictionary<string, string>(d, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var results = new List<SurveyQuestionResultDto>();
            foreach (var q in questions)
            {
                var values = answerSets
                    .Select(a => a.TryGetValue(q.Key, out var v) ? v : null)
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Select(v => v!)
                    .ToList();
                var result = new SurveyQuestionResultDto { Key = q.Key, Text = q.Text, Type = q.Type, Answered = values.Count };
                switch (q.Type)
                {
                    case "Rating":
                        var ratings = values.Select(v => int.TryParse(v, out var n) ? n : 0).Where(n => n >= 1).ToList();
                        result.Average = ratings.Count == 0 ? null : Math.Round((decimal)ratings.Average(), 2);
                        result.Counts = ratings.GroupBy(n => n.ToString()).ToDictionary(g => g.Key, g => g.Count());
                        break;
                    case "Choice":
                        result.Counts = q.Options.ToDictionary(o => o,
                            o => values.Count(v => v.Equals(o, StringComparison.OrdinalIgnoreCase)));
                        break;
                    case "Text":
                        result.TextAnswers = values.Take(200).ToList();
                        break;
                }
                results.Add(result);
            }

            var eligible = await employeeRepository.GetAll().CountAsync();
            var responseCount = answerSets.Count;
            return new SurveyResultsDto
            {
                SurveyId = survey.Id,
                Title = survey.Title,
                IsAnonymous = survey.IsAnonymous,
                Status = survey.Status.ToString(),
                ResponseCount = responseCount,
                EligibleCount = eligible,
                CompletionRatePercent = eligible == 0 ? 0 : Math.Round(responseCount * 100m / eligible, 1),
                Questions = results
            };
        }
    }
}
