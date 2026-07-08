"use client";

import { useEffect, useMemo, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { FileText, History, UserX, UserCheck, AlertTriangle } from "lucide-react";
import DropDownField from "@/components/ui/dropDownField";
import { EntityListShell, useEntityList } from "@/template";
import {
  getTerminatedEmployees,
  getTerminations,
  getReinstatementInfo,
  reinstateEmployee,
} from "@/services/admin/employee/termination";
import { getMovements, getDisciplinaryMeasures } from "@/services/admin/employee/personnelActions";
import getAllPosition from "@/services/admin/position/getAll";
import { employeePhotoUrl } from "@/services/admin/employee/photo";
import GenerateDocumentModal from "../employee/generateDocumentModal";
import Modal from "@/components/common/modal";
import Loading from "@/components/common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";
import type { TerminatedEmployeeModel, TerminationClearanceModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";

const fmtDate = (v?: string) => (v ? v.slice(0, 10) : "—");

const CASE_TONE: Record<string, string> = {
  Initiated: "bg-info/15 text-info",
  ClearanceInProgress: "bg-warning/15 text-warning",
  Settled: "bg-success/15 text-success",
  Cancelled: "bg-muted/30 text-muted",
};
const CLEAR_TONE: Record<string, string> = {
  Pending: "bg-warning/15 text-warning",
  Cleared: "bg-success/15 text-success",
  Blocked: "bg-error/15 text-error",
};

function initialsOf(name?: string) {
  return (
    name
      ?.split(/\s+/)
      .map((p) => p[0])
      .join("")
      .toUpperCase()
      .slice(0, 2) || "?"
  );
}

function Avatar({ record }: { record: TerminatedEmployeeModel }) {
  if (record.photoUrl && record.employeeId) {
    return (
      <img
        src={employeePhotoUrl(record.employeeId)}
        alt=""
        className="h-8 w-8 shrink-0 rounded-full border border-border object-cover grayscale"
      />
    );
  }
  return (
    <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-muted/20 text-xs font-bold text-muted">
      {initialsOf(record.fullName)}
    </span>
  );
}

function SectionTitle({ children }: { children: React.ReactNode }) {
  return (
    <h4 className="border-b border-border px-4 py-2 text-xs font-bold uppercase tracking-wide text-muted">
      {children}
    </h4>
  );
}

/** Read-only clearance rows of one case (who cleared what, when). */
function ClearanceHistory({ items }: { items: TerminationClearanceModel[] }) {
  const { t } = useTranslation();
  if (!items.length) return null;
  return (
    <table className="w-full text-[13px]">
      <thead>
        <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
          <th className="px-4 py-1.5 font-semibold">{t("Department")}</th>
          <th className="px-4 py-1.5 font-semibold">{t("Status")}</th>
          <th className="px-4 py-1.5 font-semibold">{t("Cleared By")}</th>
          <th className="px-4 py-1.5 font-semibold">{t("Note")}</th>
        </tr>
      </thead>
      <tbody>
        {items.map((c) => (
          <tr key={c.id} className="border-b border-border/60">
            <td className="px-4 py-1.5 text-foreground">{c.department}</td>
            <td className="px-4 py-1.5">
              <span className={`rounded px-2 py-0.5 text-xs font-semibold ${CLEAR_TONE[c.status] ?? ""}`}>
                {t(c.status)}
              </span>
            </td>
            <td className="px-4 py-1.5 text-muted">
              {c.clearedBy ? `${c.clearedBy} · ${fmtDate(c.clearedAt)}` : "—"}
            </td>
            <td className="px-4 py-1.5 text-muted">{c.note || "—"}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

/** Complete history of one terminated employee: cases + clearances, movements, discipline. */
function HistoryModal({
  employee,
  onClose,
}: {
  employee: TerminatedEmployeeModel;
  onClose: () => void;
}) {
  const { t } = useTranslation();
  const employeeId = employee.employeeId;

  const { data: cases, isLoading: casesLoading } = useQuery({
    queryKey: ["employeeTerminations", employeeId],
    queryFn: () => getTerminations(employeeId),
  });
  const { data: movements, isLoading: movesLoading } = useQuery({
    queryKey: ["employeeMovements", employeeId],
    queryFn: () => getMovements(employeeId),
  });
  const { data: discipline, isLoading: discLoading } = useQuery({
    queryKey: ["disciplinaryMeasures", employeeId],
    queryFn: () => getDisciplinaryMeasures(employeeId),
  });

  const loading = casesLoading || movesLoading || discLoading;

  return (
    <Modal
      visible
      size="xl"
      title={`${t("Employment History")} — ${employee.fullName} (${employee.employeeNumber})`}
      onClose={onClose}
      footer={
        <button
          type="button"
          onClick={onClose}
          className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
        >
          {t("Close")}
        </button>
      }
    >
      {loading && <Loading />}
      {!loading && (
        <div className="space-y-3">
          {/* Termination cases with their clearance record */}
          <div className="rounded-lg border border-border bg-card">
            <SectionTitle>{t("Termination Cases")}</SectionTitle>
            {(cases ?? []).length === 0 && (
              <p className="px-4 py-3 text-sm text-muted">
                {t("No recorded termination case (status set directly).")}
              </p>
            )}
            {(cases ?? []).map((c) => (
              <div key={c.id} className="border-b border-border/60 last:border-b-0">
                <div className="flex flex-wrap items-center gap-2 px-4 py-2.5 text-sm">
                  <span className={`rounded px-2 py-0.5 text-xs font-semibold ${CASE_TONE[c.status ?? ""] ?? ""}`}>
                    {t(c.status ?? "")}
                  </span>
                  <span className="rounded bg-secondary px-2 py-0.5 text-xs text-foreground">
                    {t(c.terminationType ?? "")}
                  </span>
                  <span className="text-xs text-muted">
                    {t("Notice")}: {fmtDate(c.noticeDate)} · {t("Last day")}: {fmtDate(c.lastWorkingDate)}
                    {c.settledAt ? ` · ${t("Settled")}: ${fmtDate(c.settledAt)}` : ""}
                  </span>
                  <span className="w-full text-xs text-muted" title={c.reason}>
                    {c.reason}
                  </span>
                </div>
                <ClearanceHistory items={c.clearances ?? []} />
              </div>
            ))}
          </div>

          {/* Personnel movements */}
          <div className="rounded-lg border border-border bg-card">
            <SectionTitle>{t("Personnel Movements")}</SectionTitle>
            {(movements ?? []).length === 0 && (
              <p className="px-4 py-3 text-sm text-muted">{t("No recorded movements.")}</p>
            )}
            {(movements ?? []).length > 0 && (
              <table className="w-full text-[13px]">
                <thead>
                  <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
                    <th className="px-4 py-1.5 font-semibold">{t("Type")}</th>
                    <th className="px-4 py-1.5 font-semibold">{t("Effective Date")}</th>
                    <th className="px-4 py-1.5 font-semibold">{t("From → To")}</th>
                    <th className="px-4 py-1.5 font-semibold">{t("Status")}</th>
                  </tr>
                </thead>
                <tbody>
                  {(movements ?? []).map((m) => (
                    <tr key={m.id} className="border-b border-border/60">
                      <td className="px-4 py-1.5 text-foreground">{t(m.movementType ?? "")}</td>
                      <td className="px-4 py-1.5 text-foreground">{fmtDate(m.effectiveDate)}</td>
                      <td className="px-4 py-1.5 text-muted">
                        {(m.fromPositionName || "—") + " → " + (m.toPositionName || "—")}
                      </td>
                      <td className="px-4 py-1.5 text-muted">{t(m.status ?? "")}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>

          {/* Disciplinary record */}
          <div className="rounded-lg border border-border bg-card">
            <SectionTitle>{t("Disciplinary Record")}</SectionTitle>
            {(discipline ?? []).length === 0 && (
              <p className="px-4 py-3 text-sm text-muted">{t("No disciplinary cases.")}</p>
            )}
            {(discipline ?? []).length > 0 && (
              <table className="w-full text-[13px]">
                <thead>
                  <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
                    <th className="px-4 py-1.5 font-semibold">{t("Violation")}</th>
                    <th className="px-4 py-1.5 font-semibold">{t("Date")}</th>
                    <th className="px-4 py-1.5 font-semibold">{t("Measure")}</th>
                    <th className="px-4 py-1.5 font-semibold">{t("Status")}</th>
                  </tr>
                </thead>
                <tbody>
                  {(discipline ?? []).map((d) => (
                    <tr key={d.id} className="border-b border-border/60">
                      <td className="px-4 py-1.5 text-foreground">{d.violationType}</td>
                      <td className="px-4 py-1.5 text-foreground">{fmtDate(d.violationDate)}</td>
                      <td className="px-4 py-1.5 text-muted">{d.measureType}</td>
                      <td className="px-4 py-1.5 text-muted">{t(d.status ?? "")}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </div>
      )}
    </Modal>
  );
}

/**
 * Reinstate a terminated employee: restore the previous position when it is still vacant, otherwise
 * force the selection of a new vacant position (their previous one has since been filled).
 */
function ReinstateModal({
  employee,
  onClose,
  onDone,
}: {
  employee: TerminatedEmployeeModel;
  onClose: () => void;
  onDone: () => void;
}) {
  const { t } = useTranslation();
  const employeeId = employee.employeeId;
  const [positionId, setPositionId] = useState("");
  const [positionName, setPositionName] = useState("");
  const [touched, setTouched] = useState(false);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  // Server-side, searchable position lookup: only 10 rows are shown, but typing searches ALL
  // vacant positions (the search text is pushed to the API, not filtered client-side).
  const [positionParam, setPositionParam] = useState({
    ...parameterInitialData,
    take: 10,
    isVacant: true,
  });

  const { data: info, isLoading: infoLoading } = useQuery({
    queryKey: ["reinstatementInfo", employeeId],
    queryFn: () => getReinstatementInfo(employeeId),
  });
  const { data: positions, isLoading: positionsLoading } = useQuery({
    queryKey: ["positions", "vacant", positionParam],
    queryFn: () => getAllPosition(positionParam),
  });

  const previousAvailable = info?.previousPositionAvailable === true;

  // Default the selection to the previous position when it is still available (user can override).
  useEffect(() => {
    if (previousAvailable && info?.previousPositionId && !positionId) {
      setPositionId(info.previousPositionId);
      setPositionName(info.previousPositionTitle ?? "");
    }
  }, [previousAvailable, info?.previousPositionId, info?.previousPositionTitle, positionId]);

  const confirm = async () => {
    setTouched(true);
    if (!positionId) {
      setError(t("Select a vacant position to reinstate the employee to."));
      return;
    }
    setBusy(true);
    const res = await reinstateEmployee(employeeId, positionId);
    setBusy(false);
    if (!res.ok) {
      setError(res.message);
      return;
    }
    onDone();
  };

  return (
    <Modal
      visible
      size="md"
      title={t("Reinstate Employee")}
      description={`${employee.fullName} (${employee.employeeNumber})`}
      onClose={onClose}
      footer={
        <>
          <button
            type="button"
            onClick={onClose}
            className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
          >
            {t("Cancel")}
          </button>
          <button
            type="button"
            disabled={busy || infoLoading}
            onClick={confirm}
            className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-on-accent disabled:opacity-50"
          >
            <UserCheck size={16} /> {t("Reinstate")}
          </button>
        </>
      }
    >
      {infoLoading && <Loading />}
      {!infoLoading && info && (
        <div className="space-y-3">
          {previousAvailable ? (
            <p className="rounded-md border border-success/30 bg-success/10 px-3 py-2 text-sm text-foreground">
              {t("Previous position is available")}:{" "}
              <strong>{info.previousPositionTitle}</strong>. {t("It will be restored unless you pick another below.")}
            </p>
          ) : (
            <p className="flex items-start gap-2 rounded-md border border-warning/30 bg-warning/10 px-3 py-2 text-sm text-foreground">
              <AlertTriangle size={16} className="mt-0.5 shrink-0 text-warning" />
              <span>
                {info.previousPositionTitle ? (
                  <>
                    {t("Previous position")} <strong>{info.previousPositionTitle}</strong>{" "}
                    {info.previousPositionOccupiedBy
                      ? t("is now held by {{name}}.", { name: info.previousPositionOccupiedBy })
                      : t("is no longer available.")}{" "}
                  </>
                ) : (
                  <>{t("No previous position is on record.")} </>
                )}
                {t("Select a vacant position for the reinstatement.")}
              </span>
            </p>
          )}

          <DropDownField
            name="positionId"
            type="dropDown"
            label={previousAvailable ? t("Position") : t("Vacant Position")}
            required={!previousAvailable}
            placeholder={t("Search a vacant position…")}
            value={positionId}
            displayValue={positionName}
            isLoading={positionsLoading}
            param={positionParam}
            setParam={setPositionParam as never}
            data={
              (positions?.data ?? []).map((p) => ({
                id: p.id,
                name: `${p.code} — ${p.positionClassTitle ?? ""}${p.organizationUnitName ? ` · ${p.organizationUnitName}` : ""}`,
              })) as never
            }
            onSelect={(_name, item: { id: string; name: string }) => {
              setPositionId(item.id);
              setPositionName(item.name);
              setError(null);
            }}
          />

          {touched && !positionId && (
            <p className="text-xs text-error">{t("A vacant position is required.")}</p>
          )}
          {error && <p className="text-xs text-error">{error}</p>}
        </div>
      )}
    </Modal>
  );
}

/**
 * Termination List: all terminated employees (excluded from the main employee directory) with
 * their complete history and official-document generation (Experience / Termination letters).
 */
function TerminationList() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [historyFor, setHistoryFor] = useState<TerminatedEmployeeModel | null>(null);
  const [docFor, setDocFor] = useState<TerminatedEmployeeModel | null>(null);
  const [reinstateFor, setReinstateFor] = useState<TerminatedEmployeeModel | null>(null);

  const list = useEntityList({
    queryKey: "terminatedEmployees",
    fetchPage: getTerminatedEmployees,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "fullName",
          label: "Employee",
          render: (text: string, record: TerminatedEmployeeModel) => (
            <span className="flex items-center gap-2.5">
              <Avatar record={record} />
              <span className="min-w-0">
                <span className="block truncate font-semibold">{text}</span>
                <span className="block text-xs text-muted">{record.employeeNumber}</span>
              </span>
            </span>
          ),
        },
        {
          name: "terminationType",
          label: "Type",
          render: (text: string) =>
            text ? (
              <span className="rounded bg-secondary px-2 py-0.5 text-xs font-semibold text-foreground">
                {t(text)}
              </span>
            ) : (
              <span className="text-xs text-muted">—</span>
            ),
        },
        {
          name: "lastWorkingDate",
          label: "Last Working Date",
          render: (text: string) => fmtDate(text),
        },
        {
          name: "settledAt",
          label: "Settled At",
          render: (text: string) => fmtDate(text),
        },
        {
          name: "reason",
          label: "Reason",
          render: (text: string) => (
            <span className="block max-w-[260px] truncate text-muted" title={text}>
              {text || "—"}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: TerminatedEmployeeModel) => (
            <span className="inline-flex items-center gap-1">
              <button
                type="button"
                onClick={() => setHistoryFor(record)}
                title={t("View History")}
                className="inline-flex items-center gap-1 rounded border border-border px-2 py-1 text-xs text-foreground hover:border-primary hover:text-primary"
              >
                <History size={14} />
              </button>
              <button
                type="button"
                onClick={() => setDocFor(record)}
                title={t("Generate Document")}
                className="inline-flex items-center gap-1 rounded border border-border px-2 py-1 text-xs text-foreground hover:border-primary hover:text-primary"
              >
                <FileText size={14} />
              </button>
              <button
                type="button"
                onClick={() => setReinstateFor(record)}
                title={t("Reinstate Employee")}
                className="inline-flex items-center gap-1 rounded border border-border px-2 py-1 text-xs text-success hover:border-success hover:bg-success/10"
              >
                <UserCheck size={14} />
              </button>
            </span>
          ),
        },
      ] as DataTableColumnModel[],
    [t],
  );

  return (
    <div className="m-1 flex h-full min-h-0 flex-col rounded-lg border border-border bg-card">
      <div className="flex items-center gap-2 border-b border-border px-3 py-2 text-sm font-semibold text-foreground">
        <UserX size={16} className="text-primary" />
        {t("Termination List")}
        <span className="font-normal text-xs text-muted">
          — {t("terminated employees with their history and official documents")}
        </span>
      </div>
      <div className="min-h-0 flex-1 overflow-auto">
        <EntityListShell
          listKey="terminatedEmployees"
          listLabel="Terminated Employees"
          columns={columns}
          {...list}
        />
      </div>

      {historyFor && <HistoryModal employee={historyFor} onClose={() => setHistoryFor(null)} />}
      {docFor && (
        <GenerateDocumentModal
          employeeId={docFor.employeeId}
          employeeName={docFor.fullName}
          onClose={() => setDocFor(null)}
        />
      )}
      {reinstateFor && (
        <ReinstateModal
          employee={reinstateFor}
          onClose={() => setReinstateFor(null)}
          onDone={() => {
            setReinstateFor(null);
            // The employee leaves the Termination List and returns to the active directory;
            // their previous/target position vacancy also changed.
            queryClient.invalidateQueries({ queryKey: ["terminatedEmployees"] });
            queryClient.invalidateQueries({ queryKey: ["employees"] });
            queryClient.invalidateQueries({ queryKey: ["positions"] });
          }}
        />
      )}
    </div>
  );
}

export default TerminationList;
