import type { AppraisalModel, AppraisalLineModel } from "@/models";

const esc = (v: unknown) =>
  String(v ?? "").replace(/[&<>"]/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" })[c] as string);
const d10 = (v?: string) => (v ? String(v).slice(0, 10) : "—");

const lineRows = (lines: AppraisalLineModel[] = []) =>
  lines
    .map(
      (l) => `<tr>
        <td>${esc(l.title)}</td>
        <td class="num">${esc(l.weight ?? 0)}%</td>
        <td class="num">${l.selfScore ?? "—"}</td>
        <td class="num">${l.managerScore ?? "—"}</td>
      </tr>`,
    )
    .join("");

const section = (heading: string, lines?: AppraisalLineModel[]) =>
  lines && lines.length > 0
    ? `<h3>${esc(heading)}</h3>
       <table><thead><tr><th>Item</th><th>Weight</th><th>Self</th><th>Manager</th></tr></thead>
       <tbody>${lineRows(lines)}</tbody></table>`
    : "";

/** Signatures block — only when the employee has accepted (HC146). */
const signatures = (a: AppraisalModel) =>
  a.acknowledgmentStatus === "Accepted"
    ? `<div class="sigs">
        <div class="sig"><div class="line">${esc(a.employeeSignature)}</div><div class="lbl">Employee — ${d10(a.employeeSignedAt)}</div></div>
        <div class="sig"><div class="line">${esc(a.managerSignature) || "&nbsp;"}</div><div class="lbl">Manager${a.managerSignedAt ? ` — ${d10(a.managerSignedAt)}` : ""}</div></div>
      </div>`
    : `<p class="pending">Not yet acknowledged by the employee — signatures pending.</p>`;

/** Opens a print-ready performance report in a new window and triggers the print dialog (save as PDF). */
export function printAppraisalReport(a: AppraisalModel) {
  const html = `<!doctype html><html><head><meta charset="utf-8" />
    <title>Performance Report — ${esc(a.employeeName)}</title>
    <style>
      * { box-sizing: border-box; }
      body { font-family: Arial, Helvetica, sans-serif; color: #1a1a1a; margin: 40px; font-size: 13px; }
      h1 { font-size: 20px; margin: 0 0 4px; }
      h3 { font-size: 14px; margin: 20px 0 6px; border-bottom: 1px solid #ccc; padding-bottom: 3px; }
      .meta { color: #555; margin-bottom: 8px; }
      .score { display: inline-block; margin-top: 8px; padding: 6px 12px; background: #f3f4f6; border-radius: 6px; font-weight: bold; }
      table { width: 100%; border-collapse: collapse; margin-bottom: 6px; }
      th, td { text-align: left; padding: 5px 8px; border-bottom: 1px solid #e5e7eb; }
      th { background: #f9fafb; font-size: 11px; text-transform: uppercase; letter-spacing: .04em; color: #555; }
      td.num, th.num { text-align: left; }
      .comments p { margin: 4px 0; }
      .sigs { display: flex; gap: 60px; margin-top: 48px; }
      .sig { flex: 1; }
      .sig .line { border-top: 1px solid #333; padding-top: 4px; font-style: italic; }
      .sig .lbl { color: #555; font-size: 11px; margin-top: 2px; }
      .pending { margin-top: 40px; color: #888; font-style: italic; }
      @media print { body { margin: 20px; } }
    </style></head><body>
      <h1>Performance Appraisal Report</h1>
      <div class="meta">${esc(a.employeeName)} &middot; ${esc(a.reviewCycleName)} &middot; Stage: ${esc(a.stage)}</div>
      ${a.periodStart ? `<div class="meta">Review period: ${d10(a.periodStart)} &ndash; ${d10(a.periodEnd)}</div>` : ""}
      ${a.overallScore != null ? `<div class="score">Overall Score: ${esc(a.overallScore)}${a.isCalibrated ? " (calibrated)" : ""}</div>` : ""}
      ${section("Goals", a.goals)}
      ${section("Competencies", a.competencies)}
      <div class="comments">
        ${a.selfComments ? `<h3>Self Comments</h3><p>${esc(a.selfComments)}</p>` : ""}
        ${a.managerComments ? `<h3>Manager Comments</h3><p>${esc(a.managerComments)}</p>` : ""}
      </div>
      <h3>Acknowledgment</h3>
      ${signatures(a)}
    </body></html>`;

  const w = window.open("", "_blank", "width=900,height=700");
  if (!w) return;
  w.document.write(html);
  w.document.close();
  w.focus();
  setTimeout(() => w.print(), 300);
}
