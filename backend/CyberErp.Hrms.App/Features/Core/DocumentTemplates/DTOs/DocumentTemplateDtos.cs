using FluentValidation;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.App.Features.Core.DocumentTemplates.DTOs
{
    public class DocumentTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DocumentType { get; set; } = nameof(DocumentTemplateType.EmploymentLetter);
        public string? HeaderHtml { get; set; }
        public string Body { get; set; } = string.Empty;
        public string? FooterHtml { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateDocumentTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string DocumentType { get; set; } = nameof(DocumentTemplateType.EmploymentLetter);
        public string? HeaderHtml { get; set; }
        public string Body { get; set; } = string.Empty;
        public string? FooterHtml { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateDocumentTemplateDto : CreateDocumentTemplateDto
    {
        public Guid Id { get; set; }
    }

    /// <summary>A rendered document ready to preview / print (merged HTML for one employee).</summary>
    public class GeneratedDocumentDto
    {
        public string Title { get; set; } = string.Empty;
        public string Html { get; set; } = string.Empty;
    }

    public class CreateDocumentTemplateDtoValidator : AbstractValidator<CreateDocumentTemplateDto>
    {
        public CreateDocumentTemplateDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Body).NotEmpty().WithMessage("Template body cannot be empty.");
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.DocumentType).NotEmpty()
                .Must(v => Enum.TryParse<DocumentTemplateType>(v, out _))
                .WithMessage("DocumentType must be one of: EmploymentLetter, ExperienceLetter, IdCard, Other.");
        }
    }

    public class UpdateDocumentTemplateDtoValidator : AbstractValidator<UpdateDocumentTemplateDto>
    {
        public UpdateDocumentTemplateDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            Include(new CreateDocumentTemplateDtoValidator());
        }
    }
}
