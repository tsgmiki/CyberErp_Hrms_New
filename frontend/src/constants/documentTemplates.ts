/**
 * Ready-to-edit starter content for each document type. Inline styles only, so the HTML previews
 * and prints faithfully without external CSS. Tokens like {{FullName}} / {{Logo}} are resolved per
 * employee (and per company) at generation time.
 */
export interface DocumentTemplateSample {
  header?: string;
  body: string;
  footer?: string;
}

// A table (not flexbox) keeps the logo and text on the same line when printed —
// print engines render flexbox inconsistently, tables reliably.
const letterHeader = `
<table style="width:100%; border-collapse:collapse; border-bottom:2px solid #1e3a5f;">
  <tr>
    <td style="vertical-align:middle; padding-bottom:8px;">{{Logo}}</td>
    <td style="vertical-align:middle; text-align:right; padding-bottom:8px; font-family:Arial, sans-serif; font-size:12px; color:#334;">
      <div style="font-weight:bold; font-size:14px; color:#1e3a5f;">{{Branch}}</div>
      <div>Human Resources Department</div>
    </td>
  </tr>
</table>`;

const letterFooter = `
<div style="font-family:Arial, sans-serif; text-align:center;">
  This is a computer-generated document issued on {{Today}}.
</div>`;

export const documentTemplateSamples: Record<string, DocumentTemplateSample> = {
  EmploymentLetter: {
    header: letterHeader,
    body: `
<div style="font-family: Georgia, serif; color:#111; line-height:1.6;">
  <p style="text-align:right;">{{Today}}</p>
  <h2 style="text-align:center; text-transform:uppercase; letter-spacing:1px;">Employment Confirmation Letter</h2>
  <p>To Whom It May Concern,</p>
  <p>This is to certify that <strong>{{FullName}}</strong> (Employee No. {{EmployeeNumber}}) is
  employed at our organization as <strong>{{Position}}</strong> in the {{OrganizationUnit}} unit,
  {{Branch}} branch, since {{HireDate}}.</p>
  <p>Their current employment status is {{EmploymentStatus}} and they are assigned job grade
  {{JobGrade}}.</p>
  <p>This letter is issued upon the employee's request for whatever purpose it may serve.</p>
  <p style="margin-top:48px;">Sincerely,</p>
  <p><strong>Human Resources Department</strong></p>
</div>`,
    footer: letterFooter,
  },

  ExperienceLetter: {
    header: letterHeader,
    body: `
<div style="font-family: Georgia, serif; color:#111; line-height:1.6;">
  <p style="text-align:right;">{{Today}}</p>
  <h2 style="text-align:center; text-transform:uppercase; letter-spacing:1px;">Experience Letter</h2>
  <p>To Whom It May Concern,</p>
  <p>This is to certify that <strong>{{FullName}}</strong> (Employee No. {{EmployeeNumber}}) has
  served our organization as <strong>{{Position}}</strong> in the {{OrganizationUnit}} unit
  from {{HireDate}} to date.</p>
  <p>During their tenure, we found them to be diligent, professional and dedicated. We wish them
  every success in their future endeavours.</p>
  <p style="margin-top:48px;">Sincerely,</p>
  <p><strong>Human Resources Department</strong></p>
</div>`,
    footer: letterFooter,
  },

  ClearanceCertificate: {
    header: letterHeader,
    body: `
<div style="font-family: Georgia, serif; color:#111; line-height:1.6;">
  <p style="text-align:right;">{{Today}}</p>
  <h2 style="text-align:center; text-transform:uppercase; letter-spacing:1px;">Clearance Certificate</h2>
  <p>This is to certify that <strong>{{FullName}}</strong> (Employee No. {{EmployeeNumber}}),
  formerly holding the position of <strong>{{Position}}</strong> in the {{OrganizationUnit}} unit,
  has completed the organizational clearance process following separation effective
  <strong>{{TerminationDate}}</strong> (last working day {{LastWorkingDate}}).</p>
  <p>Departmental clearance status: <strong>{{ClearanceStatus}}</strong></p>
  <div style="margin:12px 0;">{{ClearanceTable}}</div>
  <p>Accordingly, the employee is hereby granted final clearance as of {{ClearanceDate}}.</p>
  <table style="width:100%; margin-top:48px; font-family:Arial, sans-serif;">
    <tr>
      <td style="text-align:left;">_____________________________<br/>Human Resources</td>
      <td style="text-align:center;">_____________________________<br/>Finance</td>
      <td style="text-align:right;">_____________________________<br/>Authorized Signature</td>
    </tr>
  </table>
</div>`,
    footer: letterFooter,
  },

  AnnualLeaveRequest: {
    header: letterHeader,
    body: `
<div style="font-family: Georgia, serif; color:#111; line-height:1.6;">
  <p style="text-align:right;">{{Today}}</p>
  <h2 style="text-align:center; text-transform:uppercase; letter-spacing:1px;">Annual Leave Request</h2>
  <table style="width:100%; font-family:Arial, sans-serif; font-size:13px; margin-bottom:12px;">
    <tr>
      <td style="padding:2px 0;"><strong>Employee:</strong> {{EmployeeName}} ({{EmployeeNumber}})</td>
      <td style="padding:2px 0; text-align:right;"><strong>Request Date:</strong> {{RequestDate}}</td>
    </tr>
    <tr>
      <td style="padding:2px 0;"><strong>Ledger (Fiscal Year):</strong> {{Ledger}}</td>
      <td style="padding:2px 0; text-align:right;"><strong>Available Balance:</strong> {{LedgerAvailable}}</td>
    </tr>
  </table>
  <div style="margin:12px 0;">{{LeaveDetailsTable}}</div>
  <p><strong>Total Leave Days Requested:</strong> {{TotalLeaveDays}}</p>
  <p><strong>Remark:</strong> {{Remark}}</p>
  <table style="width:100%; margin-top:48px; font-family:Arial, sans-serif;">
    <tr>
      <td style="text-align:left;">_____________________________<br/>Employee Signature</td>
      <td style="text-align:center;">_____________________________<br/>Supervisor</td>
      <td style="text-align:right;">_____________________________<br/>Human Resources</td>
    </tr>
  </table>
</div>`,
    footer: letterFooter,
  },

  IdCard: {
    body: `
<div style="width:320px; border:2px solid #1e3a5f; border-radius:10px; overflow:hidden; font-family: Arial, sans-serif;">
  <div style="display:flex; align-items:center; gap:8px; background:#1e3a5f; color:#fff; padding:8px 12px; font-weight:bold; letter-spacing:1px;">
    {{Logo}}<span>EMPLOYEE ID CARD</span>
  </div>
  <div style="display:flex; padding:12px; gap:12px;">
    <div style="flex:0 0 auto;">{{Photo}}</div>
    <div style="font-size:13px; color:#111; line-height:1.5;">
      <div style="font-size:15px; font-weight:bold;">{{FullName}}</div>
      <div>{{Position}}</div>
      <div style="color:#555;">ID: {{EmployeeNumber}}</div>
      <div style="color:#555;">{{OrganizationUnit}}</div>
      <div style="color:#555;">{{Branch}}</div>
    </div>
  </div>
</div>`,
  },
};
