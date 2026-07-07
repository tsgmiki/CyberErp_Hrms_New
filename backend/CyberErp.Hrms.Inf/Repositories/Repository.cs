using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities;
using CyberErp.Hrms.Inf.Common;
using CyberErp.Hrms.Inf.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CyberErp.Hrms.Inf.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly HrmsDbContext _context;
        private readonly DbSet<T> _dbSet;
        private readonly ITenantService _tenantService;
        private readonly ICurrentUserService _currentUserService;

        public Repository(HrmsDbContext context, ITenantService tenantService, ICurrentUserService currentUserService)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _tenantService = tenantService;
            _currentUserService = currentUserService;
        }

        private IQueryable<T> ApplyTenantFilter(IQueryable<T> query)
        {
            // Skip tenant filter for Tenant and SubscriptionPlan (root entities)
            if (typeof(T).Name == "Tenant" || typeof(T).Name == "SubscriptionPlan")
            {
                return query;
            }

            var tenantId = _tenantService.GetCurrentTenantId();
            if (!string.IsNullOrEmpty(tenantId))
            {
                query = query.Where(e => e.TenantId == tenantId);
            }
            return ApplyBranchFilter(query);
        }

        /// <summary>
        /// Branch-level data isolation: a branch administrator only sees rows for their assigned
        /// branch; Head Office (and users with no branch assignment) bypass the filter and see all.
        /// </summary>
        private IQueryable<T> ApplyBranchFilter(IQueryable<T> query)
        {
            if (_currentUserService.IsHeadOffice())
            {
                return query;
            }

            var branchId = _currentUserService.GetCurrentBranchId();
            if (!branchId.HasValue)
            {
                // Not a branch-scoped user → unrestricted (e.g. tenant owner before branch assignment).
                return query;
            }

            // A branch's own record: the branch admin only sees their branch.
            if (typeof(T).Name == "Branch")
            {
                return query.Where(e => e.Id == branchId.Value);
            }

            // Branch-scoped entities (organization units, positions, audit log) filter by BranchId.
            if (typeof(IBranchScoped).IsAssignableFrom(typeof(T)))
            {
                return query.Where(e => EF.Property<Guid?>(e, "BranchId") == branchId.Value);
            }

            return query;
        }

        private void SetAuditFields(T entity, bool isNew)
        {
            // Skip tenant validation for Tenant (root tenant entity doesn't need a TenantId)
            if (typeof(T).Name == "Tenant" || typeof(T).Name == "SubscriptionPlan")
            {
                if (isNew)
                {
                    entity.Create(_currentUserService.GetCurrentUserName());
                }
                return;
            }

            // Check if TenantId already set on the entity (e.g., during signup when creating new tenant/user)
            var existingTenantId = entity.TenantId;

            // If TenantId is not set, set it from the current tenant service (without validation)
            if (string.IsNullOrEmpty(existingTenantId))
            {
                var tenantId = _tenantService.GetCurrentTenantId();
                if (!string.IsNullOrEmpty(tenantId))
                {
                    entity.TenantId = tenantId;
                }
                // If there's no tenantId, we leave it as null (validation removed)
            }

            // Set CreatedBy for new entities
            if (isNew)
            {
                entity.Create(_currentUserService.GetCurrentUserName());
            }
        }

        public IQueryable<T> GetAll()
        {
            return ApplyTenantFilter(_dbSet);
        }

        public IQueryable<T> GetAllWithoutTenantFilter()
        {
            return _dbSet;
        }

        public IQueryable<T> GetAll(Expression<Func<T, bool>> predicate)
        {
            return ApplyTenantFilter(_dbSet).Where(predicate);
        }
        public async Task Delete(Expression<Func<T, bool>> predicate)
        {
            // Get entities matching the predicate with tenant filter applied
            var entities = await ApplyTenantFilter(_dbSet).Where(predicate).ToListAsync();

            // Remove the entities
            _dbSet.RemoveRange(entities);
        }
        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await ApplyTenantFilter(_dbSet)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddAsync(T entity)
        {
            SetAuditFields(entity, isNew: true);
            await _dbSet.AddAsync(entity);
        }

        public void UpdateAsync(T entity)
        {
            // Set UpdatedBy
            entity.UpdatedBy = _currentUserService.GetCurrentUserName();

            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            if (entry.State != EntityState.Added)
            {
                entry.State = EntityState.Modified;
            }
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            _context.ChangeTracker.DetectChanges();
            return await _context.SaveChangesAsync();
        }

        public void MarkPropertyModified(T entity, string propertyName)
        {
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            entry.Property(propertyName).IsModified = true;
        }

        public void AddRange(ICollection<T> entities)
        {

            foreach (var entity in entities)
            {
                  var entry = _context.Entry(entity);
                SetAuditFields(entity, isNew: true);
                if (entry.State == EntityState.Modified)
                {
                     if (entity.RowVersion == null) 
                    {
                        entry.State = EntityState.Added;
                    }
                }
                else
                {
                     entry.State = EntityState.Added;
                  
                }
            }

        }

        public void RemoveRange(ICollection<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }
    }
}
