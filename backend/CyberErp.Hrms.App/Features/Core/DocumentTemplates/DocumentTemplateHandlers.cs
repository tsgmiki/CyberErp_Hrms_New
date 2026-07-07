using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.DocumentTemplates.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.DocumentTemplates
{
    public interface ICreateDocumentTemplate { Task<Guid> CreateAsync(CreateDocumentTemplateDto dto); }
    public interface IUpdateDocumentTemplate { Task UpdateAsync(UpdateDocumentTemplateDto dto); }
    public interface IDeleteDocumentTemplate { Task DeleteAsync(Guid id); }
    public interface IGetDocumentTemplateById { Task<DocumentTemplateDto> GetAsync(Guid id); }
    public interface IGetAllDocumentTemplates { Task<PaginatedResponse<DocumentTemplateDto>> GetAsync(GetAllRequest request); }

    internal static class DocumentTemplateShared
    {
        internal static readonly System.Linq.Expressions.Expression<Func<DocumentTemplate, DocumentTemplateDto>> Projection = t => new DocumentTemplateDto
        {
            Id = t.Id,
            Name = t.Name,
            DocumentType = t.DocumentType.ToString(),
            HeaderHtml = t.HeaderHtml,
            Body = t.Body,
            FooterHtml = t.FooterHtml,
            Description = t.Description,
            IsActive = t.IsActive
        };
    }

    // ---- Create ---------------------------------------------------------------

    public class CreateDocumentTemplate(
        IRepository<DocumentTemplate> repository,
        IValidator<CreateDocumentTemplateDto> validator,
        ILogger<CreateDocumentTemplate> logger) : ICreateDocumentTemplate
    {
        public async Task<Guid> CreateAsync(CreateDocumentTemplateDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name))
                throw new DuplicateException(nameof(DocumentTemplate), nameof(dto.Name), dto.Name);

            var entity = DocumentTemplate.Create(
                dto.Name, Enum.Parse<DocumentTemplateType>(dto.DocumentType),
                dto.Body, dto.HeaderHtml, dto.FooterHtml, dto.Description, dto.IsActive);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created DocumentTemplate {Id} ({Name})", entity.Id, entity.Name);
            return entity.Id;
        }
    }

    // ---- Update ---------------------------------------------------------------

    public class UpdateDocumentTemplate(
        IRepository<DocumentTemplate> repository,
        IValidator<UpdateDocumentTemplateDto> validator,
        ILogger<UpdateDocumentTemplate> logger) : IUpdateDocumentTemplate
    {
        public async Task UpdateAsync(UpdateDocumentTemplateDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(DocumentTemplate), dto.Id.ToString());

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(DocumentTemplate), nameof(dto.Name), dto.Name);

            entity.Update(dto.Name, Enum.Parse<DocumentTemplateType>(dto.DocumentType),
                dto.Body, dto.HeaderHtml, dto.FooterHtml, dto.Description, dto.IsActive);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Updated DocumentTemplate {Id}", entity.Id);
        }
    }

    // ---- Delete ---------------------------------------------------------------

    public class DeleteDocumentTemplate(
        IRepository<DocumentTemplate> repository,
        ILogger<DeleteDocumentTemplate> logger) : IDeleteDocumentTemplate
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(DocumentTemplate), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted DocumentTemplate {Id}", id);
        }
    }

    // ---- Get by id ------------------------------------------------------------

    public class GetDocumentTemplateById(IRepository<DocumentTemplate> repository) : IGetDocumentTemplateById
    {
        public async Task<DocumentTemplateDto> GetAsync(Guid id) =>
            await repository.GetAll()
                .Where(t => t.Id == id)
                .Select(DocumentTemplateShared.Projection)
                .FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(DocumentTemplate), id.ToString());
    }

    // ---- Get all (paged) ------------------------------------------------------

    public class GetAllDocumentTemplates(IRepository<DocumentTemplate> repository) : IGetAllDocumentTemplates
    {
        public async Task<PaginatedResponse<DocumentTemplateDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<DocumentTemplateType>(request.Status, out var type))
                query = query.Where(x => x.DocumentType == type);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) ||
                    (x.Description != null && x.Description.Contains(term)));
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderBy(x => x.Name)
                .Skip(skip).Take(take)
                .Select(DocumentTemplateShared.Projection)
                .ToListAsync();

            return new PaginatedResponse<DocumentTemplateDto> { Total = total, Data = data };
        }
    }
}
