namespace CyberErp.Hrms.Dom.Entities
{
    /// <summary>
    /// Marks an entity as belonging to a branch. The infrastructure repository uses this to
    /// enforce branch-level data isolation: branch administrators only see rows whose
    /// <see cref="BranchId"/> matches their assigned branch, while Head Office bypasses the filter.
    /// A null BranchId denotes a global / Head-Office-level record (invisible to branch admins).
    /// </summary>
    public interface IBranchScoped
    {
        Guid? BranchId { get; }
    }
}
