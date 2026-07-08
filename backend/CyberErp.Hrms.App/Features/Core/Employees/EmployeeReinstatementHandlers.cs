using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    // ---- DTOs ---------------------------------------------------------------

    /// <summary>
    /// Pre-reinstatement context for the Termination List: the employee's previous position and
    /// whether it is still available, so the UI can either restore it directly or force a new
    /// vacant-position choice.
    /// </summary>
    public class ReinstatementInfoDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public Guid? PreviousPositionId { get; set; }
        public string? PreviousPositionTitle { get; set; }
        /// <summary>True when the previous position still exists and is vacant (can be restored as-is).</summary>
        public bool PreviousPositionAvailable { get; set; }
        /// <summary>Who now holds the previous position, when it is no longer available.</summary>
        public string? PreviousPositionOccupiedBy { get; set; }
    }

    public class ReinstateEmployeeDto
    {
        public Guid EmployeeId { get; set; }
        /// <summary>The (vacant) position to place the reinstated employee on.</summary>
        public Guid PositionId { get; set; }
    }

    public class ReinstateEmployeeDtoValidator : AbstractValidator<ReinstateEmployeeDto>
    {
        public ReinstateEmployeeDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.PositionId).NotEmpty().WithMessage("Select a vacant position to reinstate the employee to.");
        }
    }

    // ---- Interfaces -----------------------------------------------------------

    public interface IGetReinstatementInfo { Task<ReinstatementInfoDto> GetAsync(Guid employeeId); }
    public interface IReinstateEmployee { Task ReinstateAsync(ReinstateEmployeeDto dto); }

    // ---- Reinstatement info (drives the reinstate modal) --------------------------

    public class GetReinstatementInfo(
        IRepository<Employee> employeeRepository,
        IRepository<EmployeeTermination> terminationRepository,
        IRepository<Position> positionRepository) : IGetReinstatementInfo
    {
        public async Task<ReinstatementInfoDto> GetAsync(Guid employeeId)
        {
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, employeeId);

            // Materialize the raw name parts and join in memory (EF can't translate string.Join here).
            var employee = await employeeRepository.GetAll()
                .Where(e => e.Id == employeeId)
                .Select(e => new
                {
                    e.IsTerminated,
                    e.EmploymentStatus,
                    FirstName = e.Person != null ? e.Person.FirstName : null,
                    FatherName = e.Person != null ? e.Person.FatherName : null,
                    GrandFatherName = e.Person != null ? e.Person.GrandFatherName : null
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), employeeId.ToString());

            if (!employee.IsTerminated && employee.EmploymentStatus != EmploymentStatus.Terminated)
                throw new ValidationException("employeeId", "This employee is not terminated.");

            var info = new ReinstatementInfoDto
            {
                EmployeeId = employeeId,
                EmployeeName = JoinName(employee.FirstName, employee.FatherName, employee.GrandFatherName)
            };

            // Previous position snapshotted on the latest settled termination.
            var vacatedPositionId = await terminationRepository.GetAll()
                .Where(t => t.EmployeeId == employeeId && t.Status == TerminationStatus.Settled)
                .OrderByDescending(t => t.SettledAt)
                .Select(t => t.VacatedPositionId)
                .FirstOrDefaultAsync();

            if (vacatedPositionId is null) return info; // no snapshot (legacy case) — UI forces a picker

            var position = await positionRepository.GetAll()
                .Where(p => p.Id == vacatedPositionId.Value)
                .Select(p => new
                {
                    p.Id,
                    p.IsVacant,
                    Title = p.PositionClass != null ? p.PositionClass.Title : p.Code
                })
                .FirstOrDefaultAsync();

            if (position is null) return info; // position was deleted — UI forces a picker

            info.PreviousPositionId = position.Id;
            info.PreviousPositionTitle = position.Title;
            info.PreviousPositionAvailable = position.IsVacant;

            if (!position.IsVacant)
            {
                var occupant = await employeeRepository.GetAll()
                    .Where(e => e.PositionId == position.Id)
                    .Select(e => new
                    {
                        e.EmployeeNumber,
                        FirstName = e.Person != null ? e.Person.FirstName : null,
                        GrandFatherName = e.Person != null ? e.Person.GrandFatherName : null
                    })
                    .FirstOrDefaultAsync();
                info.PreviousPositionOccupiedBy = occupant is null
                    ? null
                    : JoinName(occupant.FirstName, occupant.GrandFatherName) is { Length: > 0 } n ? n : occupant.EmployeeNumber;
            }
            return info;
        }

        private static string JoinName(params string?[] parts) =>
            string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    // ---- Reinstate (system automations) -------------------------------------------

    public class ReinstateEmployee(
        IRepository<Employee> employeeRepository,
        IRepository<EmployeeTermination> terminationRepository,
        IRepository<Position> positionRepository,
        IValidator<ReinstateEmployeeDto> validator,
        ILogger<ReinstateEmployee> logger) : IReinstateEmployee
    {
        public async Task ReinstateAsync(ReinstateEmployeeDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, dto.EmployeeId);

            var employee = await employeeRepository.GetAll().FirstOrDefaultAsync(e => e.Id == dto.EmployeeId)
                ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());
            if (!employee.IsTerminated && employee.EmploymentStatus != EmploymentStatus.Terminated)
                throw new ValidationException("employeeId", "This employee is not terminated.");

            // The target position must exist and be vacant (their old one may now be filled).
            var position = await positionRepository.GetAll().FirstOrDefaultAsync(p => p.Id == dto.PositionId)
                ?? throw new NotFoundException(nameof(Position), dto.PositionId.ToString());
            if (!position.IsVacant)
                throw new ValidationException("positionId", "The selected position is no longer vacant. Choose another vacant position.");

            // Restore placement: Active on the chosen position; branch follows the position; the
            // department/organization unit is derived from it. Salary and pay point are preserved.
            employee.Reinstate(position.Id, position.BranchId);
            employeeRepository.UpdateAsync(employee);
            await EmployeeShared.MarkPositionOccupiedAsync(position.Id, positionRepository);

            // Reverse the latest settled termination case (history is preserved via ReinstatedAt).
            var termination = await terminationRepository.GetAll()
                .Where(t => t.EmployeeId == dto.EmployeeId && t.Status == TerminationStatus.Settled && t.ReinstatedAt == null)
                .OrderByDescending(t => t.SettledAt)
                .FirstOrDefaultAsync();
            if (termination is not null)
            {
                termination.MarkReinstated();
                terminationRepository.UpdateAsync(termination);
            }

            await employeeRepository.SaveChangesAsync();
            logger.LogInformation("Reinstated Employee {EmployeeId} onto Position {PositionId}", dto.EmployeeId, position.Id);
        }
    }
}
