using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.DocumentTemplates
{
    public interface IUploadCompanyLogo { Task UploadAsync(Stream content, string fileName, long length); }
    public interface IGetCompanyLogo { Task<(byte[] Content, string ContentType)> GetAsync(); }
    public interface IDeleteCompanyLogo { Task DeleteAsync(); }
    public interface IGetCompanyLogoInfo { Task<CompanyLogoInfo> GetAsync(); }

    public class CompanyLogoInfo
    {
        public bool HasLogo { get; set; }
        public string? ContentType { get; set; }
    }

    internal static class LogoStorage
    {
        internal const long MaxBytes = 2 * 1024 * 1024; // 2 MB

        internal static readonly Dictionary<string, string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp",
            [".gif"] = "image/gif",
        };

        /// <summary>
        /// Loads (or creates) the single organization row.
        ///
        /// <para>This was <c>Hrms.CompanyProfile</c> until 2026-08-13. Core.Organization is the richer
        /// successor and now owns the letterhead outright — see logic.md §12.11.</para>
        ///
        /// <para>Organization requires a Code and a LegalName, neither of which the profile had, so a
        /// row created here carries deliberate placeholders for an administrator to correct on the
        /// company-profile screen. Refusing to store a logo for want of a legal name would be worse.</para>
        /// </summary>
        internal static async Task<Organization> GetOrCreateAsync(IRepository<Organization> repository)
        {
            var organization = await repository.GetAll().FirstOrDefaultAsync();
            if (organization is null)
            {
                organization = Organization.Create("DEFAULT", "Organization");
                await repository.AddAsync(organization);
            }
            return organization;
        }
    }

    /// <summary>Stores the tenant's company logo inline for use as the {{Logo}} merge token (HC022).</summary>
    public class UploadCompanyLogo(
        IRepository<Organization> repository,
        ILogger<UploadCompanyLogo> logger) : IUploadCompanyLogo
    {
        public async Task UploadAsync(Stream content, string fileName, long length)
        {
            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext) || !LogoStorage.AllowedTypes.TryGetValue(ext, out var contentType))
                throw new ValidationException("logo", "Logo must be a JPG, PNG, WEBP or GIF image.");
            if (length <= 0 || length > LogoStorage.MaxBytes)
                throw new ValidationException("logo", "Logo must be between 1 byte and 2 MB.");

            using var ms = new MemoryStream();
            await content.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var organization = await LogoStorage.GetOrCreateAsync(repository);
            organization.SetLogo(bytes, contentType);
            repository.UpdateAsync(organization);
            await repository.SaveChangesAsync();
            logger.LogInformation("Stored company logo ({Bytes} bytes)", bytes.Length);
        }
    }

    /// <summary>Returns the company logo bytes, or throws NotFound when none is configured.</summary>
    public class GetCompanyLogo(IRepository<Organization> repository) : IGetCompanyLogo
    {
        public async Task<(byte[] Content, string ContentType)> GetAsync()
        {
            var organization = await repository.GetAll()
                .Select(o => new { o.Logo, o.LogoContentType })
                .FirstOrDefaultAsync();

            if (organization?.Logo is null || organization.Logo.Length == 0)
                throw new NotFoundException("CompanyLogo", "current");

            return (organization.Logo, organization.LogoContentType ?? "application/octet-stream");
        }
    }

    public class GetCompanyLogoInfo(IRepository<Organization> repository) : IGetCompanyLogoInfo
    {
        public async Task<CompanyLogoInfo> GetAsync()
        {
            var organization = await repository.GetAll()
                .Select(o => new { HasLogo = o.Logo != null, o.LogoContentType })
                .FirstOrDefaultAsync();

            return new CompanyLogoInfo
            {
                HasLogo = organization?.HasLogo ?? false,
                ContentType = organization?.LogoContentType,
            };
        }
    }

    public class DeleteCompanyLogo(
        IRepository<Organization> repository,
        ILogger<DeleteCompanyLogo> logger) : IDeleteCompanyLogo
    {
        public async Task DeleteAsync()
        {
            var organization = await repository.GetAll().FirstOrDefaultAsync();
            if (organization is null) return;

            organization.SetLogo(null, null);
            repository.UpdateAsync(organization);
            await repository.SaveChangesAsync();
            logger.LogInformation("Cleared company logo");
        }
    }
}
