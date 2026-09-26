using CyberErp.Hrms.App.Common;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.OrganizationUnits.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.OrganizationUnits
{
    public interface ICreateOrganizationUnit { Task<Guid> CreateAsync(CreateOrganizationUnitDto dto); }
    public interface IUpdateOrganizationUnit { Task UpdateAsync(UpdateOrganizationUnitDto dto); }
    public interface IDeleteOrganizationUnit { Task DeleteAsync(Guid id); }
    public interface IGetOrganizationUnitById { Task<OrganizationUnitDto> GetAsync(Guid id); }
    public interface IGetAllOrganizationUnits { Task<PaginatedResponse<OrganizationUnitDto>> GetAsync(GetAllRequest request); }
    public interface IGetMyOrganizationUnits { Task<PaginatedResponse<OrganizationUnitDto>> GetAsync(GetAllRequest request); }
    public interface IGetOrganizationTree { Task<List<OrgUnitTreeNodeDto>> GetAsync(); }
    public interface IMoveOrganizationUnit { Task MoveAsync(MoveOrganizationUnitDto dto); }

    public class CreateOrganizationUnit(
        IRepository<OrganizationUnit> repository,
        ICurrentUserService currentUser,
        IValidator<CreateOrganizationUnitDto> validator,
        ILogger<CreateOrganizationUnit> logger) : ICreateOrganizationUnit
    {
        public async Task<Guid> CreateAsync(CreateOrganizationUnitDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Branch admins are pinned to their branch; Head Office chooses the branch freely.
            var branchId = currentUser.IsHeadOffice() ? dto.BranchId : currentUser.GetCurrentBranchId();

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code && x.BranchId == branchId))
                throw new DuplicateException(nameof(OrganizationUnit), nameof(dto.Code), dto.Code);

            if (dto.ParentId.HasValue && !await repository.GetAll().AnyAsync(x => x.Id == dto.ParentId.Value))
                throw new NotFoundException(nameof(OrganizationUnit), dto.ParentId.Value.ToString(), "Parent organization unit was not found.");

            var type = Enum.Parse<OrganizationUnitType>(dto.UnitType);
            var entity = OrganizationUnit.Create(dto.Code, dto.Name, type, branchId, dto.ParentId, dto.WorkLocationId, dto.AllocatedHeadcount, dto.Description, dto.IsActive);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created OrganizationUnit {Id} ({Code}) in branch {BranchId}", entity.Id, entity.Code, branchId);
            return entity.Id;
        }
    }

    public class UpdateOrganizationUnit(
        IRepository<OrganizationUnit> repository,
        ICurrentUserService currentUser,
        IValidator<UpdateOrganizationUnitDto> validator,
        ILogger<UpdateOrganizationUnit> logger) : IUpdateOrganizationUnit
    {
        public async Task UpdateAsync(UpdateOrganizationUnitDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Load tracked so EF keeps the real original RowVersion for the concurrency check.
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(OrganizationUnit), dto.Id.ToString());

            // Head Office may reassign to another branch; a branch admin stays inside theirs.
            // The rule itself is deliberate branch isolation — what was wrong is that breaking it
            // used to be SILENT: the submitted BranchId was dropped and the call still answered
            // 200, so the user was told the save succeeded and watched the field revert. Say no
            // out loud instead, and only when the value would actually change (an unchanged or
            // omitted BranchId is not an attempt to reassign, so it must not fail an ordinary edit).
            Guid? branchId;
            if (currentUser.IsHeadOffice())
            {
                branchId = dto.BranchId;
            }
            else
            {
                if (dto.BranchId.HasValue && dto.BranchId != entity.BranchId)
                    throw new ValidationException(nameof(dto.BranchId),
                        "Only a Head Office user can move an organization unit to a different branch.");
                branchId = entity.BranchId;
            }

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code && x.BranchId == branchId && x.Id != dto.Id))
                throw new DuplicateException(nameof(OrganizationUnit), nameof(dto.Code), dto.Code);

            if (dto.ParentId.HasValue)
            {
                var parentMap = await repository.GetAll()
                    .Select(x => new { x.Id, x.ParentId })
                    .ToDictionaryAsync(x => x.Id, x => x.ParentId);
                if (HierarchyGuard.WouldCreateCycle(parentMap, dto.Id, dto.ParentId))
                    throw new ValidationException(nameof(dto.ParentId), "The selected parent would create a cycle in the organization hierarchy.");
            }

            var type = Enum.Parse<OrganizationUnitType>(dto.UnitType);
            entity.Update(dto.Code, dto.Name, type, branchId, dto.ParentId, dto.WorkLocationId, dto.AllocatedHeadcount, dto.Description, dto.IsActive);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Updated OrganizationUnit {Id}", entity.Id);
        }
    }

    public class DeleteOrganizationUnit(
        IRepository<OrganizationUnit> repository,
        IRepository<Position> positionRepository,
        ILogger<DeleteOrganizationUnit> logger) : IDeleteOrganizationUnit
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(OrganizationUnit), id.ToString());

            if (await repository.GetAll().AnyAsync(x => x.ParentId == id))
                throw new ValidationException(nameof(id), "Cannot delete an organization unit that has child units. Remove or reassign them first.");

            if (await positionRepository.GetAll().AnyAsync(p => p.OrganizationUnitId == id))
                throw new ValidationException(nameof(id), "Cannot delete an organization unit that has positions. Remove or reassign them first.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted OrganizationUnit {Id}", id);
        }
    }

    /// <summary>
    /// One drag-and-drop: reparent a unit and place it among its new siblings.
    /// </summary>
    /// <remarks>
    /// <para>Separate from <see cref="UpdateOrganizationUnit"/> on purpose. An edit posts the whole
    /// unit; a drag knows only where it was dropped. Routing the drag through Update would mean the
    /// client sending back every field it did not touch, and any field it got wrong — or simply had
    /// stale — would be written as if the user had typed it.</para>
    ///
    /// <para>⚠️ BranchId is deliberately NOT changed by a move. A branch unit legitimately hangs
    /// under a head-office parent, so inheriting the parent's branch would silently re-home units
    /// (and for a branch admin, silently move them out of their own scope). Branch reassignment
    /// stays an explicit edit, where it is already guarded.</para>
    ///
    /// <para>⚠️ Nor does a move check UnitType ordering. Nothing stops the edit form putting a
    /// Directorate under a Team today, and a drag that refused what the form allows would just read
    /// as a broken drag. If that rule is wanted it belongs in both places at once.</para>
    /// </remarks>
    public class MoveOrganizationUnit(
        IRepository<OrganizationUnit> repository,
        IValidator<MoveOrganizationUnitDto> validator,
        ILogger<MoveOrganizationUnit> logger) : IMoveOrganizationUnit
    {
        public async Task MoveAsync(MoveOrganizationUnitDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Tracked, so the RowVersion concurrency token is the real one. GetAll() also applies
            // the tenant and branch filters, so a unit outside the caller's scope is simply not
            // found here rather than being silently moved.
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(OrganizationUnit), dto.Id.ToString());

            // The whole level map, in one read: it answers "does the parent exist", "is the anchor
            // really a sibling" and "would this be a cycle" without three separate round trips.
            var all = await repository.GetAll()
                .Select(x => new { x.Id, x.ParentId, x.SortOrder, x.UnitType, x.Name })
                .ToListAsync();
            var parentMap = all.ToDictionary(x => x.Id, x => x.ParentId);

            if (dto.ParentId.HasValue && !parentMap.ContainsKey(dto.ParentId.Value))
                throw new NotFoundException(nameof(OrganizationUnit), dto.ParentId.Value.ToString(),
                    "The unit it was dropped on was not found.");

            // ⚠️ THE GUARD THAT MATTERS FOR DRAG-AND-DROP. Dropping a unit onto its own descendant
            // detaches that whole subtree from the tree: it still exists, every row still has a
            // parent, and nothing in the schema is violated — it simply stops being reachable from
            // any root, so it vanishes from the org chart and from every manager climb that walks
            // ParentId. A mouse can do it in one gesture, which is exactly why this is checked
            // server-side and not only in the UI.
            if (HierarchyGuard.WouldCreateCycle(parentMap, dto.Id, dto.ParentId))
                throw new ValidationException(nameof(dto.ParentId),
                    "A unit cannot be moved inside one of its own sub-units.");

            if (dto.AfterId.HasValue)
            {
                if (!parentMap.TryGetValue(dto.AfterId.Value, out var anchorParent))
                    throw new NotFoundException(nameof(OrganizationUnit), dto.AfterId.Value.ToString(),
                        "The unit it was dropped after was not found.");

                // A stale client tree is the normal way this fails: the anchor was itself moved
                // elsewhere since the page loaded. Refusing beats quietly dropping the unit at a
                // position the user never pointed at.
                if (anchorParent != dto.ParentId)
                    throw new ValidationException(nameof(dto.AfterId),
                        "The hierarchy changed while you were dragging. Refresh and try again.");
            }

            // Siblings of the DESTINATION in their current order. Same tie-breakers as the tree
            // query, so the ranks handed back describe the level the user was actually looking at.
            var siblings = all
                .Where(x => x.ParentId == dto.ParentId)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.UnitType).ThenBy(x => x.Name)
                .Select(x => x.Id);

            // Renumber the whole level. Cheap — a level is a handful of rows — and it keeps the
            // gaps even, so repeated dragging never converges on adjacent integers with nowhere
            // left to insert between them.
            var order = SiblingOrder.Place(siblings, dto.Id, dto.AfterId);
            var moved = order.First(o => o.Id == dto.Id);

            entity.MoveTo(dto.ParentId, moved.Rank);
            repository.UpdateAsync(entity);

            foreach (var (id, rank) in order.Where(o => o.Id != dto.Id))
            {
                var sibling = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id);
                if (sibling is null) continue;
                sibling.SetSortOrder(rank);   // no-op when the rank is unchanged
                repository.UpdateAsync(sibling);
            }

            await repository.SaveChangesAsync();
            logger.LogInformation(
                "Moved OrganizationUnit {Id} under {ParentId} at rank {Rank} ({Siblings} siblings resequenced)",
                dto.Id, dto.ParentId, moved.Rank, order.Count - 1);
        }
    }

    public class GetOrganizationUnitById(IRepository<OrganizationUnit> repository) : IGetOrganizationUnitById
    {
        public async Task<OrganizationUnitDto> GetAsync(Guid id)
        {
            var dto = await repository.GetAll()
                .Where(o => o.Id == id)
                .Select(o => new OrganizationUnitDto
                {
                    Id = o.Id,
                    Code = o.Code,
                    Name = o.Name,
                    UnitType = o.UnitType.ToString(),
                    BranchId = o.BranchId,
                    BranchName = o.Branch != null ? o.Branch.Name : null,
                    ParentId = o.ParentId,
                    ParentName = o.Parent != null ? o.Parent.Name : null,
                    WorkLocationId = o.WorkLocationId,
                    WorkLocationName = o.WorkLocation != null ? o.WorkLocation.Name : null,
                    AllocatedHeadcount = o.AllocatedHeadcount,
                    Description = o.Description,
                    IsActive = o.IsActive
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(OrganizationUnit), id.ToString());

            dto.HasChildren = await repository.GetAll().AnyAsync(x => x.ParentId == id);
            return dto;
        }
    }

    public class GetAllOrganizationUnits(IRepository<OrganizationUnit> repository) : IGetAllOrganizationUnits
    {
        public async Task<PaginatedResponse<OrganizationUnitDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            if (request.IsRoot == true)
                query = query.Where(x => x.ParentId == null);
            else if (request.ParentId.HasValue)
                query = query.Where(x => x.ParentId == request.ParentId.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || x.Code.Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.UnitType).ThenBy(x => x.Name)
                .Skip(skip).Take(take)
                .Select(o => new OrganizationUnitDto
                {
                    Id = o.Id,
                    Code = o.Code,
                    Name = o.Name,
                    UnitType = o.UnitType.ToString(),
                    BranchId = o.BranchId,
                    BranchName = o.Branch != null ? o.Branch.Name : null,
                    ParentId = o.ParentId,
                    ParentName = o.Parent != null ? o.Parent.Name : null,
                    WorkLocationId = o.WorkLocationId,
                    WorkLocationName = o.WorkLocation != null ? o.WorkLocation.Name : null,
                    AllocatedHeadcount = o.AllocatedHeadcount,
                    Description = o.Description,
                    IsActive = o.IsActive
                })
                .ToListAsync();

            var pageIds = data.Select(d => d.Id).ToList();
            var parentsWithChildren = await repository.GetAll()
                .Where(x => x.ParentId != null && pageIds.Contains(x.ParentId.Value))
                .Select(x => x.ParentId!.Value)
                .Distinct()
                .ToListAsync();
            foreach (var row in data)
                row.HasChildren = parentsWithChildren.Contains(row.Id);

            return new PaginatedResponse<OrganizationUnitDto> { Total = total, Data = data };
        }
    }

    /// <summary>
    /// The org units the CALLER may act for (self-service manager tools like Home hiring requests):
    /// HR admin → all active units; a manager → their own unit + sub-units (subtree); anyone else →
    /// none. Same DTO/paging shape as GetAll so a form dropdown can swap to it directly.
    /// </summary>
    public class GetMyOrganizationUnits(
        IRepository<OrganizationUnit> repository,
        Performance.IPerformanceVisibilityService visibility) : IGetMyOrganizationUnits
    {
        public async Task<PaginatedResponse<OrganizationUnitDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var scope = await visibility.GetScopeAsync();
            var query = repository.GetAll().Where(x => x.IsActive);
            // Non-admins are limited to their managed subtree (empty set for non-managers → no rows).
            if (!scope.IsAdmin)
                query = query.Where(x => scope.UnitIds.Contains(x.Id));

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || x.Code.Contains(term));
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderBy(x => x.UnitType).ThenBy(x => x.Name)
                .Skip(skip).Take(take)
                .Select(o => new OrganizationUnitDto
                {
                    Id = o.Id,
                    Code = o.Code,
                    Name = o.Name,
                    UnitType = o.UnitType.ToString(),
                    BranchId = o.BranchId,
                    BranchName = o.Branch != null ? o.Branch.Name : null,
                    ParentId = o.ParentId,
                    ParentName = o.Parent != null ? o.Parent.Name : null,
                    WorkLocationId = o.WorkLocationId,
                    WorkLocationName = o.WorkLocation != null ? o.WorkLocation.Name : null,
                    AllocatedHeadcount = o.AllocatedHeadcount,
                    Description = o.Description,
                    IsActive = o.IsActive
                })
                .ToListAsync();

            return new PaginatedResponse<OrganizationUnitDto> { Total = total, Data = data };
        }
    }

    public class GetOrganizationTree(IRepository<OrganizationUnit> repository) : IGetOrganizationTree
    {
        public async Task<List<OrgUnitTreeNodeDto>> GetAsync()
        {
            // One query, assembled into a tree in memory (org structures are small).
            //
            // SortOrder leads, because the hierarchy is now arranged by hand (drag-and-drop) and a
            // unit has to stay where somebody put it. UnitType and Name remain as tie-breakers: they
            // are what ordered the tree before SortOrder existed, and they still decide units that
            // have never been dragged (all sitting on the same seeded value) — so an un-arranged
            // level looks exactly as it always did instead of falling into insertion order.
            var all = await repository.GetAll()
                .OrderBy(x => x.SortOrder).ThenBy(x => x.UnitType).ThenBy(x => x.Name)
                .Select(o => new { o.Id, o.Code, o.Name, o.UnitType, o.AllocatedHeadcount, o.ParentId })
                .ToListAsync();

            var nodes = all.ToDictionary(
                x => x.Id,
                x => new OrgUnitTreeNodeDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    UnitType = x.UnitType.ToString(),
                    AllocatedHeadcount = x.AllocatedHeadcount
                });

            var roots = new List<OrgUnitTreeNodeDto>();
            foreach (var x in all)
            {
                var node = nodes[x.Id];
                if (x.ParentId.HasValue && nodes.TryGetValue(x.ParentId.Value, out var parent))
                    parent.Children.Add(node);
                else
                    roots.Add(node);
            }
            return roots;
        }
    }
}
