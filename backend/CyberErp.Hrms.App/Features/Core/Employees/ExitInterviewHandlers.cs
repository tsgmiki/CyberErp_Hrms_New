using System.Text.Json;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Engagement;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    // ---- DTOs ---------------------------------------------------------------
    public class ExitQuestionnaireDto
    {
        public List<SurveyQuestionDto> Questions { get; set; } = [];
    }

    public class ExitInterviewDto
    {
        public Guid Id { get; set; }
        public Guid TerminationId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CompletedOn { get; set; }
        public List<SurveyQuestionDto> Questions { get; set; } = [];
        public Dictionary<string, string> Answers { get; set; } = [];
    }

    public class SubmitExitInterviewDto
    {
        public Dictionary<string, string> Answers { get; set; } = [];
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IGetExitQuestionnaire { Task<ExitQuestionnaireDto> GetAsync(); }
    public interface ISaveExitQuestionnaire { Task SaveAsync(ExitQuestionnaireDto dto); }
    public interface ILaunchExitInterview { Task<Guid> LaunchAsync(Guid terminationId); }
    public interface ISubmitExitInterview { Task SubmitAsync(Guid id, SubmitExitInterviewDto dto); }
    public interface IGetExitInterview { Task<ExitInterviewDto?> GetAsync(Guid terminationId); }

    internal static class ExitInterviewShared
    {
        /// <summary>Same answer rules as surveys: required, rating 1–5, listed choices, no unknown keys.</summary>
        internal static string ValidateAnswers(List<SurveyQuestionDto> questions, Dictionary<string, string> raw)
        {
            var answers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (key, value) in raw)
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
                        if (!int.TryParse(value, out var rating) || rating < 1 || rating > 5)
                            throw new ValidationException(q.Key, $"\"{q.Text}\" takes a rating between 1 and 5.");
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
            return JsonSerializer.Serialize(answers, SurveyShared.Json);
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class GetExitQuestionnaire(
        IRepository<ExitQuestionnaire> repository,
        IPerformanceVisibilityService visibility) : IGetExitQuestionnaire
    {
        public async Task<ExitQuestionnaireDto> GetAsync()
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("access", "The questionnaire configuration is HR-only — leavers answer through their interview.");
            var row = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync();
            return new ExitQuestionnaireDto
            {
                Questions = row is null ? [] : SurveyShared.ParseQuestions(row.QuestionsJson)
            };
        }
    }

    public class SaveExitQuestionnaire(
        IRepository<ExitQuestionnaire> repository,
        IPerformanceVisibilityService visibility,
        ILogger<SaveExitQuestionnaire> logger) : ISaveExitQuestionnaire
    {
        public async Task SaveAsync(ExitQuestionnaireDto dto)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("access", "Only HR administrators can configure the exit questionnaire.");

            var questionsJson = SurveyShared.ValidateAndSerializeQuestions(dto.Questions, isPoll: false);
            var row = await repository.GetAll().FirstOrDefaultAsync();
            if (row is null)
            {
                await repository.AddAsync(ExitQuestionnaire.Create(questionsJson));
            }
            else
            {
                row.Update(questionsJson);
                repository.UpdateAsync(row);
            }
            await repository.SaveChangesAsync();
            logger.LogInformation("Exit questionnaire saved ({Count} questions)", dto.Questions.Count);
        }
    }

    public class LaunchExitInterview(
        IRepository<ExitInterview> repository,
        IRepository<ExitQuestionnaire> questionnaireRepository,
        IRepository<EmployeeTermination> terminationRepository,
        IPerformanceVisibilityService visibility,
        ILogger<LaunchExitInterview> logger) : ILaunchExitInterview
    {
        public async Task<Guid> LaunchAsync(Guid terminationId)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("access", "Only HR administrators launch exit interviews.");

            var status = await terminationRepository.GetAll().AsNoTracking()
                .Where(t => t.Id == terminationId).Select(t => (TerminationStatus?)t.Status).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(EmployeeTermination), terminationId.ToString());
            if (status == TerminationStatus.Cancelled)
                throw new ValidationException(nameof(terminationId), "A cancelled case has no exit interview.");

            // One interview per case — re-launching returns the existing one.
            var existing = await repository.GetAll()
                .Where(x => x.TerminationId == terminationId).Select(x => (Guid?)x.Id).FirstOrDefaultAsync();
            if (existing.HasValue) return existing.Value;

            var questionnaire = await questionnaireRepository.GetAll().AsNoTracking().FirstOrDefaultAsync()
                ?? throw new ValidationException(nameof(terminationId), "Configure the exit questionnaire before launching interviews.");

            var created = ExitInterview.Create(terminationId, questionnaire.QuestionsJson);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Exit interview {Id} launched for case {Case}", created.Id, terminationId);
            return created.Id;
        }
    }

    public class SubmitExitInterview(
        IRepository<ExitInterview> repository,
        IRepository<EmployeeTermination> terminationRepository,
        IPerformanceVisibilityService visibility,
        ILogger<SubmitExitInterview> logger) : ISubmitExitInterview
    {
        public async Task SubmitAsync(Guid id, SubmitExitInterviewDto dto)
        {
            var interview = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(ExitInterview), id.ToString());
            if (interview.Status == ExitInterviewStatus.Completed)
                throw new ValidationException(nameof(id), "The interview is already completed.");

            // HC219 — the LEAVER answers (self-service), or HR records the conversation.
            var scope = await visibility.GetScopeAsync();
            var leaverId = await terminationRepository.GetAll().AsNoTracking()
                .Where(t => t.Id == interview.TerminationId).Select(t => t.EmployeeId).FirstOrDefaultAsync();
            var isLeaver = scope.EmployeeId.HasValue && scope.EmployeeId.Value == leaverId;
            if (!scope.IsAdmin && !isLeaver)
                throw new ValidationException(nameof(id), "Only the leaver or HR can complete the exit interview.");

            var questions = SurveyShared.ParseQuestions(interview.QuestionsJson);
            var answersJson = ExitInterviewShared.ValidateAnswers(questions, dto.Answers);

            interview.Complete(answersJson, scope.EmployeeId, DateTime.UtcNow.Date);
            repository.UpdateAsync(interview);
            await repository.SaveChangesAsync();
            logger.LogInformation("Exit interview {Id} completed ({By})", id, isLeaver ? "leaver" : "HR");
        }
    }

    public class GetExitInterview(
        IRepository<ExitInterview> repository,
        IRepository<EmployeeTermination> terminationRepository,
        IPerformanceVisibilityService visibility) : IGetExitInterview
    {
        public async Task<ExitInterviewDto?> GetAsync(Guid terminationId)
        {
            var leaverId = await terminationRepository.GetAll().AsNoTracking()
                .Where(t => t.Id == terminationId).Select(t => (Guid?)t.EmployeeId).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(EmployeeTermination), terminationId.ToString());

            var scope = await visibility.GetScopeAsync();
            var isLeaver = scope.EmployeeId.HasValue && scope.EmployeeId.Value == leaverId;
            if (!scope.IsAdmin && !isLeaver && !await visibility.CanAccessEmployeeAsync(leaverId))
                throw new ValidationException(nameof(terminationId), "You do not have access to this exit case.");

            var interview = await repository.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(x => x.TerminationId == terminationId);
            if (interview is null) return null;

            return new ExitInterviewDto
            {
                Id = interview.Id,
                TerminationId = interview.TerminationId,
                Status = interview.Status.ToString(),
                CompletedOn = interview.CompletedOn,
                Questions = SurveyShared.ParseQuestions(interview.QuestionsJson),
                Answers = string.IsNullOrEmpty(interview.AnswersJson)
                    ? []
                    : JsonSerializer.Deserialize<Dictionary<string, string>>(interview.AnswersJson, SurveyShared.Json) ?? []
            };
        }
    }
}
