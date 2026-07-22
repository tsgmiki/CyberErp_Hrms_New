using CyberErp.Hrms.App.Common.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>
    /// QuestPDF implementation of <see cref="IPdfService"/> — renders an offer letter (HC111) as an
    /// A4 corporate letter over the company letterhead: logo + company identity in the header, the
    /// merged body, a signatory block, and a page-number footer.
    /// </summary>
    public class QuestPdfService : IPdfService
    {
        static QuestPdfService() => QuestPDF.Settings.License = LicenseType.Community;

        public byte[] RenderOfferLetter(OfferLetterDocument d)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Calibri));

                    page.Header().Element(h => Header(h, d));
                    page.Content().Element(c => Content(c, d));

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text(d.CompanyName ?? "CyberErp HRMS — Recruitment")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                        row.ConstantItem(80).AlignRight().Text(t =>
                        {
                            t.DefaultTextStyle(s => s.FontSize(8).FontColor(Colors.Grey.Darken1));
                            t.CurrentPageNumber();
                            t.Span(" / ");
                            t.TotalPages();
                        });
                    });
                });
            }).GeneratePdf();
        }

        private static void Header(IContainer container, OfferLetterDocument d)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    if (d.Logo is { Length: > 0 })
                        row.ConstantItem(110).AlignLeft().AlignMiddle().MaxHeight(64).Image(d.Logo).FitHeight();
                    else
                        row.ConstantItem(1);

                    row.RelativeItem().AlignRight().Column(id =>
                    {
                        if (!string.IsNullOrWhiteSpace(d.CompanyName))
                            id.Item().Text(d.CompanyName).SemiBold().FontSize(15).FontColor(Colors.Black);
                        foreach (var line in ContactLines(d))
                            id.Item().Text(line).FontSize(9).FontColor(Colors.Grey.Darken2);
                    });
                });
                col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
            });
        }

        private static void Content(IContainer container, OfferLetterDocument d)
        {
            container.PaddingVertical(14).Column(col =>
            {
                col.Item().AlignRight().Text(d.DateText).FontSize(10).FontColor(Colors.Grey.Darken1);
                if (!string.IsNullOrWhiteSpace(d.Title))
                    col.Item().PaddingTop(6).Text(d.Title).SemiBold().FontSize(13);

                col.Item().PaddingTop(10).Column(body =>
                {
                    foreach (var paragraph in d.Body.Replace("\r\n", "\n").Split('\n'))
                    {
                        if (string.IsNullOrWhiteSpace(paragraph))
                            body.Item().Height(8);
                        else
                            body.Item().Text(paragraph).LineHeight(1.35f);
                    }
                });

                if (!string.IsNullOrWhiteSpace(d.SignatoryName) || !string.IsNullOrWhiteSpace(d.SignatoryTitle))
                {
                    col.Item().PaddingTop(28).Column(sig =>
                    {
                        sig.Item().Text("Sincerely,");
                        sig.Item().PaddingTop(18).Text(d.SignatoryName ?? string.Empty).SemiBold();
                        if (!string.IsNullOrWhiteSpace(d.SignatoryTitle))
                            sig.Item().Text(d.SignatoryTitle).FontSize(10).FontColor(Colors.Grey.Darken1);
                    });
                }
            });
        }

        private static IEnumerable<string> ContactLines(OfferLetterDocument d)
        {
            if (!string.IsNullOrWhiteSpace(d.ContactAddress))
                foreach (var line in d.ContactAddress.Replace("\r\n", "\n").Split('\n'))
                    if (!string.IsNullOrWhiteSpace(line)) yield return line.Trim();

            var contact = new[] { d.ContactPhone, d.ContactEmail }
                .Where(v => !string.IsNullOrWhiteSpace(v));
            if (contact.Any()) yield return string.Join("  ·  ", contact);
        }
    }
}
