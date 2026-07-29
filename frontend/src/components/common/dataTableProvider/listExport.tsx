import {
  Document,
  Image,
  Page,
  StyleSheet,
  Text,
  View,
  pdf,
} from "@react-pdf/renderer";
import * as XLSX from "xlsx";
import type { Font } from "exceljs";
import { buildExportRows, buildExportSheetData, type ExportLabelFn } from "./listExportUtils";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListExportHeader } from "./listViewToolbar";

const pdfStyles = StyleSheet.create({
  page: {
    padding: 28,
    fontSize: 8,
    fontFamily: "Helvetica",
  },
  title: {
    fontSize: 12,
    fontWeight: "bold",
    marginBottom: 12,
  },
  meta: {
    fontSize: 8,
    color: "#64748b",
    marginBottom: 10,
  },
  table: {
    width: "100%",
  },
  headerRow: {
    flexDirection: "row",
    backgroundColor: "#f1f5f9",
    borderBottomWidth: 1,
    borderBottomColor: "#cbd5e1",
  },
  row: {
    flexDirection: "row",
    borderBottomWidth: 0.5,
    borderBottomColor: "#e2e8f0",
  },
  headerCell: {
    flex: 1,
    padding: 4,
    fontSize: 7,
    fontWeight: "bold",
  },
  cell: {
    flex: 1,
    padding: 4,
    fontSize: 7,
  },
  // ISO header block — three columns: logo (left) · company + report title (center) · date/time (right).
  headerBlock: {
    flexDirection: "row",
    alignItems: "center",
    marginBottom: 10,
    paddingBottom: 8,
    borderBottomWidth: 1,
    borderBottomColor: "#cbd5e1",
  },
  headerLeft: {
    width: 110,
  },
  headerCenter: {
    flex: 1,
    alignItems: "center",
  },
  headerRight: {
    width: 130,
    alignItems: "flex-end",
  },
  headerLogo: {
    height: 34,
    maxWidth: 100,
    objectFit: "contain",
  },
  headerCompany: {
    fontSize: 13,
    fontWeight: "bold",
    textAlign: "center",
  },
  headerTitle: {
    fontSize: 10,
    marginTop: 2,
    textAlign: "center",
  },
  headerDept: {
    fontSize: 9,
    fontWeight: "bold",
    color: "#334155",
    marginTop: 1,
    textAlign: "center",
  },
  headerMeta: {
    fontSize: 8,
    color: "#64748b",
    textAlign: "right",
  },
});

function sanitizeFileName(title: string): string {
  return title.replace(/[<>:"/\\|?*]+/g, "_").trim() || "export";
}

function downloadBlob(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = fileName;
  anchor.click();
  URL.revokeObjectURL(url);
}

function ListExportPdfDocument({
  title,
  headers,
  rows,
  generatedAt,
  header,
}: {
  title: string;
  headers: string[];
  rows: string[][];
  generatedAt: string;
  header?: ListExportHeader;
}) {
  return (
    <Document>
      <Page size="A4" orientation="landscape" style={pdfStyles.page}>
        {header ? (
          <View style={pdfStyles.headerBlock}>
            {/* Left — logo. */}
            <View style={pdfStyles.headerLeft}>
              {header.logoDataUrl ? <Image style={pdfStyles.headerLogo} src={header.logoDataUrl} /> : null}
            </View>
            {/* Center — company name (bold) with the report title directly underneath. */}
            <View style={pdfStyles.headerCenter}>
              {header.company ? <Text style={pdfStyles.headerCompany}>{header.company}</Text> : null}
              <Text style={pdfStyles.headerTitle}>{header.title ?? title}</Text>
              {header.headerTitle && header.headerTitle !== header.company
                ? <Text style={pdfStyles.headerDept}>{header.headerTitle}</Text> : null}
            </View>
            {/* Right — generation date/time. */}
            <View style={pdfStyles.headerRight}>
              <Text style={pdfStyles.headerMeta}>{header.generatedAt ?? generatedAt}</Text>
            </View>
          </View>
        ) : (
          <>
            <Text style={pdfStyles.title}>{title}</Text>
            <Text style={pdfStyles.meta}>{generatedAt}</Text>
          </>
        )}
        <View style={pdfStyles.table}>
          <View style={pdfStyles.headerRow}>
            {headers.map((header) => (
              <Text key={header} style={pdfStyles.headerCell}>
                {header}
              </Text>
            ))}
          </View>
          {rows.map((row, rowIndex) => (
            <View key={`row-${rowIndex}`} style={pdfStyles.row}>
              {row.map((cell, cellIndex) => (
                <Text key={`${rowIndex}-${cellIndex}`} style={pdfStyles.cell}>
                  {cell}
                </Text>
              ))}
            </View>
          ))}
        </View>
      </Page>
    </Document>
  );
}

/** Splits a data: URL into its base64 payload + an ExcelJS-supported extension (null = unsupported). */
function parseLogoDataUrl(dataUrl: string): { base64: string; extension: "png" | "jpeg" | "gif" } | null {
  const match = /^data:image\/(png|jpeg|jpg|gif);base64,(.+)$/i.exec(dataUrl);
  if (!match) return null;
  const ext = match[1].toLowerCase() === "jpg" ? "jpeg" : (match[1].toLowerCase() as "png" | "jpeg" | "gif");
  return { base64: match[2], extension: ext };
}

/**
 * Header-aware Excel export. SheetJS CE cannot embed images, so reports with an ISO header
 * block go through ExcelJS (lazy chunk — loaded only when a header export actually runs).
 */
async function exportExcelWithHeader(options: {
  title: string;
  data: Record<string, unknown>[] | undefined;
  columns: DataTableColumnModel[];
  labelFor: ExportLabelFn;
  header: ListExportHeader;
}): Promise<void> {
  const { headers, rows } = buildExportRows(options.data, options.columns, options.labelFor);
  if (rows.length === 0) return;

  const ExcelJS = (await import("exceljs")).default;
  const workbook = new ExcelJS.Workbook();
  const sheet = workbook.addWorksheet("data");
  const { header } = options;

  // ISO header — three zones across the table width: logo overlaid top-LEFT, company name + report
  // title CENTERED, generation date/time RIGHT-aligned.
  const colCount = Math.max(headers.length, 1);
  const merge = (row: number) => { if (colCount > 1) sheet.mergeCells(row, 1, row, colCount); };

  const logo = header.logoDataUrl ? parseLogoDataUrl(header.logoDataUrl) : null;
  if (logo) {
    const imageId = workbook.addImage({ base64: logo.base64, extension: logo.extension });
    sheet.addImage(imageId, { tl: { col: 0, row: 0 }, ext: { width: 120, height: 52 } });
  }

  // Centered lines: company name (bold) with the report title directly underneath, then any header line.
  const centerLines: { value: string; font: Partial<Font> }[] = [];
  if (header.company) centerLines.push({ value: header.company, font: { bold: true, size: 14 } });
  centerLines.push({ value: header.title ?? options.title, font: { bold: true, size: 11 } });
  if (header.headerTitle && header.headerTitle !== header.company)
    centerLines.push({ value: header.headerTitle, font: { bold: true, size: 9, color: { argb: "FF334155" } } });

  let rowIdx = 1;
  for (const ln of centerLines) {
    const cell = sheet.getCell(rowIdx, 1);
    cell.value = ln.value;
    cell.font = ln.font;
    cell.alignment = { horizontal: "center", vertical: "middle" };
    merge(rowIdx);
    rowIdx++;
  }
  if (header.generatedAt) {
    const cell = sheet.getCell(rowIdx, 1);
    cell.value = header.generatedAt;
    cell.font = { size: 9, color: { argb: "FF64748B" } };
    cell.alignment = { horizontal: "right", vertical: "middle" };
    merge(rowIdx);
    rowIdx++;
  }

  // Column header row sits below the header block (which clears the ~4-row logo) after a blank gap row.
  const headRowIdx = Math.max(rowIdx, 4) + 1;
  const headRow = sheet.getRow(headRowIdx);
  headers.forEach((h, i) => {
    const cell = headRow.getCell(i + 1);
    cell.value = h;
    cell.font = { bold: true };
    cell.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FFF1F5F9" } };
  });
  rows.forEach((r, i) => {
    const row = sheet.getRow(headRowIdx + 1 + i);
    r.forEach((cellValue, j) => { row.getCell(j + 1).value = cellValue; });
  });

  // Reasonable widths from content (capped so one long cell can't blow the layout up).
  headers.forEach((h, i) => {
    const maxLen = Math.max(h.length, ...rows.slice(0, 200).map((r) => (r[i] ?? "").length));
    sheet.getColumn(i + 1).width = Math.min(Math.max(maxLen + 2, 10), 50);
  });

  const buffer = await workbook.xlsx.writeBuffer();
  downloadBlob(
    new Blob([buffer], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }),
    `${sanitizeFileName(options.title)}.xlsx`,
  );
}

export async function exportListToExcel(options: {
  title: string;
  data: Record<string, unknown>[] | undefined;
  columns: DataTableColumnModel[];
  labelFor: ExportLabelFn;
  header?: ListExportHeader;
}): Promise<void> {
  if (options.header) {
    await exportExcelWithHeader({ ...options, header: options.header });
    return;
  }
  const sheetData = buildExportSheetData(options.data, options.columns, options.labelFor);
  if (sheetData.length === 0) return;

  const workbook = XLSX.utils.book_new();
  const worksheet = XLSX.utils.json_to_sheet(sheetData);
  XLSX.utils.book_append_sheet(workbook, worksheet, "data");
  XLSX.writeFile(workbook, `${sanitizeFileName(options.title)}.xlsx`);
}

export async function exportListToPdf(options: {
  title: string;
  data: Record<string, unknown>[] | undefined;
  columns: DataTableColumnModel[];
  labelFor: ExportLabelFn;
  header?: ListExportHeader;
}): Promise<void> {
  const { headers, rows } = buildExportRows(options.data, options.columns, options.labelFor);
  if (rows.length === 0) return;

  const generatedAt = new Date().toLocaleString();
  const blob = await pdf(
    ListExportPdfDocument({
      title: options.title,
      headers,
      rows,
      generatedAt,
      header: options.header,
    }),
  ).toBlob();

  downloadBlob(blob, `${sanitizeFileName(options.title)}.pdf`);
}
