using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Training;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>
    /// Generates the human-readable training record an inspection asks for.
    ///
    /// <para>⚠️ GENERATED FROM THE DATA, never transcribed. "Accurate and complete copies" is the
    /// requirement, and the only way to be sure of it is for the document and the screen to be
    /// rendering the same query. Nothing here is typed in or cached (logic §12.90).</para>
    ///
    /// <para>⚠️ Every signature is printed with its INTEGRITY STATE. A record whose facts have
    /// changed since signing prints as such rather than silently showing a signature that no longer
    /// covers what is above it — an inspector reading a clean-looking document is exactly who that
    /// would mislead.</para>
    ///
    /// <para>This is a generated copy, not a validated archival rendering. If QA needs byte-identical
    /// reproducible output the renderer itself has to enter validation; that is a follow-up, and it
    /// is called out in the document footer rather than left implied.</para>
    /// </summary>
    public class TrainingRecordDocument(
        IRepository<Employee> employees,
        IRepository<TrainingEnrollment> enrollments,
        IGetSignableRecord recordHandler) : IGetTrainingRecordDocument
    {
        /// <summary>
        /// ⚠️ Repeated from <see cref="QuestPdfService"/> on purpose. QuestPDF refuses to render
        /// until its licence is declared, and that declaration lives in a STATIC CONSTRUCTOR — which
        /// runs only when its own type is first touched. A second renderer that never touches the
        /// first one therefore starts unlicensed and throws on the first document it draws.
        /// </summary>
        static TrainingRecordDocument() => QuestPDF.Settings.License = LicenseType.Community;

        public async Task<(byte[] Content, string FileName)> GetAsync(Guid employeeId)
        {
            var person = await employees.GetAll().AsNoTracking()
                    .Where(e => e.Id == employeeId)
                    .Select(e => new
                    {
                        e.EmployeeNumber,
                        Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber
                    })
                    .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), employeeId.ToString());

            var ids = await enrollments.GetAll().AsNoTracking()
                .Where(e => e.EmployeeId == employeeId && e.Status == TrainingEnrollmentStatus.Completed)
                .OrderByDescending(e => e.CompletedOn)
                .Select(e => e.Id)
                .ToListAsync();

            var records = new List<SignableRecordDto>();
            foreach (var id in ids) records.Add(await recordHandler.GetAsync(id));

            var generatedOn = DateTime.UtcNow;
            var bytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.6f, Unit.Centimetre);
                    page.DefaultTextStyle(t => t.FontSize(9).FontColor("#1A1A1A"));

                    page.Header().Column(header =>
                    {
                        header.Item().Text("Training Record").FontSize(17).SemiBold();
                        header.Item().PaddingTop(2).Text($"{person.Name} · {person.EmployeeNumber}")
                            .FontSize(10).FontColor("#555555");
                        header.Item().PaddingTop(6).LineHorizontal(1).LineColor("#999999");
                    });

                    page.Content().PaddingVertical(10).Column(body =>
                    {
                        if (records.Count == 0)
                        {
                            body.Item().PaddingTop(14)
                                .Text("No completed training is recorded for this employee.")
                                .FontColor("#555555");
                            return;
                        }

                        foreach (var r in records)
                        {
                            body.Item().PaddingBottom(12).Column(entry =>
                            {
                                entry.Item().Text(r.CourseName).FontSize(11).SemiBold();

                                var line = new List<string>();
                                if (!string.IsNullOrWhiteSpace(r.CourseCode)) line.Add(r.CourseCode!);
                                if (r.CourseVersionNumber.HasValue) line.Add($"version {r.CourseVersionNumber}");
                                if (r.CompletedOn.HasValue) line.Add($"completed {r.CompletedOn:dd MMM yyyy}");
                                if (r.AttendancePercent.HasValue) line.Add($"attendance {r.AttendancePercent:0.#}%");
                                // A null score prints as "not assessed" rather than 0 — the distinction
                                // the whole assessment phase exists to preserve.
                                line.Add(r.AssessmentScore.HasValue
                                    ? $"score {r.AssessmentScore:0.#}%"
                                    : "not assessed");
                                if (!string.IsNullOrWhiteSpace(r.AssignmentName)) line.Add($"required by: {r.AssignmentName}");

                                entry.Item().PaddingTop(1).Text(string.Join(" · ", line))
                                    .FontSize(8.5f).FontColor("#555555");

                                if (r.Signatures.Count == 0)
                                {
                                    entry.Item().PaddingTop(3).PaddingLeft(8)
                                        .Text("Unsigned.").FontSize(8.5f).Italic().FontColor("#8A6A12");
                                    return;
                                }

                                foreach (var s in r.Signatures)
                                {
                                    entry.Item().PaddingTop(3).PaddingLeft(8).Column(sig =>
                                    {
                                        sig.Item().Text(text =>
                                        {
                                            text.Span($"{s.Meaning}: ").SemiBold().FontSize(8.5f);
                                            text.Span($"{s.SignedByName} — {s.SignedOn:dd MMM yyyy HH:mm} UTC")
                                                .FontSize(8.5f);
                                        });
                                        sig.Item().Text($"“{s.SignedStatement}”")
                                            .FontSize(8).Italic().FontColor("#555555");
                                        if (!string.IsNullOrWhiteSpace(s.Note))
                                            sig.Item().Text($"Note: {s.Note}").FontSize(8).FontColor("#555555");
                                        if (!s.Intact)
                                            sig.Item().Text("⚠ This record has changed since it was signed.")
                                                .FontSize(8).SemiBold().FontColor("#A33A2E");
                                    });
                                }
                            });
                        }
                    });

                    page.Footer().Column(footer =>
                    {
                        footer.Item().LineHorizontal(0.5f).LineColor("#999999");
                        footer.Item().PaddingTop(3).Text(text =>
                        {
                            text.DefaultTextStyle(t => t.FontSize(7.5f).FontColor("#555555"));
                            text.Span($"Generated from the HRMS training records on {generatedOn:dd MMM yyyy HH:mm} UTC. ");
                            text.Span("Generated copy — the rendering itself is not validated. ");
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                    });
                });
            }).GeneratePdf();

            var safeNumber = string.Concat((person.EmployeeNumber ?? "record")
                .Select(c => char.IsLetterOrDigit(c) ? c : '-'));
            return (bytes, $"training-record-{safeNumber}.pdf");
        }
    }
}
