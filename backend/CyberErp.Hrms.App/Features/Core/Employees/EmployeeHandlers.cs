using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Employees.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    public interface ICreateEmployee { Task<Guid> CreateAsync(CreateEmployeeDto dto); }
    public interface IUpdateEmployee { Task UpdateAsync(UpdateEmployeeDto dto); }
    public interface IDeleteEmployee { Task DeleteAsync(Guid id); }
    public interface IGetEmployeeById { Task<EmployeeDto> GetAsync(Guid id); }
    public interface IGetAllEmployees { Task<PaginatedResponse<EmployeeDto>> GetAsync(GetAllRequest request); }

    // ---- Shared helpers -------------------------------------------------------

    internal static class EmployeeShared
    {
        internal static readonly System.Linq.Expressions.Expression<Func<Employee, EmployeeDto>> Projection = e => new EmployeeDto
        {
            Id = e.Id,
            PersonId = e.PersonId,
            EmployeeNumber = e.EmployeeNumber,
            // Personal identity comes from the shared person record (Core.CorePerson).
            FirstName = e.Person != null ? e.Person.FirstName : string.Empty,
            FirstNameA = e.Person != null ? e.Person.FirstNameA : null,
            FatherName = e.Person != null ? e.Person.FatherName : null,
            FatherNameA = e.Person != null ? e.Person.FatherNameA : null,
            GrandFatherName = e.Person != null ? e.Person.GrandFatherName : string.Empty,
            GrandFatherNameA = e.Person != null ? e.Person.GrandFatherNameA : null,
            Gender = e.Person != null ? e.Person.Gender.ToString() : string.Empty,
            MaritalStatus = e.Person != null ? e.Person.MaritalStatusId.ToString() : string.Empty,
            NationalityId = e.Person != null ? e.Person.NationalityId : null,
            PhoneNumber = e.Person != null ? e.Person.PhoneNumber : null,
            LocationName = e.Person != null ? e.Person.LocationName : null,
            EmploymentStatus = e.EmploymentStatus.ToString(),
            DateOfBirth = e.DateOfBirth,
            PlaceOfBirth = e.PlaceOfBirth,
            SpouseName = e.SpouseName,
            Email = e.Email,
            PhotoUrl = e.PhotoUrl,
            NationalId = e.NationalId,
            Tin = e.Tin,
            PensionNumber = e.PensionNumber,
            HireDate = e.HireDate,
            JobGradeId = e.JobGradeId,
            JobGradeName = e.JobGrade != null ? e.JobGrade.Name : null,
            Salary = e.Salary,
            PositionId = e.PositionId,
            PositionCode = e.Position != null ? e.Position.Code : null,
            PositionClassTitle = e.Position != null && e.Position.PositionClass != null ? e.Position.PositionClass.Title : null,
            // Organization unit is derived from the position (not stored on the employee).
            OrganizationUnitId = e.Position != null ? e.Position.OrganizationUnitId : null,
            OrganizationUnitName = e.Position != null && e.Position.OrganizationUnit != null ? e.Position.OrganizationUnit.Name : null,
            BranchId = e.BranchId,
            BranchName = e.Branch != null ? e.Branch.Name : null
        };

        internal static void SetFullName(EmployeeDto dto) =>
            dto.FullName = string.Join(" ",
                new[] { dto.FirstName, dto.FatherName, dto.GrandFatherName }.Where(p => !string.IsNullOrWhiteSpace(p)));

        /// <summary>
        /// Resolves the branch from the assigned position (whose branch follows its organization
        /// unit). Employees without a position are global / unassigned.
        /// </summary>
        internal static async Task<Guid?> ResolveBranchAsync(CreateEmployeeDto dto, IRepository<Position> positions)
        {
            if (!dto.PositionId.HasValue) return null;

            var pos = await positions.GetAll()
                .Where(p => p.Id == dto.PositionId.Value)
                .Select(p => new { p.BranchId })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Position), dto.PositionId.Value.ToString());
            return pos.BranchId;
        }

        internal static async Task EnsureReferencesExistAsync(
            CreateEmployeeDto dto,
            IRepository<Position> positions,
            IRepository<JobGrade> jobGrades)
        {
            if (dto.PositionId.HasValue && !await positions.GetAll().AnyAsync(p => p.Id == dto.PositionId.Value))
                throw new NotFoundException(nameof(Position), dto.PositionId.Value.ToString());
            if (dto.JobGradeId.HasValue && !await jobGrades.GetAll().AnyAsync(g => g.Id == dto.JobGradeId.Value))
                throw new NotFoundException(nameof(JobGrade), dto.JobGradeId.Value.ToString());
        }

        /// <summary>
        /// Custom-field engine (HC021): validates submitted values against the active definitions
        /// (unknown names rejected, required fields enforced) and upserts one value row per field.
        /// A null dictionary leaves stored values untouched (partial API clients).
        /// </summary>
        internal static async Task ApplyCustomFieldsAsync(
            Guid employeeId,
            Dictionary<string, string?>? submitted,
            IRepository<EmployeeFieldDefinition> definitions,
            IRepository<EmployeeFieldValue> values)
        {
            if (submitted is null) return;

            var activeDefs = await definitions.GetAll()
                .Where(d => d.IsActive)
                .ToListAsync();
            var defsByName = activeDefs.ToDictionary(d => d.Name, StringComparer.OrdinalIgnoreCase);

            var unknown = submitted.Keys.Where(k => !defsByName.ContainsKey(k)).ToList();
            if (unknown.Count > 0)
                throw new ValidationException("customFields", $"Unknown employee field(s): {string.Join(", ", unknown)}");

            foreach (var def in activeDefs.Where(d => d.IsRequired))
            {
                submitted.TryGetValue(def.Name, out var v);
                if (string.IsNullOrWhiteSpace(v))
                    throw new ValidationException(def.Name, $"'{def.Label}' is required.");
            }

            var existing = await values.GetAll()
                .Where(v => v.EmployeeId == employeeId)
                .ToListAsync();

            foreach (var (name, value) in submitted)
            {
                var def = defsByName[name];
                var row = existing.FirstOrDefault(v => v.FieldDefinitionId == def.Id);
                if (row is null)
                {
                    await values.AddAsync(EmployeeFieldValue.Create(employeeId, def.Id, value));
                }
                else if (row.Value != value)
                {
                    row.SetValue(value);
                    values.UpdateAsync(row);
                }
            }
        }

        /// <summary>Marks a position occupied (not vacant) because an employee was just assigned to it.</summary>
        internal static async Task MarkPositionOccupiedAsync(Guid? positionId, IRepository<Position> positions)
        {
            if (!positionId.HasValue) return;
            var pos = await positions.GetAll().FirstOrDefaultAsync(p => p.Id == positionId.Value);
            if (pos is null || !pos.IsVacant) return;
            pos.SetVacant(false);
            positions.UpdateAsync(pos);
        }

        /// <summary>
        /// Recomputes a position's vacancy after an employee left it (reassigned or deleted): the
        /// position is vacant again unless another employee still occupies it. <paramref name="excludeEmployeeId"/>
        /// skips the leaving employee, whose row may not be flushed to the database yet.
        /// </summary>
        internal static async Task RecomputePositionVacancyAsync(
            Guid? positionId,
            Guid excludeEmployeeId,
            IRepository<Position> positions,
            IRepository<Employee> employees)
        {
            if (!positionId.HasValue) return;
            var pos = await positions.GetAll().FirstOrDefaultAsync(p => p.Id == positionId.Value);
            if (pos is null) return;

            var stillOccupied = await employees.GetAll()
                .AnyAsync(e => e.PositionId == positionId.Value && e.Id != excludeEmployeeId);
            pos.SetVacant(!stillOccupied);
            positions.UpdateAsync(pos);
        }
    }

    // ---- Create ---------------------------------------------------------------

    public class CreateEmployee(
        IRepository<Employee> repository,
        IRepository<Person> personRepository,
        IRepository<Position> positionRepository,
        IRepository<JobGrade> jobGradeRepository,
        IRepository<EmployeeFieldDefinition> fieldDefinitionRepository,
        IRepository<EmployeeFieldValue> fieldValueRepository,
        IValidator<CreateEmployeeDto> validator,
        ILogger<CreateEmployee> logger) : ICreateEmployee
    {
        public async Task<Guid> CreateAsync(CreateEmployeeDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.EmployeeNumber == dto.EmployeeNumber))
                throw new DuplicateException(nameof(Employee), nameof(dto.EmployeeNumber), dto.EmployeeNumber);

            await EmployeeShared.EnsureReferencesExistAsync(dto, positionRepository, jobGradeRepository);
            var branchId = await EmployeeShared.ResolveBranchAsync(dto, positionRepository);

            // Person + employee are inserted in ONE SaveChanges — a single database transaction,
            // so a failure on either side rolls back both.
            var person = Person.Create(
                dto.FirstName, dto.GrandFatherName,
                Enum.Parse<Gender>(dto.Gender), Enum.Parse<MaritalStatus>(dto.MaritalStatus),
                dto.FatherName, dto.FirstNameA, dto.FatherNameA, dto.GrandFatherNameA,
                dto.NationalityId, dto.PhoneNumber, dto.LocationName);
            await personRepository.AddAsync(person);

            var entity = Employee.Create(
                person.Id, dto.EmployeeNumber,
                Enum.Parse<EmploymentStatus>(dto.EmploymentStatus),
                dto.DateOfBirth, dto.PlaceOfBirth, dto.SpouseName, dto.Email,
                dto.NationalId, dto.Tin, dto.PensionNumber,
                dto.HireDate, dto.PositionId, dto.JobGradeId, dto.Salary, branchId);
            await repository.AddAsync(entity);

            await EmployeeShared.ApplyCustomFieldsAsync(entity.Id, dto.CustomFields, fieldDefinitionRepository, fieldValueRepository);

            // The assigned position is now occupied.
            await EmployeeShared.MarkPositionOccupiedAsync(dto.PositionId, positionRepository);

            await repository.SaveChangesAsync();
            logger.LogInformation("Created Employee {Id} ({Number}) with Person {PersonId}", entity.Id, entity.EmployeeNumber, person.Id);
            return entity.Id;
        }
    }

    // ---- Update ---------------------------------------------------------------

    public class UpdateEmployee(
        IRepository<Employee> repository,
        IRepository<Person> personRepository,
        IRepository<Position> positionRepository,
        IRepository<JobGrade> jobGradeRepository,
        IRepository<EmployeeFieldDefinition> fieldDefinitionRepository,
        IRepository<EmployeeFieldValue> fieldValueRepository,
        IValidator<UpdateEmployeeDto> validator,
        ILogger<UpdateEmployee> logger) : IUpdateEmployee
    {
        public async Task UpdateAsync(UpdateEmployeeDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(Employee), dto.Id.ToString());
            var oldPositionId = entity.PositionId;

            if (await repository.GetAll().AnyAsync(x => x.EmployeeNumber == dto.EmployeeNumber && x.Id != dto.Id))
                throw new DuplicateException(nameof(Employee), nameof(dto.EmployeeNumber), dto.EmployeeNumber);

            await EmployeeShared.EnsureReferencesExistAsync(dto, positionRepository, jobGradeRepository);
            var branchId = await EmployeeShared.ResolveBranchAsync(dto, positionRepository);

            // Person + employee update in the same SaveChanges (one transaction).
            var person = await personRepository.GetAll().FirstOrDefaultAsync(p => p.Id == entity.PersonId)
                ?? throw new NotFoundException(nameof(Person), entity.PersonId.ToString());
            person.Update(
                dto.FirstName, dto.FatherName, dto.GrandFatherName,
                Enum.Parse<Gender>(dto.Gender), Enum.Parse<MaritalStatus>(dto.MaritalStatus),
                dto.FirstNameA, dto.FatherNameA, dto.GrandFatherNameA,
                dto.NationalityId, dto.PhoneNumber, dto.LocationName);
            personRepository.UpdateAsync(person);

            entity.Update(
                dto.EmployeeNumber,
                Enum.Parse<EmploymentStatus>(dto.EmploymentStatus),
                dto.DateOfBirth, dto.PlaceOfBirth, dto.SpouseName, dto.Email,
                dto.NationalId, dto.Tin, dto.PensionNumber,
                dto.HireDate, dto.PositionId, dto.JobGradeId, dto.Salary, branchId);
            repository.UpdateAsync(entity);

            await EmployeeShared.ApplyCustomFieldsAsync(entity.Id, dto.CustomFields, fieldDefinitionRepository, fieldValueRepository);

            // Keep position vacancy in sync when the placement changed: the vacated position may
            // reopen, and the newly assigned position becomes occupied.
            if (oldPositionId != dto.PositionId)
            {
                await EmployeeShared.RecomputePositionVacancyAsync(oldPositionId, entity.Id, positionRepository, repository);
                await EmployeeShared.MarkPositionOccupiedAsync(dto.PositionId, positionRepository);
            }

            await repository.SaveChangesAsync();
            logger.LogInformation("Updated Employee {Id}", entity.Id);
        }
    }

    // ---- Delete ---------------------------------------------------------------

    public class DeleteEmployee(
        IRepository<Employee> repository,
        IRepository<EmployeeDependent> dependentRepository,
        IRepository<Position> positionRepository,
        ILogger<DeleteEmployee> logger) : IDeleteEmployee
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Employee), id.ToString());
            var positionId = entity.PositionId;

            // Children (education/experience/dependents/field values) cascade with the employee,
            // but other employees' family entries may point here (internal relationships, HC020).
            if (await dependentRepository.GetAll().AnyAsync(d => d.RelatedEmployeeId == id))
                throw new ValidationException(nameof(id),
                    "Other employees' family records reference this employee. Remove those references first.");

            repository.Delete(entity);

            // The vacated position reopens unless another employee still holds it.
            await EmployeeShared.RecomputePositionVacancyAsync(positionId, id, positionRepository, repository);

            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted Employee {Id}", id);
        }
    }

    // ---- Get by id (full profile incl. custom fields) ---------------------------

    public class GetEmployeeById(
        IRepository<Employee> repository,
        IRepository<EmployeeFieldValue> fieldValueRepository,
        IRepository<EmployeeFieldDefinition> fieldDefinitionRepository) : IGetEmployeeById
    {
        public async Task<EmployeeDto> GetAsync(Guid id)
        {
            var dto = await repository.GetAll()
                .Where(e => e.Id == id)
                .Select(EmployeeShared.Projection)
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), id.ToString());
            EmployeeShared.SetFullName(dto);

            dto.CustomFields = await fieldValueRepository.GetAll()
                .Where(v => v.EmployeeId == id)
                .Join(fieldDefinitionRepository.GetAll(),
                    v => v.FieldDefinitionId, d => d.Id,
                    (v, d) => new { d.Name, v.Value })
                .ToDictionaryAsync(x => x.Name, x => x.Value);

            return dto;
        }
    }

    // ---- Get all (paged) --------------------------------------------------------

    public class GetAllEmployees(IRepository<Employee> repository) : IGetAllEmployees
    {
        public async Task<PaginatedResponse<EmployeeDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            // parentId scopes to an organization unit, derived through the assigned position
            // (the org unit is not stored on the employee).
            if (request.ParentId.HasValue)
                query = query.Where(x => x.Position != null && x.Position.OrganizationUnitId == request.ParentId.Value);

            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<EmploymentStatus>(request.Status, out var status))
                query = query.Where(x => x.EmploymentStatus == status);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x =>
                    x.EmployeeNumber.Contains(term) ||
                    (x.Person != null && (
                        x.Person.FirstName.Contains(term) ||
                        (x.Person.FatherName != null && x.Person.FatherName.Contains(term)) ||
                        x.Person.GrandFatherName.Contains(term) ||
                        (x.Person.FirstNameA != null && x.Person.FirstNameA.Contains(term)) ||
                        (x.Person.FatherNameA != null && x.Person.FatherNameA.Contains(term)) ||
                        (x.Person.GrandFatherNameA != null && x.Person.GrandFatherNameA.Contains(term)))) ||
                    (x.Email != null && x.Email.Contains(term)));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.EmployeeNumber)
                .Skip(skip).Take(take)
                .Select(EmployeeShared.Projection)
                .ToListAsync();
            foreach (var row in data) EmployeeShared.SetFullName(row);

            return new PaginatedResponse<EmployeeDto> { Total = total, Data = data };
        }
    }
}
