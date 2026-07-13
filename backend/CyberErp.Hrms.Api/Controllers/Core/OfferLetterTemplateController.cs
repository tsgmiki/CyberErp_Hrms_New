using CyberErp.Hrms.App.Features.Core.Recruitment;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>
    /// HR configuration for the offer-letter PDF (HC111): the company letterhead identity
    /// (name / contact — logo is shared with document templates at <c>DocumentTemplate/logo</c>)
    /// and the customizable, tokenized letter body rendered into the offer PDF that is e-mailed to
    /// the candidate on approval.
    /// </summary>
    public class OfferLetterTemplateController(
        IGetOfferLetterTemplate getTemplate,
        ISaveOfferLetterTemplate saveTemplate,
        IGetCompanyProfile getCompany,
        ISaveCompanyProfile saveCompany,
        IGetOfferMergeFields mergeFields,
        IPreviewOfferLetter preview) : BaseController
    {
        /// <summary>The current offer-letter template (falls back to the built-in default).</summary>
        [HttpGet]
        public Task<OfferLetterTemplateDto> Get() => getTemplate.GetAsync();

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveOfferLetterTemplateDto dto)
        {
            await saveTemplate.SaveAsync(dto);
            return Ok(new { message = "Offer-letter template saved" });
        }

        /// <summary>Company letterhead identity (name / contact address / phone / e-mail).</summary>
        [HttpGet("company")]
        public Task<CompanyProfileDto> GetCompany() => getCompany.GetAsync();

        [HttpPut("company")]
        public async Task<IActionResult> UpdateCompany([FromBody] SaveCompanyProfileDto dto)
        {
            await saveCompany.SaveAsync(dto);
            return Ok(new { message = "Company profile saved" });
        }

        /// <summary>Merge tokens available to the template editor palette.</summary>
        [HttpGet("merge-fields")]
        public ActionResult<List<OfferMergeFieldDto>> GetMergeFields() => mergeFields.Get();

        /// <summary>Renders the (unsaved) template with sample data to a PDF for a live preview.</summary>
        [HttpPost("preview")]
        public async Task<IActionResult> Preview([FromBody] SaveOfferLetterTemplateDto dto)
        {
            var pdf = await preview.PreviewAsync(dto);
            return File(pdf, "application/pdf", "offer-letter-preview.pdf");
        }
    }
}
