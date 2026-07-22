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

        /// <summary>Loads (or creates) the single company profile row for the current tenant.</summary>
        internal static async Task<CompanyProfile> GetOrCreateAsync(IRepository<CompanyProfile> repository)
        {
            var profile = await repository.GetAll().FirstOrDefaultAsync();
            if (profile is null)
            {
                profile = CompanyProfile.Create();
                await repository.AddAsync(profile);
            }
            return profile;
        }
    }

    /// <summary>Stores the tenant's company logo inline for use as the {{Logo}} merge token (HC022).</summary>
    public class UploadCompanyLogo(
        IRepository<CompanyProfile> repository,
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

            var profile = await LogoStorage.GetOrCreateAsync(repository);
            profile.SetLogo(bytes, contentType);
            repository.UpdateAsync(profile);
            await repository.SaveChangesAsync();
            logger.LogInformation("Stored company logo ({Bytes} bytes)", bytes.Length);
        }
    }

    /// <summary>Returns the company logo bytes, or throws NotFound when none is configured.</summary>
    public class GetCompanyLogo(IRepository<CompanyProfile> repository) : IGetCompanyLogo
    {
        public async Task<(byte[] Content, string ContentType)> GetAsync()
        {
            var profile = await repository.GetAll()
                .Select(p => new { p.LogoContent, p.LogoContentType })
                .FirstOrDefaultAsync();

            if (profile?.LogoContent is null || profile.LogoContent.Length == 0)
                throw new NotFoundException("CompanyLogo", "current");

            return (profile.LogoContent, profile.LogoContentType ?? "application/octet-stream");
        }
    }

    public class GetCompanyLogoInfo(IRepository<CompanyProfile> repository) : IGetCompanyLogoInfo
    {
        public async Task<CompanyLogoInfo> GetAsync()
        {
            var profile = await repository.GetAll()
                .Select(p => new { HasLogo = p.LogoContent != null, p.LogoContentType })
                .FirstOrDefaultAsync();

            return new CompanyLogoInfo
            {
                HasLogo = profile?.HasLogo ?? false,
                ContentType = profile?.LogoContentType,
            };
        }
    }

    public class DeleteCompanyLogo(
        IRepository<CompanyProfile> repository,
        ILogger<DeleteCompanyLogo> logger) : IDeleteCompanyLogo
    {
        public async Task DeleteAsync()
        {
            var profile = await repository.GetAll().FirstOrDefaultAsync();
            if (profile is null) return;

            profile.ClearLogo();
            repository.UpdateAsync(profile);
            await repository.SaveChangesAsync();
            logger.LogInformation("Cleared company logo");
        }
    }
}
