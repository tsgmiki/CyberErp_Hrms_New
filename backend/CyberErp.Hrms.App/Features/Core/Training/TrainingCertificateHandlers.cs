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
    public class TrainingCertificateDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public Guid? TrainingCourseId { get; set; }
        public string? CourseName { get; set; }
        public Guid? TrainingEnrollmentId { get; set; }
        public string CertificateNo { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime IssuedOn { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>Issue the digital certificate for a COMPLETED enrollment (HC200).</summary>
    public class IssueCertificateDto
    {
        public Guid TrainingEnrollmentId { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public string? Title { get; set; }
    }

    /// <summary>Record/edit a certificate manually — e.g. an externally earned credential.</summary>
    public class SaveTrainingCertificateDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid? TrainingCourseId { get; set; }
        public string? CertificateNo { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime IssuedOn { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public string? Notes { get; set; }
    }

    public class RenewCertificateDto
    {
        public DateTime NewExpiresOn { get; set; }
    }

    public class SaveTrainingCertificateDtoValidator : AbstractValidator<SaveTrainingCertificateDto>
    {
        public SaveTrainingCertificateDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
            RuleFor(x => x.CertificateNo).MaximumLength(50);
            RuleFor(x => x.IssuedOn).NotEmpty();
            RuleFor(x => x.Notes).MaximumLength(1000);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IIssueTrainingCertificate { Task<Guid> IssueAsync(IssueCertificateDto dto); }
    public interface ISaveTrainingCertificate { Task<Guid> SaveAsync(SaveTrainingCertificateDto dto); }
    public interface IRenewTrainingCertificate { Task RenewAsync(Guid id, RenewCertificateDto dto); }
    public interface IDeleteTrainingCertificate { Task DeleteAsync(Guid id); }
    public interface IGetAllTrainingCertificates { Task<PaginatedResponse<TrainingCertificateDto>> GetAsync(GetAllRequest request); }
    /// <summary>HC200 — renewal tracking: certificates expiring within the window, soonest first.</summary>
    public interface IGetExpiringTrainingCertificates { Task<List<TrainingCertificateDto>> GetAsync(int days); }

    internal static class TrainingCertificateShared
    {
        internal static string NewCertificateNo() =>
            $"CERT-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        internal static async Task EnsureAdminAsync(IPerformanceVisibilityService visibility)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("access", "Only HR administrators can manage certificates.");
        }

        internal static IQueryable<TrainingCertificateDto> Project(
            IQueryable<EmployeeTrainingCertificate> query,
            IQueryable<Employee> employees,
            IQueryable<TrainingCourse> courses)
        {
            return query.Select(x => new TrainingCertificateDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                EmployeeName = employees.Where(e => e.Id == x.EmployeeId)
                    .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                    .FirstOrDefault(),
                EmployeeNumber = employees.Where(e => e.Id == x.EmployeeId)
                    .Select(e => e.EmployeeNumber).FirstOrDefault(),
                TrainingCourseId = x.TrainingCourseId,
                CourseName = courses.Where(c => c.Id == x.TrainingCourseId).Select(c => c.Name).FirstOrDefault(),
                TrainingEnrollmentId = x.TrainingEnrollmentId,
                CertificateNo = x.CertificateNo,
                Title = x.Title,
                IssuedOn = x.IssuedOn,
                ExpiresOn = x.ExpiresOn,
                Notes = x.Notes
            });
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class IssueTrainingCertificate(
        IRepository<EmployeeTrainingCertificate> repository,
        IRepository<TrainingEnrollment> enrollmentRepository,
        IRepository<TrainingSession> sessionRepository,
        IRepository<TrainingCourse> courseRepository,
        IPerformanceVisibilityService visibility,
        ILogger<IssueTrainingCertificate> logger) : IIssueTrainingCertificate
    {
        public async Task<Guid> IssueAsync(IssueCertificateDto dto)
        {
            var enrollment = await enrollmentRepository.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == dto.TrainingEnrollmentId)
                ?? throw new NotFoundException(nameof(TrainingEnrollment), dto.TrainingEnrollmentId.ToString());

            // Issued by HR or the participant's manager — same authority that records participation.
            var scope = await visibility.GetScopeAsync();
            var isManagerOfEmployee = scope.IsManager && enrollment.EmployeeId != scope.EmployeeId
                && await visibility.CanAccessEmployeeAsync(enrollment.EmployeeId);
            if (!scope.IsAdmin && !isManagerOfEmployee)
                throw new ValidationException(nameof(dto.TrainingEnrollmentId), "Only HR or the employee's manager can issue a certificate.");
            if (enrollment.Status != TrainingEnrollmentStatus.Completed)
                throw new ValidationException(nameof(dto.TrainingEnrollmentId), $"Certificates are issued for COMPLETED enrollments (current: {enrollment.Status}).");

            // One certificate per enrollment — idempotent.
            var existing = await repository.GetAll()
                .Where(c => c.TrainingEnrollmentId == enrollment.Id)
                .Select(c => (Guid?)c.Id).FirstOrDefaultAsync();
            if (existing.HasValue) return existing.Value;

            var course = await sessionRepository.GetAll().AsNoTracking()
                .Where(s => s.Id == enrollment.TrainingSessionId)
                .Join(courseRepository.GetAll(), s => s.TrainingCourseId, c => c.Id, (s, c) => new { c.Id, c.Name })
                .FirstOrDefaultAsync();

            var created = EmployeeTrainingCertificate.Create(
                enrollment.EmployeeId,
                TrainingCertificateShared.NewCertificateNo(),
                string.IsNullOrWhiteSpace(dto.Title) ? $"Certificate of Completion — {course?.Name}" : dto.Title.Trim(),
                enrollment.CompletedOn ?? DateTime.UtcNow.Date,
                dto.ExpiresOn,
                course?.Id,
                enrollment.Id);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Issued certificate {No} for enrollment {Enrollment}", created.CertificateNo, enrollment.Id);
            return created.Id;
        }
    }

    public class SaveTrainingCertificate(
        IRepository<EmployeeTrainingCertificate> repository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveTrainingCertificateDto> validator,
        ILogger<SaveTrainingCertificate> logger) : ISaveTrainingCertificate
    {
        public async Task<Guid> SaveAsync(SaveTrainingCertificateDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            await TrainingCertificateShared.EnsureAdminAsync(visibility);

            if (!await employeeRepository.GetAll().AnyAsync(e => e.Id == dto.EmployeeId))
                throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(EmployeeTrainingCertificate), dto.Id.Value.ToString());
                entity.Update(dto.Title, dto.IssuedOn, dto.ExpiresOn, dto.Notes);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated certificate {Id}", entity.Id);
                return entity.Id;
            }

            var certificateNo = string.IsNullOrWhiteSpace(dto.CertificateNo)
                ? TrainingCertificateShared.NewCertificateNo()
                : dto.CertificateNo.Trim();
            if (await repository.GetAll().AnyAsync(x => x.CertificateNo == certificateNo))
                throw new DuplicateException(nameof(EmployeeTrainingCertificate), nameof(dto.CertificateNo), certificateNo);

            var created = EmployeeTrainingCertificate.Create(dto.EmployeeId, certificateNo, dto.Title,
                dto.IssuedOn, dto.ExpiresOn, dto.TrainingCourseId, null, dto.Notes);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Recorded certificate {No} for employee {Employee}", certificateNo, dto.EmployeeId);
            return created.Id;
        }
    }

    public class RenewTrainingCertificate(
        IRepository<EmployeeTrainingCertificate> repository,
        IPerformanceVisibilityService visibility,
        ILogger<RenewTrainingCertificate> logger) : IRenewTrainingCertificate
    {
        public async Task RenewAsync(Guid id, RenewCertificateDto dto)
        {
            await TrainingCertificateShared.EnsureAdminAsync(visibility);
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeTrainingCertificate), id.ToString());
            entity.Renew(dto.NewExpiresOn);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Renewed certificate {Id} to {Expiry:d}", id, dto.NewExpiresOn);
        }
    }

    public class DeleteTrainingCertificate(
        IRepository<EmployeeTrainingCertificate> repository,
        IPerformanceVisibilityService visibility,
        ILogger<DeleteTrainingCertificate> logger) : IDeleteTrainingCertificate
    {
        public async Task DeleteAsync(Guid id)
        {
            await TrainingCertificateShared.EnsureAdminAsync(visibility);
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeTrainingCertificate), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted certificate {Id}", id);
        }
    }

    public class GetAllTrainingCertificates(
        IRepository<EmployeeTrainingCertificate> repository,
        IRepository<Employee> employeeRepository,
        IRepository<TrainingCourse> courseRepository,
        IPerformanceVisibilityService visibility) : IGetAllTrainingCertificates
    {
        public async Task<PaginatedResponse<TrainingCertificateDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();

            // Role scoping as one SQL predicate: HR all, manager subtree, employee own.
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
            {
                var myEmp = scope.EmployeeId ?? Guid.Empty;
                if (scope.IsManager)
                {
                    var emps = employeeRepository.GetAll();
                    var unitIds = scope.UnitIds;
                    query = query.Where(x => x.EmployeeId == myEmp
                        || emps.Any(e => e.Id == x.EmployeeId && e.Position != null
                            && unitIds.Contains(e.Position.OrganizationUnitId)));
                }
                else
                {
                    query = query.Where(x => x.EmployeeId == myEmp);
                }
            }

            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Title.Contains(term) || x.CertificateNo.Contains(term));
            }

            var total = await query.CountAsync();
            var data = await TrainingCertificateShared.Project(
                    query.OrderByDescending(x => x.IssuedOn).Skip(skip).Take(take),
                    employeeRepository.GetAll(), courseRepository.GetAll())
                .ToListAsync();

            return new PaginatedResponse<TrainingCertificateDto> { Total = total, Data = data };
        }
    }

    public class GetExpiringTrainingCertificates(
        IRepository<EmployeeTrainingCertificate> repository,
        IRepository<Employee> employeeRepository,
        IRepository<TrainingCourse> courseRepository,
        IPerformanceVisibilityService visibility) : IGetExpiringTrainingCertificates
    {
        public async Task<List<TrainingCertificateDto>> GetAsync(int days)
        {
            await TrainingCertificateShared.EnsureAdminAsync(visibility);
            if (days is < 1 or > 366)
                throw new ValidationException(nameof(days), "The window is 1 to 366 days.");

            var today = DateTime.UtcNow.Date;
            var horizon = today.AddDays(days);
            // Ordered read off the (TenantId, ExpiresOn) index — includes already-lapsed certificates.
            return await TrainingCertificateShared.Project(
                    repository.GetAll().AsNoTracking()
                        .Where(x => x.ExpiresOn != null && x.ExpiresOn <= horizon)
                        .OrderBy(x => x.ExpiresOn).Take(200),
                    employeeRepository.GetAll(), courseRepository.GetAll())
                .ToListAsync();
        }
    }
}
