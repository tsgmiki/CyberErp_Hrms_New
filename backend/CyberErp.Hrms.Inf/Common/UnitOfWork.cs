using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Inf.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.Inf.Common;

public class UnitOfWork(DbContext db) : IUnitOfWork
{
    private readonly DbContext _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}