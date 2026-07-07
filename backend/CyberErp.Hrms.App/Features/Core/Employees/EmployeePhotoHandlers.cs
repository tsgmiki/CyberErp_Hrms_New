using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    public interface IUploadEmployeePhoto
    {
        Task UploadAsync(Guid employeeId, Stream content, string fileName, long length);
    }

    public interface IGetEmployeePhoto
    {
        Task<(byte[] Content, string ContentType)> GetAsync(Guid employeeId);
    }

    internal static class PhotoStorage
    {
        internal const long MaxBytes = 2 * 1024 * 1024; // 2 MB

        internal static readonly Dictionary<string, string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp",
        };

        internal static string ResolveRoot(IConfiguration configuration) =>
            configuration["Storage:EmployeePhotoPath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "employee-photos");
    }

    /// <summary>Stores an employee photo on disk and records the file name on the employee (HC015/HC023).</summary>
    public class UploadEmployeePhoto(
        IRepository<Employee> repository,
        IConfiguration configuration,
        ILogger<UploadEmployeePhoto> logger) : IUploadEmployeePhoto
    {
        public async Task UploadAsync(Guid employeeId, Stream content, string fileName, long length)
        {
            // Tracked + branch-filtered fetch: a branch admin cannot attach photos across branches.
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == employeeId)
                ?? throw new NotFoundException(nameof(Employee), employeeId.ToString());

            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext) || !PhotoStorage.AllowedTypes.ContainsKey(ext))
                throw new ValidationException("photo", "Photo must be a JPG, PNG or WEBP image.");
            if (length <= 0 || length > PhotoStorage.MaxBytes)
                throw new ValidationException("photo", "Photo must be between 1 byte and 2 MB.");

            var root = PhotoStorage.ResolveRoot(configuration);
            Directory.CreateDirectory(root);

            // Remove a previous photo with a different extension so exactly one file remains.
            if (!string.IsNullOrEmpty(entity.PhotoUrl))
            {
                var old = Path.Combine(root, entity.PhotoUrl);
                if (File.Exists(old) && !string.Equals(Path.GetExtension(old), ext, StringComparison.OrdinalIgnoreCase))
                    File.Delete(old);
            }

            var storedName = $"{employeeId}{ext.ToLowerInvariant()}";
            var path = Path.Combine(root, storedName);
            await using (var file = File.Create(path))
            {
                await content.CopyToAsync(file);
            }

            entity.SetPhoto(storedName);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Stored photo for Employee {Id} ({File})", employeeId, storedName);
        }
    }

    /// <summary>Streams the stored employee photo (branch-filtered like every employee read).</summary>
    public class GetEmployeePhoto(
        IRepository<Employee> repository,
        IConfiguration configuration) : IGetEmployeePhoto
    {
        public async Task<(byte[] Content, string ContentType)> GetAsync(Guid employeeId)
        {
            var photo = await repository.GetAll()
                .Where(x => x.Id == employeeId)
                .Select(x => x.PhotoUrl)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(photo))
                throw new NotFoundException("EmployeePhoto", employeeId.ToString());

            var path = Path.Combine(PhotoStorage.ResolveRoot(configuration), photo);
            if (!File.Exists(path))
                throw new NotFoundException("EmployeePhoto", employeeId.ToString());

            var contentType = PhotoStorage.AllowedTypes.TryGetValue(Path.GetExtension(path), out var ct)
                ? ct
                : "application/octet-stream";
            return (await File.ReadAllBytesAsync(path), contentType);
        }
    }
}
