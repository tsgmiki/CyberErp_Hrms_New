using System.Text.RegularExpressions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Notifications
{
    /// <summary>
    /// Turns an event into the messages an ADMINISTRATOR configured for it: picks the template,
    /// merges the tokens, resolves the recipient rules to real addresses, and sends.
    ///
    /// <para>Never throws. A missing template is not an error — it means the client has not asked to
    /// be told about that event.</para>
    /// </summary>
    public partial class NotificationDispatcher(
        IRepository<NotificationTemplate> templates,
        IRepository<NotificationRecipient> recipientRules,
        IRepository<Employee> employees,
        IRepository<User> users,
        IRepository<TenantUser> tenantUsers,
        IRepository<TenantUserRole> tenantUserRoles,
        IRepository<Position> positions,
        IWorkflowApproverAuth approverAuth,
        IOrgManagerResolver managerResolver,
        IEmailService email,
        ILogger<NotificationDispatcher> logger) : INotificationDispatcher
    {
        /// <summary>Matches {{Token}} with optional inner spacing, case-insensitively.</summary>
        [GeneratedRegex(@"\{\{\s*(\w+)\s*\}\}", RegexOptions.IgnoreCase)]
        private static partial Regex TokenPattern();

        public async Task<int> DispatchAsync(NotificationContext context)
        {
            try
            {
                var matches = await SelectTemplatesAsync(context);
                if (matches.Count == 0) return 0;

                var sent = 0;
                foreach (var template in matches)
                {
                    var subject = Merge(template.Subject, context.Tokens);
                    var body = Merge(template.Body, context.Tokens);

                    var addresses = await ResolveRecipientsAsync(template.Id, context);
                    if (addresses.Count == 0)
                    {
                        // Configured but unreachable: the rules resolved to nobody with an address.
                        // Worth a log line — "nobody was told" is otherwise indistinguishable from
                        // "nothing was configured".
                        logger.LogInformation(
                            "Notification {EventKey}: template {TemplateId} matched but no recipient rule resolved to an address.",
                            context.EventKey, template.Id);
                        continue;
                    }

                    // The symmetric half of the line above: with the count logged on BOTH paths,
                    // "who was told" is answerable from the log even without a working relay.
                    logger.LogInformation(
                        "Notification {EventKey}: template {TemplateId} resolved to {Recipients} recipient(s).",
                        context.EventKey, template.Id, addresses.Count);

                    // ⚠️ Sent INDIVIDUALLY, one message per address. IEmailService takes a single
                    // recipient, so To/Cc/Bcc are recorded on the rule but every address currently
                    // receives its own copy — which is also the privacy-safe behaviour for the
                    // AllEmployees rule, where a real Cc would publish the whole staff address list.
                    foreach (var address in addresses)
                    {
                        if (await email.SendAsync(address, subject, body)) sent++;
                    }
                }

                return sent;
            }
            catch (Exception ex)
            {
                // A notification must never break the operation that raised it.
                logger.LogError(ex, "Notification {EventKey} could not be dispatched.", context.EventKey);
                return 0;
            }
        }

        /// <summary>
        /// The active templates for this event, narrowed to the MOST SPECIFIC scope that matches.
        ///
        /// <para>A client can write one "leave approved" template for every workflow and a different
        /// one for the HR step. Both match the HR step; sending both would double-mail. So the
        /// buckets are tried in order — step, then workflow, then general — and the first non-empty
        /// one wins.</para>
        /// </summary>
        private async Task<List<NotificationTemplate>> SelectTemplatesAsync(NotificationContext context)
        {
            var all = await templates.GetAll().AsNoTracking()
                .Where(t => t.IsActive && t.EventKey == context.EventKey)
                .ToListAsync();
            if (all.Count == 0) return all;

            var stepScoped = all.Where(t =>
                t.WorkflowDefinitionId != null && t.WorkflowDefinitionId == context.WorkflowDefinitionId &&
                t.StepOrder != null && t.StepOrder == context.StepOrder).ToList();
            if (stepScoped.Count > 0) return stepScoped;

            var workflowScoped = all.Where(t =>
                t.WorkflowDefinitionId != null && t.WorkflowDefinitionId == context.WorkflowDefinitionId &&
                t.StepOrder == null).ToList();
            if (workflowScoped.Count > 0) return workflowScoped;

            return all.Where(t => t.WorkflowDefinitionId == null).ToList();
        }

        /// <summary>
        /// Replaces {{Token}} with the event's value. An UNKNOWN token merges to empty rather than
        /// being left in place: a thin sentence reads as an oversight, whereas a literal
        /// "{{EmployeeName}}" in a message to staff reads as a broken system.
        /// </summary>
        private static string Merge(string text, IReadOnlyDictionary<string, string?> tokens)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return TokenPattern().Replace(text, m =>
            {
                var key = m.Groups[1].Value;
                var hit = tokens.FirstOrDefault(t =>
                    string.Equals(t.Key, key, StringComparison.OrdinalIgnoreCase));
                return hit.Value ?? string.Empty;
            });
        }

        /// <summary>Resolves every active rule on the template to a de-duplicated address set.</summary>
        private async Task<List<string>> ResolveRecipientsAsync(Guid templateId, NotificationContext context)
        {
            var rules = await recipientRules.GetAll().AsNoTracking()
                .Where(r => r.NotificationTemplateId == templateId && r.IsActive)
                .ToListAsync();

            var addresses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var rule in rules)
            {
                switch (rule.Kind)
                {
                    case RecipientKind.Address:
                        if (!string.IsNullOrWhiteSpace(rule.Address)) addresses.Add(rule.Address.Trim());
                        break;

                    case RecipientKind.Requester:
                        if (context.RequesterEmployeeId is Guid requester)
                            AddAll(addresses, await EmployeeAddressesAsync([requester]));
                        break;

                    case RecipientKind.Employee:
                        if (rule.TargetId is Guid employeeId)
                            AddAll(addresses, await EmployeeAddressesAsync([employeeId]));
                        break;

                    case RecipientKind.RequesterManager:
                        if (context.RequesterEmployeeId is Guid managed)
                        {
                            // Reuses the org resolver the workflow engine already trusts for
                            // "immediate manager" — one definition of the reporting line, not two.
                            var manager = await managerResolver.ResolveImmediateManagerAsync(managed);
                            // EmployeeIds is a LIST: a unit can have several managerial staff, and
                            // the resolver returns all of them rather than picking one arbitrarily.
                            if (manager is not null && manager.EmployeeIds.Count > 0)
                                AddAll(addresses, await EmployeeAddressesAsync(manager.EmployeeIds));
                        }
                        break;

                    case RecipientKind.CurrentApprover:
                        if (context.WorkflowDefinitionId is Guid definitionId && context.StepOrder is int step)
                        {
                            // Same resolution the engine uses to decide WHO MAY APPROVE, so the
                            // notification cannot disagree with the inbox.
                            var userIds = await approverAuth.ResolveApproverUserIdsAsync(
                                definitionId, step, context.RequesterEmployeeId);
                            AddAll(addresses, await UserAddressesAsync(userIds));
                        }
                        break;

                    case RecipientKind.Role:
                        if (rule.TargetId is Guid roleId)
                            AddAll(addresses, await RoleAddressesAsync(roleId));
                        break;

                    case RecipientKind.OrganizationUnit:
                        if (rule.TargetId is Guid unitId)
                            AddAll(addresses, await UnitAddressesAsync(unitId));
                        break;

                    case RecipientKind.AllEmployees:
                        var everyone = await AllEmployeeAddressesAsync();
                        // Loud on purpose: this is the rule a client regrets, and the log is what
                        // explains a 500-message morning.
                        logger.LogWarning(
                            "Notification {EventKey}: an AllEmployees rule resolved to {Count} addresses.",
                            context.EventKey, everyone.Count);
                        AddAll(addresses, everyone);
                        break;
                }
            }

            return [.. addresses];
        }

        private static void AddAll(HashSet<string> into, IEnumerable<string> found)
        {
            foreach (var a in found) into.Add(a);
        }

        private async Task<List<string>> EmployeeAddressesAsync(IReadOnlyList<Guid> employeeIds) =>
            await employees.GetAllWithoutTenantFilter().AsNoTracking()
                .Where(e => employeeIds.Contains(e.Id) && e.Email != null && e.Email != "")
                .Select(e => e.Email!)
                .ToListAsync();

        /// <summary>
        /// Addresses for a set of LOGINS, falling back to the linked employee record.
        ///
        /// <para>⚠️ The fallback is not a nicety. A login's Email is optional and, in practice,
        /// frequently blank — HR maintains the address on the EMPLOYEE. Without the fallback a
        /// CurrentApprover rule resolves the right person and then silently reaches nobody, which
        /// is indistinguishable from a misconfigured template.</para>
        /// </summary>
        private async Task<List<string>> UserAddressesAsync(IReadOnlyCollection<Guid> userIds)
        {
            if (userIds.Count == 0) return [];

            var logins = await users.GetAllWithoutTenantFilter().AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Email, u.EmployeeId })
                .ToListAsync();

            var found = logins
                .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                .Select(u => u.Email!.Trim())
                .ToList();

            // Only the logins that came up empty fall through to their employee record.
            var unresolved = logins
                .Where(u => string.IsNullOrWhiteSpace(u.Email) && u.EmployeeId is not null)
                .Select(u => u.EmployeeId!.Value)
                .Distinct()
                .ToList();

            if (unresolved.Count > 0) found.AddRange(await EmployeeAddressesAsync(unresolved));

            return found;
        }

        /// <summary>Everyone holding the role in this tenant, through the tenant-scoped membership chain.</summary>
        private async Task<List<string>> RoleAddressesAsync(Guid tenantRoleId)
        {
            var userIds = await tenantUsers.GetAll().AsNoTracking()
                .Join(tenantUserRoles.GetAll().Where(r => r.TenantRoleId == tenantRoleId),
                    tu => tu.Id, tur => tur.TenantUserId, (tu, tur) => tu.UserId)
                .Distinct()
                .ToListAsync();
            return await UserAddressesAsync(userIds);
        }

        /// <summary>Active employees whose POSITION sits in the unit — the employee has no direct unit link.</summary>
        private async Task<List<string>> UnitAddressesAsync(Guid organizationUnitId)
        {
            var positionIds = await positions.GetAll().AsNoTracking()
                .Where(p => p.OrganizationUnitId == organizationUnitId)
                .Select(p => p.Id)
                .ToListAsync();
            if (positionIds.Count == 0) return [];

            return await employees.GetAll().AsNoTracking()
                .Where(e => e.PositionId != null && positionIds.Contains(e.PositionId.Value)
                            && e.EmploymentStatus == EmploymentStatus.Active
                            && e.Email != null && e.Email != "")
                .Select(e => e.Email!)
                .ToListAsync();
        }

        private async Task<List<string>> AllEmployeeAddressesAsync() =>
            await employees.GetAll().AsNoTracking()
                .Where(e => e.EmploymentStatus == EmploymentStatus.Active
                            && e.Email != null && e.Email != "")
                .Select(e => e.Email!)
                .ToListAsync();
    }
}
