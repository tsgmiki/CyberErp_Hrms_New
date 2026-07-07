import { Document, Image, Page, StyleSheet, Text, View, pdf } from "@react-pdf/renderer";

const styles = StyleSheet.create({
  page: { padding: 28, fontFamily: "Helvetica" },
  title: { fontSize: 14, fontWeight: "bold", marginBottom: 2, color: "#0b0b0b" },
  meta: { fontSize: 8, color: "#64748b", marginBottom: 12 },
  imageWrap: { flexGrow: 1, alignItems: "center", justifyContent: "center" },
});

function sanitizeFileName(title: string): string {
  return title.replace(/[<>:"/\\|?*]+/g, "_").trim() || "org-chart";
}

function downloadBlob(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = fileName;
  anchor.click();
  URL.revokeObjectURL(url);
}

/**
 * Renders a captured org-chart PNG into a landscape A4 PDF and downloads it.
 * Uses the same `pdf().toBlob()` approach as the list exporter.
 */
export async function exportOrgChartPdf(
  dataUrl: string,
  opts: { width: number; height: number; title?: string },
): Promise<void> {
  const title = opts.title ?? "Organization Chart";
  const generatedAt = new Date().toLocaleString();

  // A4 landscape usable area (points) after margins.
  const maxW = 786;
  const maxH = 500;
  const ratio = Math.min(maxW / opts.width, maxH / opts.height, 1);
  const imgW = Math.max(1, Math.round(opts.width * ratio));
  const imgH = Math.max(1, Math.round(opts.height * ratio));

  const blob = await pdf(
    <Document title={title}>
      <Page size="A4" orientation="landscape" style={styles.page}>
        <Text style={styles.title}>{title}</Text>
        <Text style={styles.meta}>Generated {generatedAt}</Text>
        <View style={styles.imageWrap}>
          <Image src={dataUrl} style={{ width: imgW, height: imgH }} />
        </View>
      </Page>
    </Document>,
  ).toBlob();

  downloadBlob(blob, `${sanitizeFileName(title)}.pdf`);
}
