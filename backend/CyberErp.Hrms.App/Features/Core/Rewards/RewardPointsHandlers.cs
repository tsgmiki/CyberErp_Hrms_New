using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Rewards
{
    // ---- DTOs ---------------------------------------------------------------
    public class RewardPointsTransactionDto
    {
        public Guid Id { get; set; }
        public int Points { get; set; }
        public string Source { get; set; } = string.Empty;
        public Guid? ReferenceId { get; set; }
        public string? Note { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    public class RewardPointsSummaryDto
    {
        public Guid EmployeeId { get; set; }
        public int Balance { get; set; }
        public int Total { get; set; }
        public List<RewardPointsTransactionDto> Data { get; set; } = [];
    }

    public class RedeemRewardPointsDto
    {
        public int Points { get; set; }
        public string? Note { get; set; }
    }

    public class RedeemRewardPointsDtoValidator : AbstractValidator<RedeemRewardPointsDto>
    {
        public RedeemRewardPointsDtoValidator()
        {
            RuleFor(x => x.Points).GreaterThan(0).WithMessage("Redeem a positive number of points.");
            RuleFor(x => x.Note).MaximumLength(500);
        }
    }

    public class LeaderboardRowDto
    {
        public int Rank { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public int Points { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IGetRewardPoints { Task<RewardPointsSummaryDto> GetAsync(Guid? employeeId, GetAllRequest request); }
    /// <summary>HC180 — an employee spends their own points; returns the new balance.</summary>
    public interface IRedeemRewardPoints { Task<int> RedeemAsync(RedeemRewardPointsDto dto); }
    /// <summary>HC209 — top point earners over a window (gamification leaderboard, open to all).</summary>
    public interface IGetPointsLeaderboard { Task<List<LeaderboardRowDto>> GetAsync(int days); }

    // ---- Handlers -----------------------------------------------------------
    public class GetRewardPoints(
        IRepository<RewardPointsTransaction> repository,
        IPerformanceVisibilityService visibility) : IGetRewardPoints
    {
        public async Task<RewardPointsSummaryDto> GetAsync(Guid? employeeId, GetAllRequest request)
        {
            var scope = await visibility.GetScopeAsync();
            var target = employeeId ?? scope.EmployeeId
                ?? throw new ValidationException(nameof(employeeId), "Your account is not linked to an employee record.");
            if (!await visibility.CanAccessEmployeeAsync(target))
                throw new ValidationException(nameof(employeeId), "You do not have access to this employee's points.");

            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking().Where(x => x.EmployeeId == target);
            var balance = await query.SumAsync(x => (int?)x.Points) ?? 0;
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.TransactionDate).ThenByDescending(x => x.CreatedAt)
                .Skip(skip).Take(take)
                .Select(x => new RewardPointsTransactionDto
                {
                    Id = x.Id,
                    Points = x.Points,
                    Source = x.Source.ToString(),
                    ReferenceId = x.ReferenceId,
                    Note = x.Note,
                    TransactionDate = x.TransactionDate
                }).ToListAsync();

            return new RewardPointsSummaryDto { EmployeeId = target, Balance = balance, Total = total, Data = data };
        }
    }

    public class GetPointsLeaderboard(
        IRepository<RewardPointsTransaction> repository,
        IRepository<Employee> employeeRepository) : IGetPointsLeaderboard
    {
        public async Task<List<LeaderboardRowDto>> GetAsync(int days)
        {
            var window = Math.Clamp(days, 7, 366);
            var since = DateTime.UtcNow.Date.AddDays(-window);
            var employees = employeeRepository.GetAll();

            // EARNED points only (positive entries) — redemptions don't cost you your rank.
            var rows = await repository.GetAll().AsNoTracking()
                .Where(x => x.Points > 0 && x.TransactionDate >= since)
                .GroupBy(x => x.EmployeeId)
                .Select(g => new { EmployeeId = g.Key, Points = g.Sum(x => x.Points) })
                .OrderByDescending(x => x.Points)
                .Take(10)
                .Select(x => new LeaderboardRowDto
                {
                    EmployeeId = x.EmployeeId,
                    Points = x.Points,
                    EmployeeName = employees.Where(e => e.Id == x.EmployeeId)
                        .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                        .FirstOrDefault(),
                    EmployeeNumber = employees.Where(e => e.Id == x.EmployeeId)
                        .Select(e => e.EmployeeNumber).FirstOrDefault()
                }).ToListAsync();

            for (var i = 0; i < rows.Count; i++) rows[i].Rank = i + 1;
            return rows;
        }
    }

    public class RedeemRewardPoints(
        IRepository<RewardPointsTransaction> repository,
        IPerformanceVisibilityService visibility,
        IValidator<RedeemRewardPointsDto> validator,
        ILogger<RedeemRewardPoints> logger) : IRedeemRewardPoints
    {
        public async Task<int> RedeemAsync(RedeemRewardPointsDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var scope = await visibility.GetScopeAsync();
            var employeeId = scope.EmployeeId
                ?? throw new ValidationException(nameof(dto.Points), "Your account is not linked to an employee record.");

            var balance = await repository.GetAll()
                .Where(x => x.EmployeeId == employeeId).SumAsync(x => (int?)x.Points) ?? 0;
            if (dto.Points > balance)
                throw new ValidationException(nameof(dto.Points),
                    $"Insufficient points: balance is {balance}, requested {dto.Points}.");

            var txn = RewardPointsTransaction.Create(employeeId, -dto.Points, RewardPointsSource.Redemption,
                DateTime.UtcNow.Date, null, string.IsNullOrWhiteSpace(dto.Note) ? "Points redemption" : dto.Note.Trim());
            await repository.AddAsync(txn);
            await repository.SaveChangesAsync();
            logger.LogInformation("Employee {Employee} redeemed {Points} points", employeeId, dto.Points);
            return balance - dto.Points;
        }
    }
}
