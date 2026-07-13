using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    /// <summary>
    /// Delivers an offer letter to the candidate as a generated PDF by e-mail.
    /// <para>
    /// <see cref="TryAutoSendAsync"/> is the post-approval hook: the moment the FINAL approver
    /// approves an offer (workflow outcome, or the direct auto-approve when no chain is defined),
    /// the letter is rendered, e-mailed, and — on successful delivery — the offer moves
    /// Approved → Sent and the application to OfferPending automatically. When delivery is not
    /// possible (candidate without an e-mail address, mail outage) the offer STAYS Approved and
    /// the manual "Send to Candidate" button remains as the retry; approval itself never fails
    /// because of mail.
    /// </para>
    /// </summary>
    public interface IOfferDelivery
    {
        /// <summary>E-mails the offer letter as a PDF attachment. Never throws; false = not delivered.</summary>
        Task<bool> EmailOfferAsync(JobOffer offer);

        /// <summary>
        /// Post-approval auto-delivery: render + e-mail the PDF and, on success, mark the offer
        /// Sent and move the application to OfferPending. Never throws.
        /// </summary>
        Task TryAutoSendAsync(Guid offerId);
    }

    public class OfferDelivery(
        IRepository<JobOffer> repository,
        IRepository<JobApplication> applicationRepository,
        IRepository<JobApplicationStageLog> stageLogRepository,
        IRepository<Candidate> candidateRepository,
        IRepository<JobRequisition> requisitionRepository,
        IGenerateOfferLetter letterGenerator,
        IOfferLetterComposer letterComposer,
        IPdfService pdfService,
        IEmailService emailService,
        ILogger<OfferDelivery> logger) : IOfferDelivery
    {
        public async Task<bool> EmailOfferAsync(JobOffer offer)
        {
            try
            {
                var context = await applicationRepository.GetAll()
                    .Where(a => a.Id == offer.ApplicationId)
                    .Select(a => new
                    {
                        Candidate = candidateRepository.GetAll()
                            .Where(c => c.Id == a.CandidateId)
                            .Select(c => new { c.Email, c.FirstName, c.FatherName })
                            .FirstOrDefault(),
                        Title = requisitionRepository.GetAll()
                            .Where(q => q.Id == a.RequisitionId).Select(q => q.Title).FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();
                if (context?.Candidate is null || string.IsNullOrWhiteSpace(context.Candidate.Email))
                {
                    logger.LogInformation(
                        "Offer {Number}: candidate has no e-mail address — letter not e-mailed", offer.OfferNumber);
                    return false;
                }

                // The PDF is composed from the customizable template over the company letterhead;
                // an HR-edited LetterText (if any) is used verbatim as the body.
                var composed = await letterComposer.ComposeAsync(offer.Id,
                    string.IsNullOrWhiteSpace(offer.LetterText) ? null : offer.LetterText);
                var pdf = pdfService.RenderOfferLetter(composed.Document);

                var candidateName = string.Join(" ",
                    new[] { context.Candidate.FirstName, context.Candidate.FatherName }
                        .Where(n => !string.IsNullOrWhiteSpace(n)));
                var body = $"""
                    Dear {candidateName},

                    We are pleased to share your formal offer of employment for the position of
                    {context.Title ?? "the advertised role"} (offer {offer.OfferNumber}) — please find
                    the offer letter attached as a PDF document.

                    The offer remains valid until {offer.ExpiryDate:dd MMMM yyyy}. Kindly confirm your
                    acceptance in writing before that date.

                    Kind regards,
                    Human Resources
                    """;

                return await emailService.SendAsync(
                    context.Candidate.Email,
                    $"Job Offer — {context.Title ?? offer.OfferNumber}",
                    body,
                    [new EmailAttachment($"{offer.OfferNumber}.pdf", pdf, "application/pdf")]);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Offer {Number}: e-mail delivery failed — the offer itself is unaffected", offer.OfferNumber);
                return false;
            }
        }

        public async Task TryAutoSendAsync(Guid offerId)
        {
            try
            {
                var offer = await repository.GetAll().FirstOrDefaultAsync(o => o.Id == offerId);
                if (offer is null || offer.Status != OfferStatus.Approved) return;

                // A letter drafted by HR is used verbatim; otherwise the standard HC111 letter is
                // generated and attached so the record shows exactly what the candidate received.
                if (string.IsNullOrWhiteSpace(offer.LetterText))
                {
                    offer.AttachLetter(await letterGenerator.GenerateAsync(offer.Id));
                    repository.UpdateAsync(offer);
                    await repository.SaveChangesAsync();
                }

                // EmailOfferAsync enqueues the send (background delivery with retries): true means
                // the letter is durably queued, so the offer proceeds to Sent; false (no candidate
                // e-mail / mailer disabled) leaves it Approved for manual handling.
                if (!await EmailOfferAsync(offer))
                {
                    logger.LogInformation(
                        "Offer {Number}: approved but not auto-delivered — stays Approved for manual sending",
                        offer.OfferNumber);
                    return;
                }

                offer.MarkSent();
                repository.UpdateAsync(offer);
                await OfferShared.MoveToOfferPendingAsync(applicationRepository, stageLogRepository,
                    offer.ApplicationId, $"Offer {offer.OfferNumber} approved and e-mailed to the candidate", null);
                await repository.SaveChangesAsync();
                logger.LogInformation("Offer {Number}: auto-delivery queued on approval (PDF e-mail) and marked Sent",
                    offer.OfferNumber);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Offer {OfferId}: auto-delivery after approval failed — the approval stands", offerId);
            }
        }
    }
}
