using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    // ---- DTOs ---------------------------------------------------------------

    public class QuestionOptionDto
    {
        public Guid? Id { get; set; }
        public int SortOrder { get; set; }
        public string Text { get; set; } = string.Empty;
        /// <summary>
        /// ⚠️ THE ANSWER KEY. Populated for authoring only — the learner-facing DTOs in
        /// <c>AssessmentAttemptHandlers</c> deliberately do not carry it.
        /// </summary>
        public bool IsCorrect { get; set; }
    }

    public class QuestionDto
    {
        public Guid? Id { get; set; }
        public int SortOrder { get; set; }
        public string Text { get; set; } = string.Empty;
        /// <summary>SingleChoice | MultipleChoice | TrueFalse.</summary>
        public string Kind { get; set; } = nameof(QuestionKind.SingleChoice);
        public decimal Points { get; set; } = 1m;
        public string? Explanation { get; set; }
        public List<QuestionOptionDto> Options { get; set; } = [];
    }

    public class QuestionBankDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int QuestionCount { get; set; }
    }

    public class SaveQuestionBankDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SaveBankQuestionsDto
    {
        public Guid QuestionBankId { get; set; }
        public List<QuestionDto> Questions { get; set; } = [];
    }

    public class AssessmentDto
    {
        public Guid Id { get; set; }
        public Guid ContentModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Instructions { get; set; }
        public decimal PassMark { get; set; }
        public int? MaxAttempts { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public bool ShuffleQuestions { get; set; }
        public bool RevealAnswers { get; set; }
        public int QuestionCount { get; set; }
        public decimal TotalPoints { get; set; }
        public List<QuestionDto> Questions { get; set; } = [];
    }

    public class SaveAssessmentDto
    {
        public Guid ContentModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Instructions { get; set; }
        public decimal PassMark { get; set; } = 70m;
        public int? MaxAttempts { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public bool ShuffleQuestions { get; set; }
        public bool RevealAnswers { get; set; } = true;
    }

    public class SaveAssessmentQuestionsDto
    {
        public Guid AssessmentId { get; set; }
        public List<QuestionDto> Questions { get; set; } = [];
    }

    public class ImportQuestionsDto
    {
        public Guid AssessmentId { get; set; }
        public Guid QuestionBankId { get; set; }
        /// <summary>Empty means every question in the bank.</summary>
        public List<Guid> QuestionIds { get; set; } = [];
    }

    // ---- Interfaces ---------------------------------------------------------

    public interface IGetQuestionBanks { Task<PaginatedResponse<QuestionBankDto>> GetAsync(GetAllRequest request); }
    public interface IGetQuestionBank { Task<QuestionBankDto> GetAsync(Guid id); }
    public interface ISaveQuestionBank { Task<Guid> SaveAsync(SaveQuestionBankDto dto); }
    public interface IDeleteQuestionBank { Task DeleteAsync(Guid id); }
    public interface IGetBankQuestions { Task<List<QuestionDto>> GetAsync(Guid questionBankId); }
    public interface ISetBankQuestions { Task SetAsync(SaveBankQuestionsDto dto); }

    public interface IGetAssessment { Task<AssessmentDto?> GetByModuleAsync(Guid contentModuleId); }
    public interface ISaveAssessment { Task<Guid> SaveAsync(SaveAssessmentDto dto); }
    public interface ISetAssessmentQuestions { Task SetAsync(SaveAssessmentQuestionsDto dto); }
    public interface IImportQuestionsFromBank { Task<int> ImportAsync(ImportQuestionsDto dto); }

    // ---- Shared -------------------------------------------------------------

    internal static class AssessmentShared
    {
        internal static QuestionKind ParseKind(string kind) =>
            Enum.TryParse<QuestionKind>(kind, true, out var k)
                ? k
                : throw new ValidationException("kind", $"'{kind}' is not a question type.");

        /// <summary>Maps the authoring view of a question — answer key included.</summary>
        internal static QuestionDto ToDto(Question q) => new()
        {
            Id = q.Id,
            SortOrder = q.SortOrder,
            Text = q.Text,
            Kind = q.Kind.ToString(),
            Points = q.Points,
            Explanation = q.Explanation,
            Options = [.. q.Options.OrderBy(o => o.SortOrder).Select(o => new QuestionOptionDto
            {
                Id = o.Id, SortOrder = o.SortOrder, Text = o.Text, IsCorrect = o.IsCorrect
            })]
        };

        internal static QuestionSpec ToSpec(QuestionDto dto) => new(
            dto.Text,
            ParseKind(dto.Kind),
            dto.Points,
            dto.Explanation,
            [.. (dto.Options ?? []).Select(o => new QuestionOptionSpec(o.Text, o.IsCorrect))]);

        /// <summary>
        /// Domain rules reach the author as field errors rather than a 500. Every message here is
        /// written to be read by the person who built the question ("'Cold chain…' has no correct
        /// answer."), so it names what to fix.
        /// </summary>
        internal static void Guard(Action action, string field)
        {
            try { action(); }
            catch (ArgumentException ex) { throw new ValidationException(field, ex.Message); }
            catch (InvalidOperationException ex) { throw new ValidationException(field, ex.Message); }
        }
    }

    // ---- Question banks -----------------------------------------------------

    public class GetQuestionBanks(
        IRepository<QuestionBank> repository,
        IRepository<Question> questionRepository) : IGetQuestionBanks
    {
        public async Task<PaginatedResponse<QuestionBankDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(b => b.Name.Contains(term)
                    || (b.Description != null && b.Description.Contains(term)));
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(b => b.IsActive == active);

            var total = await query.CountAsync();
            var rows = await query.OrderBy(b => b.Name)
                .Skip(skip).Take(take)
                .Select(b => new QuestionBankDto
                {
                    Id = b.Id, Name = b.Name, Description = b.Description, IsActive = b.IsActive
                })
                .ToListAsync();

            // Counted in one grouped query rather than per row — the list is the only place the
            // number is shown, and a bank with no questions is the thing worth spotting.
            var ids = rows.Select(r => r.Id).ToList();
            var counts = await questionRepository.GetAll().AsNoTracking()
                .Where(q => q.QuestionBankId != null && ids.Contains(q.QuestionBankId.Value))
                .GroupBy(q => q.QuestionBankId!.Value)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();
            foreach (var row in rows)
                row.QuestionCount = counts.FirstOrDefault(c => c.Key == row.Id)?.Count ?? 0;

            return new PaginatedResponse<QuestionBankDto> { Data = rows, Total = total };
        }
    }

    public class GetQuestionBank(
        IRepository<QuestionBank> repository,
        IRepository<Question> questionRepository) : IGetQuestionBank
    {
        public async Task<QuestionBankDto> GetAsync(Guid id)
        {
            var bank = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync(b => b.Id == id)
                ?? throw new NotFoundException(nameof(QuestionBank), id.ToString());

            return new QuestionBankDto
            {
                Id = bank.Id,
                Name = bank.Name,
                Description = bank.Description,
                IsActive = bank.IsActive,
                QuestionCount = await questionRepository.GetAll().AsNoTracking()
                    .CountAsync(q => q.QuestionBankId == id)
            };
        }
    }

    public class SaveQuestionBank(IRepository<QuestionBank> repository) : ISaveQuestionBank
    {
        public async Task<Guid> SaveAsync(SaveQuestionBankDto dto)
        {
            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var bank = await repository.GetAll().FirstOrDefaultAsync(b => b.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(QuestionBank), dto.Id.Value.ToString());
                AssessmentShared.Guard(() => bank.Update(dto.Name, dto.Description, dto.IsActive), "name");
                repository.UpdateAsync(bank);
                await repository.SaveChangesAsync();
                return bank.Id;
            }

            QuestionBank created = null!;
            AssessmentShared.Guard(() => created = QuestionBank.Create(dto.Name, dto.Description, dto.IsActive), "name");
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            return created.Id;
        }
    }

    public class DeleteQuestionBank(
        IRepository<QuestionBank> repository,
        IRepository<Question> questionRepository) : IDeleteQuestionBank
    {
        public async Task DeleteAsync(Guid id)
        {
            var bank = await repository.GetAll().FirstOrDefaultAsync(b => b.Id == id)
                ?? throw new NotFoundException(nameof(QuestionBank), id.ToString());

            // Questions cascade with the bank in the database, but they are removed here too so the
            // change set is explicit and the tracked graph does not go stale behind the delete.
            var questions = await questionRepository.GetAll()
                .Include(q => q.Options)
                .Where(q => q.QuestionBankId == id).ToListAsync();
            foreach (var q in questions) questionRepository.Delete(q);

            repository.Delete(bank);
            await repository.SaveChangesAsync();
        }
    }

    public class GetBankQuestions(IRepository<Question> repository) : IGetBankQuestions
    {
        public async Task<List<QuestionDto>> GetAsync(Guid questionBankId)
        {
            var questions = await repository.GetAll().AsNoTracking()
                .Include(q => q.Options)
                .Where(q => q.QuestionBankId == questionBankId)
                .OrderBy(q => q.SortOrder)
                .ToListAsync();
            return [.. questions.Select(AssessmentShared.ToDto)];
        }
    }

    /// <summary>
    /// Replaces a bank's question list — set semantics, like every other child collection here.
    ///
    /// <para>Rows carrying an id are UPDATED rather than replaced. A bank question can already be
    /// referenced by nothing (imports copy it), so this is only about keeping the editor stable, but
    /// it costs nothing and matches how modules behave.</para>
    /// </summary>
    public class SetBankQuestions(
        IRepository<QuestionBank> bankRepository,
        IRepository<Question> repository,
        ILogger<SetBankQuestions> logger) : ISetBankQuestions
    {
        public async Task SetAsync(SaveBankQuestionsDto dto)
        {
            if (!await bankRepository.GetAll().AnyAsync(b => b.Id == dto.QuestionBankId))
                throw new NotFoundException(nameof(QuestionBank), dto.QuestionBankId.ToString());

            var existing = await repository.GetAll()
                .Include(q => q.Options)
                .Where(q => q.QuestionBankId == dto.QuestionBankId)
                .ToListAsync();

            var incoming = dto.Questions ?? [];
            var kept = new List<Guid>();
            var order = 1;

            foreach (var qd in incoming)
            {
                var spec = AssessmentShared.ToSpec(qd);
                var match = qd.Id.HasValue ? existing.FirstOrDefault(q => q.Id == qd.Id.Value) : null;
                if (match is not null)
                {
                    var sortOrder = order++;
                    AssessmentShared.Guard(() => match.Apply(sortOrder, spec), "questions");
                    repository.UpdateAsync(match);
                    kept.Add(match.Id);
                }
                else
                {
                    var sortOrder = order++;
                    Question created = null!;
                    AssessmentShared.Guard(
                        () => created = Question.CreateForBank(dto.QuestionBankId, sortOrder, spec), "questions");
                    if (string.IsNullOrEmpty(created.TenantId)) created.TenantId = TenantOf(existing);
                    await repository.AddAsync(created);
                }
            }

            foreach (var gone in existing.Where(q => !kept.Contains(q.Id)))
                repository.Delete(gone);

            await repository.SaveChangesAsync();
            logger.LogInformation("Question bank {BankId} now holds {Count} question(s)",
                dto.QuestionBankId, incoming.Count);
        }

        // A brand-new bank has no question to copy the tenant from; the repository's own tenant
        // interceptor fills it in that case, and this only helps when siblings already exist.
        private static string TenantOf(List<Question> siblings) =>
            siblings.FirstOrDefault()?.TenantId ?? string.Empty;
    }

    // ---- Assessments --------------------------------------------------------

    public class GetAssessment(IRepository<Assessment> repository) : IGetAssessment
    {
        public async Task<AssessmentDto?> GetByModuleAsync(Guid contentModuleId)
        {
            var assessment = await repository.GetAll().AsNoTracking()
                .Include(a => a.Questions).ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.ContentModuleId == contentModuleId);

            // Null rather than 404: "this quiz module has no quiz yet" is the normal state right
            // after the module is added, and the authoring screen renders an empty editor for it.
            return assessment is null ? null : ToDto(assessment);
        }

        internal static AssessmentDto ToDto(Assessment a) => new()
        {
            Id = a.Id,
            ContentModuleId = a.ContentModuleId,
            Title = a.Title,
            Instructions = a.Instructions,
            PassMark = a.PassMark,
            MaxAttempts = a.MaxAttempts,
            TimeLimitMinutes = a.TimeLimitMinutes,
            ShuffleQuestions = a.ShuffleQuestions,
            RevealAnswers = a.RevealAnswers,
            QuestionCount = a.Questions.Count,
            TotalPoints = a.TotalPoints,
            Questions = [.. a.Questions.OrderBy(q => q.SortOrder).Select(AssessmentShared.ToDto)]
        };
    }

    /// <summary>
    /// Creates or updates the quiz on a Quiz module.
    ///
    /// <para>⚠️ DRAFT ONLY, checked against the module's version. The assessment is the module's
    /// content, so it freezes exactly when the content does — otherwise a published course could
    /// have its pass mark quietly lowered under the people who already failed it.</para>
    /// </summary>
    public class SaveAssessment(
        IRepository<Assessment> repository,
        IRepository<ContentModule> moduleRepository,
        IRepository<CourseVersion> versionRepository,
        ILogger<SaveAssessment> logger) : ISaveAssessment
    {
        public async Task<Guid> SaveAsync(SaveAssessmentDto dto)
        {
            var module = await moduleRepository.GetAll().AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == dto.ContentModuleId)
                ?? throw new NotFoundException(nameof(ContentModule), dto.ContentModuleId.ToString());

            if (module.Kind != ContentModuleKind.Quiz)
                throw new ValidationException("contentModuleId",
                    $"'{module.Title}' is a {module.Kind} module — only a Quiz module holds an assessment.");

            await AssessmentAuthoring.EnsureDraftAsync(versionRepository, module.CourseVersionId);

            var existing = await repository.GetAll()
                .FirstOrDefaultAsync(a => a.ContentModuleId == dto.ContentModuleId);

            if (existing is not null)
            {
                AssessmentShared.Guard(() => existing.Apply(dto.Title, dto.Instructions, dto.PassMark,
                    dto.MaxAttempts, dto.TimeLimitMinutes, dto.ShuffleQuestions, dto.RevealAnswers), "title");
                repository.UpdateAsync(existing);
                await repository.SaveChangesAsync();
                return existing.Id;
            }

            Assessment created = null!;
            AssessmentShared.Guard(() => created = Assessment.Create(dto.ContentModuleId, dto.Title,
                dto.Instructions, dto.PassMark, dto.MaxAttempts, dto.TimeLimitMinutes,
                dto.ShuffleQuestions, dto.RevealAnswers), "title");
            if (string.IsNullOrEmpty(created.TenantId)) created.TenantId = module.TenantId;
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Assessment created on module {ModuleId}", dto.ContentModuleId);
            return created.Id;
        }
    }

    public class SetAssessmentQuestions(
        IRepository<Assessment> repository,
        IRepository<Question> questionRepository,
        IRepository<ContentModule> moduleRepository,
        IRepository<CourseVersion> versionRepository,
        ILogger<SetAssessmentQuestions> logger) : ISetAssessmentQuestions
    {
        public async Task SetAsync(SaveAssessmentQuestionsDto dto)
        {
            var assessment = await repository.GetAll()
                    .Include(a => a.Questions).ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(a => a.Id == dto.AssessmentId)
                ?? throw new NotFoundException(nameof(Assessment), dto.AssessmentId.ToString());

            await AssessmentAuthoring.EnsureModuleDraftAsync(
                moduleRepository, versionRepository, assessment.ContentModuleId);

            var specs = (dto.Questions ?? []).Select(AssessmentShared.ToSpec).ToList();

            // The old rows go explicitly: SetQuestions clears the aggregate's collection, and EF will
            // not delete children it was never told about on an owned-by-FK collection.
            foreach (var old in assessment.Questions.ToList())
                questionRepository.Delete(old);

            AssessmentShared.Guard(() => assessment.SetQuestions(specs), "questions");

            foreach (var q in assessment.Questions)
            {
                if (string.IsNullOrEmpty(q.TenantId)) q.TenantId = assessment.TenantId;
                foreach (var o in q.Options)
                    if (string.IsNullOrEmpty(o.TenantId)) o.TenantId = assessment.TenantId;
                await questionRepository.AddAsync(q);
            }

            repository.UpdateAsync(assessment);
            await repository.SaveChangesAsync();
            logger.LogInformation("Assessment {Id} now has {Count} question(s) worth {Points} marks",
                assessment.Id, specs.Count, assessment.TotalPoints);
        }
    }

    /// <summary>
    /// Copies questions out of a bank and APPENDS them to the assessment.
    ///
    /// <para>⚠️ Copies, never references. The bank stays editable and the assessment stays frozen —
    /// see <see cref="QuestionBank"/>. Appending rather than replacing is what lets an author build
    /// one quiz from several banks.</para>
    /// </summary>
    public class ImportQuestionsFromBank(
        IRepository<Assessment> repository,
        IRepository<Question> questionRepository,
        IRepository<ContentModule> moduleRepository,
        IRepository<CourseVersion> versionRepository,
        ILogger<ImportQuestionsFromBank> logger) : IImportQuestionsFromBank
    {
        public async Task<int> ImportAsync(ImportQuestionsDto dto)
        {
            var assessment = await repository.GetAll()
                    .Include(a => a.Questions).ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(a => a.Id == dto.AssessmentId)
                ?? throw new NotFoundException(nameof(Assessment), dto.AssessmentId.ToString());

            await AssessmentAuthoring.EnsureModuleDraftAsync(
                moduleRepository, versionRepository, assessment.ContentModuleId);

            var wanted = dto.QuestionIds ?? [];
            var source = await questionRepository.GetAll().AsNoTracking()
                .Include(q => q.Options)
                .Where(q => q.QuestionBankId == dto.QuestionBankId
                    && (wanted.Count == 0 || wanted.Contains(q.Id)))
                .OrderBy(q => q.SortOrder)
                .ToListAsync();

            if (source.Count == 0)
                throw new ValidationException("questionBankId", "That bank has no matching questions to import.");

            var specs = assessment.Questions.OrderBy(q => q.SortOrder).Select(q => q.ToSpec())
                .Concat(source.Select(q => q.ToSpec()))
                .ToList();

            foreach (var old in assessment.Questions.ToList())
                questionRepository.Delete(old);

            AssessmentShared.Guard(() => assessment.SetQuestions(specs), "questions");

            foreach (var q in assessment.Questions)
            {
                if (string.IsNullOrEmpty(q.TenantId)) q.TenantId = assessment.TenantId;
                foreach (var o in q.Options)
                    if (string.IsNullOrEmpty(o.TenantId)) o.TenantId = assessment.TenantId;
                await questionRepository.AddAsync(q);
            }

            repository.UpdateAsync(assessment);
            await repository.SaveChangesAsync();
            logger.LogInformation("Imported {Count} question(s) from bank {BankId} into assessment {Id}",
                source.Count, dto.QuestionBankId, assessment.Id);
            return source.Count;
        }
    }

    internal static class AssessmentAuthoring
    {
        /// <summary>
        /// Refuses to edit a quiz whose version is no longer a draft — the same rule the content
        /// itself obeys, applied at the one place the quiz can be reached from.
        /// </summary>
        internal static async Task EnsureDraftAsync(IRepository<CourseVersion> versions, Guid courseVersionId)
        {
            var status = await versions.GetAll().AsNoTracking()
                .Where(v => v.Id == courseVersionId)
                .Select(v => (CourseVersionStatus?)v.Status)
                .FirstOrDefaultAsync();

            if (status is null)
                throw new NotFoundException(nameof(CourseVersion), courseVersionId.ToString());
            if (status != CourseVersionStatus.Draft)
                throw new ValidationException("courseVersionId",
                    $"This version is {status} — its quizzes cannot be edited. Create a new version instead.");
        }

        internal static async Task EnsureModuleDraftAsync(
            IRepository<ContentModule> modules, IRepository<CourseVersion> versions, Guid contentModuleId)
        {
            var versionId = await modules.GetAll().AsNoTracking()
                .Where(m => m.Id == contentModuleId)
                .Select(m => m.CourseVersionId)
                .FirstOrDefaultAsync();

            if (versionId == Guid.Empty)
                throw new NotFoundException(nameof(ContentModule), contentModuleId.ToString());
            await EnsureDraftAsync(versions, versionId);
        }
    }
}
