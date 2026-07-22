using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Loans
{
    // ================= Loan Type (HC251) =================
    public class LoanTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? MaxAmount { get; set; }
        public decimal? MaxSalaryMultiple { get; set; }
        public int MaxTermMonths { get; set; }
        public decimal InterestRatePct { get; set; }
        public bool RequiresGuarantor { get; set; }
        public int MinGuarantors { get; set; }
        public int ServiceCommitmentMonths { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveLoanTypeDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? MaxAmount { get; set; }
        public decimal? MaxSalaryMultiple { get; set; }
        public int MaxTermMonths { get; set; } = 12;
        public decimal InterestRatePct { get; set; }
        public bool RequiresGuarantor { get; set; }
        public int MinGuarantors { get; set; }
        public int ServiceCommitmentMonths { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SaveLoanTypeDtoValidator : AbstractValidator<SaveLoanTypeDto>
    {
        public SaveLoanTypeDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.MaxTermMonths).GreaterThan(0);
            RuleFor(x => x.InterestRatePct).GreaterThanOrEqualTo(0);
            RuleFor(x => x.MinGuarantors).GreaterThanOrEqualTo(0);
            RuleFor(x => x.ServiceCommitmentMonths).GreaterThanOrEqualTo(0);
            RuleFor(x => x.MaxAmount).GreaterThanOrEqualTo(0).When(x => x.MaxAmount.HasValue);
            RuleFor(x => x.MaxSalaryMultiple).GreaterThanOrEqualTo(0).When(x => x.MaxSalaryMultiple.HasValue);
        }
    }

    public interface ISaveLoanType { Task<Guid> SaveAsync(SaveLoanTypeDto dto); }
    public interface IDeleteLoanType { Task DeleteAsync(Guid id); }
    public interface IGetLoanTypeById { Task<LoanTypeDto> GetAsync(Guid id); }
    public interface IGetAllLoanTypes { Task<PaginatedResponse<LoanTypeDto>> GetAsync(GetAllRequest request); }

    internal static class LoanTypeMapper
    {
        internal static readonly System.Linq.Expressions.Expression<Func<LoanType, LoanTypeDto>> Projection = t => new LoanTypeDto
        {
            Id = t.Id, Name = t.Name, Description = t.Description, MaxAmount = t.MaxAmount, MaxSalaryMultiple = t.MaxSalaryMultiple,
            MaxTermMonths = t.MaxTermMonths, InterestRatePct = t.InterestRatePct, RequiresGuarantor = t.RequiresGuarantor,
            MinGuarantors = t.MinGuarantors, ServiceCommitmentMonths = t.ServiceCommitmentMonths, IsActive = t.IsActive
        };
    }

    public class SaveLoanType(
        IRepository<LoanType> repository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveLoanTypeDto> validator) : ISaveLoanType
    {
        public async Task<Guid> SaveAsync(SaveLoanTypeDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can manage loan types.");
            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(LoanType), nameof(dto.Name), dto.Name);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(LoanType), dto.Id.Value.ToString());
                entity.Update(dto.Name, dto.Description, dto.MaxAmount, dto.MaxSalaryMultiple, dto.MaxTermMonths,
                    dto.InterestRatePct, dto.RequiresGuarantor, dto.MinGuarantors, dto.ServiceCommitmentMonths, dto.IsActive);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }
            var created = LoanType.Create(dto.Name, dto.Description, dto.MaxAmount, dto.MaxSalaryMultiple, dto.MaxTermMonths,
                dto.InterestRatePct, dto.RequiresGuarantor, dto.MinGuarantors, dto.ServiceCommitmentMonths, dto.IsActive);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            return created.Id;
        }
    }

    public class DeleteLoanType(
        IRepository<LoanType> repository,
        IRepository<Loan> loanRepository,
        IPerformanceVisibilityService visibility) : IDeleteLoanType
    {
        public async Task DeleteAsync(Guid id)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can manage loan types.");
            var entity = await repository.GetByIdAsync(id) ?? throw new NotFoundException(nameof(LoanType), id.ToString());
            if (await loanRepository.GetAll().AnyAsync(l => l.LoanTypeId == id))
                throw new ValidationException(nameof(id), "Cannot delete a loan type that has loans.");
            repository.Delete(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class GetLoanTypeById(IRepository<LoanType> repository) : IGetLoanTypeById
    {
        public async Task<LoanTypeDto> GetAsync(Guid id) =>
            await repository.GetAll().AsNoTracking().Where(x => x.Id == id).Select(LoanTypeMapper.Projection).FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(LoanType), id.ToString());
    }

    public class GetAllLoanTypes(IRepository<LoanType> repository) : IGetAllLoanTypes
    {
        public async Task<PaginatedResponse<LoanTypeDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;
            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Name.Contains(request.SearchText.Trim()));
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);
            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.Name).Skip(skip).Take(take).Select(LoanTypeMapper.Projection).ToListAsync();
            return new PaginatedResponse<LoanTypeDto> { Total = total, Data = data };
        }
    }
}
