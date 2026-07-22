"use client";

import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import { Pencil, Trash2, UserPlus, Undo2, X } from "lucide-react";
import { getAllCompanyAssets, deleteCompanyAsset, assignCompanyAsset, returnCompanyAsset } from "@/services/admin/companyAsset";
import EmployeePicker from "@/components/common/employeePicker";
import type { CompanyAssetModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import { assetCategoryOptions } from "@/constants/orgStructure";

interface Props {
  editHandler: (id: string) => void;
}

const STATUS_TONE: Record<string, string> = {
  Available: "bg-success/15 text-success",
  Assigned: "bg-info/15 text-info",
  Retired: "bg-muted/30 text-muted",
};

const categoryLabel = (id?: string) => assetCategoryOptions.find((o) => o.id === id)?.name ?? id ?? "—";

function CompanyAssetList({ editHandler }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [assignFor, setAssignFor] = useState<CompanyAssetModel | null>(null);
  const [pickId, setPickId] = useState("");
  const [pickName, setPickName] = useState("");
  const [busy, setBusy] = useState(false);
  const [actionMsg, setActionMsg] = useState("");

  const list = useEntityList({
    queryKey: "companyAssets",
    fetchPage: getAllCompanyAssets,
  });

  const refresh = (msg: string) => {
    setActionMsg(msg);
    queryClient.invalidateQueries({ queryKey: ["companyAssets"] });
  };

  const confirmAssign = async () => {
    if (!assignFor?.id || !pickId) return;
    setBusy(true);
    const res = await assignCompanyAsset(assignFor.id, pickId);
    setBusy(false);
    if (res.ok) {
      setAssignFor(null);
      setPickId("");
      setPickName("");
    }
    refresh(res.message);
  };

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Asset",
          sort: true,
          render: (text: string, record: CompanyAssetModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="text-left">
              <span className="block font-semibold">{text}</span>
              <span className="block text-xs text-muted">{record.serialNo}</span>
            </button>
          ),
        },
        { name: "category", label: "Category", render: (v: string) => categoryLabel(v) },
        {
          name: "status",
          label: "Status",
          render: (v: string) => (
            <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[v] ?? "bg-secondary/40 text-foreground"}`}>{v}</span>
          ),
        },
        {
          name: "assignedToName",
          label: "Assigned To",
          render: (v: string, record: CompanyAssetModel) =>
            v ? (
              <span>
                <span className="block">{v}</span>
                <span className="block text-xs text-muted">{record.assignedToNumber} · {String(record.assignedOn ?? "").slice(0, 10)}</span>
              </span>
            ) : (
              "—"
            ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: CompanyAssetModel) => (
            <span className="flex items-center gap-1.5">
              {record.status === "Available" && (
                <button type="button" title={t("Assign")} onClick={() => { setAssignFor(record); setPickId(""); setPickName(""); }} className="rounded p-1 text-muted hover:text-primary">
                  <UserPlus size={15} />
                </button>
              )}
              {record.status === "Assigned" && (
                <button type="button" title={t("Return to pool")} onClick={() => record.id && returnCompanyAsset(record.id).then((r) => refresh(r.message))} className="rounded p-1 text-muted hover:text-success">
                  <Undo2 size={15} />
                </button>
              )}
              <button type="button" title={t("Edit")} onClick={() => record.id && editHandler(record.id)} className="rounded p-1 text-muted hover:text-primary">
                <Pencil size={15} />
              </button>
              <button type="button" title={t("Delete")} onClick={() => record.id && deleteCompanyAsset(record.id).then((r) => refresh(r.message))} className="rounded p-1 text-muted hover:text-error">
                <Trash2 size={15} />
              </button>
            </span>
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, t],
  );

  return (
    <div className="space-y-2">
      {actionMsg && <p className="rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{actionMsg}</p>}
      <EntityListShell listKey="companyAssets" listLabel="Company Assets" columns={columns} {...list} />
      {assignFor && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="w-full max-w-sm rounded-lg border border-border bg-background p-4 shadow-xl">
            <div className="mb-3 flex items-center justify-between">
              <h3 className="text-sm font-semibold text-foreground">{t("Assign asset")}</h3>
              <button type="button" onClick={() => setAssignFor(null)} className="rounded p-1 text-muted hover:bg-secondary/40"><X size={16} /></button>
            </div>
            <p className="mb-3 text-xs text-muted">{assignFor.name} {assignFor.serialNo ? `· ${assignFor.serialNo}` : ""}</p>
            <label className="mb-1 block text-xs font-medium text-muted">{t("Employee")}</label>
            <EmployeePicker value={pickId} displayValue={pickName} onSelect={(id, name) => { setPickId(id); setPickName(name); }} />
            <div className="mt-4 flex justify-end gap-2">
              <button type="button" onClick={() => setAssignFor(null)} className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary/40">{t("Cancel")}</button>
              <button type="button" disabled={busy || !pickId} onClick={confirmAssign}
                className="rounded-md bg-primary px-3.5 py-1.5 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
                {busy ? t("Assigning…") : t("Assign")}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default CompanyAssetList;
