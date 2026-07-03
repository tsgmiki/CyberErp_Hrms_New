using CyberErp.Hrms.Dom.Entities;
using System.Linq.Expressions;

namespace CyberErp.Hrms.App.Common.Repositories
{
    public interface IRepository<T> where T : BaseEntity
    {
        IQueryable<T> GetAll();
        IQueryable<T> GetAllWithoutTenantFilter();
        IQueryable<T> GetAll(Expression<Func<T, bool>> predicate);
        Task<T?> GetByIdAsync(Guid id);
        Task AddAsync(T entity);
        void UpdateAsync(T entity);
        void Delete(T entity);
        Task Delete(Expression<Func<T, bool>> predicate);

        Task<int> SaveChangesAsync();
        void MarkPropertyModified(T entity, string propertyName);
        void AddRange(ICollection<T> entities);
        void RemoveRange(ICollection<T> entities);
    }
}

