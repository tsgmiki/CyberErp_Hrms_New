import { Document, Image, Page, StyleSheet, Text, View, pdf } from "@react-pdf/renderer";
import { hueFor } from "./palette";

/**
 * Org-chart PDF export. Takes the SAME SVG the screen renders (a full-expanded, light-theme
 * offscreen copy, serialized by the component), rasterizes it to a crisp PNG, and tiles it across
 * numbered A4-landscape pages when it can't fit one page at a readable scale — each page carrying a
 * header (title + legend) and footer. Because the source is the on-screen SVG, print === screen.
 */

// A4 landscape usable area (points) inside margins, minus header/footer bands.
const PAGE_W = 842;
const PAGE_H = 595;
const MARGIN = 28;
const HEADER_H = 44;
const FOOTER_H = 18;
const USABLE_W = PAGE_W - MARGIN * 2; // 786
const USABLE_H = PAGE_H - MARGIN * 2 - HEADER_H - FOOTER_H; // ~477

const CANVAS_BG = "#f6f6f3"; // matches the on-screen light pane
const MAX_CANVAS_PX = 15000; // browser per-dimension canvas cap safety

const styles = StyleSheet.create({
  page: { paddingTop: MARGIN, paddingBottom: MARGIN, paddingHorizontal: MARGIN, fontFamily: "Helvetica" },
  headerRow: { flexDirection: "row", justifyContent: "space-between", alignItems: "flex-start" },
  title: { fontSize: 14, fontWeight: "bold", color: "#0b0b0b" },
  meta: { fontSize: 8, color: "#64748b", marginTop: 2 },
  legendRow: { flexDirection: "row", flexWrap: "wrap", gap: 8, marginTop: 3 },
  legendItem: { flexDirection: "row", alignItems: "center", marginLeft: 10 },
  legendSwatch: { width: 7, height: 7, borderRadius: 1.5, marginRight: 3 },
  legendText: { fontSize: 7.5, color: "#334155" },
  imageWrap: { height: USABLE_H, marginTop: 8, alignItems: "center", justifyContent: "center" },
  footer: {
    position: "absolute",
    bottom: 12,
    left: MARGIN,
    right: MARGIN,
    flexDirection: "row",
    justifyContent: "space-between",
  },
  footerText: { fontSize: 7.5, color: "#94a3b8" },
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

/** Rasterizes an SVG string (Unicode-safe) to a PNG data URL at up to 2× for crisp text. */
async function rasterize(
  svgString: string,
  w: number,
  h: number,
): Promise<{ dataUrl: string; width: number; height: number }> {
  const scale = Math.min(2, MAX_CANVAS_PX / w, MAX_CANVAS_PX / h);
  const pxW = Math.round(w * scale);
  const pxH = Math.round(h * scale);

  const svg64 = window.btoa(unescape(encodeURIComponent(svgString)));
  const img = new window.Image();
  await new Promise<void>((resolve, reject) => {
    img.onload = () => resolve();
    img.onerror = () => reject(new Error("Could not rasterize the chart SVG."));
    img.src = `data:image/svg+xml;base64,${svg64}`;
  });

  const canvas = document.createElement("canvas");
  canvas.width = pxW;
  canvas.height = pxH;
  const ctx = canvas.getContext("2d");
  if (!ctx) throw new Error("Canvas 2D context unavailable.");
  ctx.fillStyle = CANVAS_BG;
  ctx.fillRect(0, 0, pxW, pxH);
  ctx.drawImage(img, 0, 0, pxW, pxH);
  return { dataUrl: canvas.toDataURL("image/png"), width: pxW, height: pxH };
}

/** Slices the full render into per-page tiles when one page can't hold it at a readable scale. */
async function paginate(dataUrl: string, pxW: number, pxH: number): Promise<{ src: string; w: number; h: number }[]> {
  const fit = Math.min(USABLE_W / pxW, USABLE_H / pxH);
  if (fit >= 1 || (USABLE_W / pxW >= 0.5 && USABLE_H / pxH >= 0.5)) {
    const s = Math.min(fit, 1);
    return [{ src: dataUrl, w: pxW * s, h: pxH * s }];
  }

  const img = new window.Image();
  await new Promise<void>((resolve, reject) => {
    img.onload = () => resolve();
    img.onerror = () => reject(new Error("Could not load chart image for pagination."));
    img.src = dataUrl;
  });

  const wide = pxW / pxH >= USABLE_W / USABLE_H;
  const scale = wide ? USABLE_H / pxH : USABLE_W / pxW; // fit the constrained axis, tile the other
  const tileSrc = wide ? USABLE_W / scale : USABLE_H / scale;
  const total = wide ? pxW : pxH;
  const count = Math.ceil(total / tileSrc);

  const pages: { src: string; w: number; h: number }[] = [];
  for (let i = 0; i < count; i++) {
    const offset = i * tileSrc;
    const len = Math.min(tileSrc, total - offset);
    const canvas = document.createElement("canvas");
    canvas.width = wide ? Math.round(len) : pxW;
    canvas.height = wide ? pxH : Math.round(len);
    const ctx = canvas.getContext("2d");
    if (!ctx) throw new Error("Canvas 2D context unavailable.");
    ctx.fillStyle = CANVAS_BG;
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    if (wide) ctx.drawImage(img, offset, 0, len, pxH, 0, 0, len, pxH);
    else ctx.drawImage(img, 0, offset, pxW, len, 0, 0, pxW, len);
    pages.push({ src: canvas.toDataURL("image/png"), w: canvas.width * scale, h: canvas.height * scale });
  }
  return pages;
}

/** Rasterizes the given org-chart SVG and downloads it as a professional multi-page landscape PDF. */
export async function exportOrgChartPdf(
  svgString: string,
  layoutW: number,
  layoutH: number,
  presentTypes: string[],
  title = "Organization Chart",
): Promise<void> {
  const { dataUrl, width, height } = await rasterize(svgString, layoutW, layoutH);
  const pages = await paginate(dataUrl, width, height);
  const generatedAt = new Date().toLocaleString();

  const blob = await pdf(
    <Document title={title}>
      {pages.map((pg, i) => (
        <Page key={i} size="A4" orientation="landscape" style={styles.page}>
          <View style={styles.headerRow}>
            <View>
              <Text style={styles.title}>{title}</Text>
              <Text style={styles.meta}>
                Generated {generatedAt}
                {pages.length > 1 ? ` — section ${i + 1} of ${pages.length}` : ""}
              </Text>
            </View>
            <View style={styles.legendRow}>
              {presentTypes.map((t) => (
                <View key={t} style={styles.legendItem}>
                  <View style={[styles.legendSwatch, { backgroundColor: hueFor(t, false) }]} />
                  <Text style={styles.legendText}>{t}</Text>
                </View>
              ))}
            </View>
          </View>
          <View style={styles.imageWrap}>
            <Image src={pg.src} style={{ width: pg.w, height: pg.h }} />
          </View>
          <View style={styles.footer} fixed>
            <Text style={styles.footerText}>{title}</Text>
            <Text style={styles.footerText}>
              Page {i + 1} of {pages.length}
            </Text>
          </View>
        </Page>
      ))}
    </Document>,
  ).toBlob();

  downloadBlob(blob, `${sanitizeFileName(title)}.pdf`);
}
