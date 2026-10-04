"use client";

import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import { Pencil, Trash2, UserPlus, Undo2, X } from "lucide-react";
import { getAllCompanyAssets, deleteCompanyAsset, assignCompanyAsset, returnCompanyAsset } from "@/services/admin/companyAsset";
import EmployeePicker from "@/components/common/employeePicker";
import Modal from "@/components/common/modal";
import Badge, { type BadgeVariant } from "@/components/common/badge/badge";
import type { CompanyAssetModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import { EntityListShell, useEntityList } from "@/template";
import { assetCategoryOptions } from "@/constants/orgStructure";

interface Props {
  editHandler: (id: string) => void;
}

/** ⚠️ `status` is an `AssetStatus` ENUM NAME — `Enum.TryParse<AssetStatus>(…, true, …)`. */
const STATUS_FILTER: ListFilterDefinition[] = [
  {
    type: "select",
    paramKey: "status",
    label: "Status",
    options: [
      { value: "", label: "All" },
      { value: "Available", label: "Available" },
      { value: "Assigned", label: "Assigned" },
      { value: "Retired", label: "Retired" },
    ],
  },
];

/** Asset status → the shared Badge's variants, which the hand-rolled tones mapped onto anyway. */
const STATUS_VARIANT: Record<string, BadgeVariant> = {
  Available: "success",
  Assigned: "info",
  Retired: "muted",
};

/** An absent value reads as a dash, never a blank cell that looks like a rendering fault. */
const dash = <span className="text-muted">—</span>;

const categoryLabel = (id?: string) => assetCategoryOptions.find((o) => o.id === id)?.name ?? id ?? "";

function CompanyAssetList({ editHandler }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [assignFor, setAssignFor] = useState<CompanyAssetModel | null>(null);
  const [pickId, setPickId] = useState("");
  const [pickName, setPickName] = useState("");
  const [busy, setBusy] = useState(false);
  // ⚠️ The outcome travels with the message. It used to render muted grey either way, so a REFUSED
  // assign or delete looked exactly like a successful one — on a screen that moves custody of
  // company property.
  const [action, setAction] = useState<{ ok: boolean; message: string } | null>(null);

  const list = useEntityList({
    queryKey: "companyAssets",
    fetchPage: getAllCompanyAssets,
  });

  const refresh = (res: { ok: boolean; message: string }) => {
    setAction(res);
    if (res.ok) queryClient.invalidateQueries({ queryKey: ["companyAssets"] });
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
    refresh(res);
  };

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Asset",
          sort: true,
          gridPrimary: true,
          gridHideLabel: true,
          render: (text: string, record: CompanyAssetModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="text-left">
              <span className="block font-semibold text-primary hover:underline">{text || "—"}</span>
              {record.serialNo ? (
                <span className="block font-mono text-xs text-muted">{record.serialNo}</span>
              ) : null}
            </button>
          ),
        },
        {
          name: "category",
          label: "Category",
          responsive: "md",
          render: (v: string) => categoryLabel(v) || dash,
        },
        {
          name: "status",
          label: "Status",
          gridHighlight: true,
          // ⚠️ The fallback used to be `bg-secondary/40`, which is unregistered — an unrecognised
          // status rendered as unstyled text with no chip at all.
          render: (v: string) =>
            v ? <Badge variant={STATUS_VARIANT[v] ?? "secondary"}>{v}</Badge> : dash,
        },
        {
          name: "assignedToName",
          label: "Assigned To",
          render: (v: string, record: CompanyAssetModel) =>
            v ? (
              <span>
                <span className="block">{v}</span>
                <span className="block text-xs text-muted">
                  <span className="font-mono">{record.assignedToNumber}</span>
                  {record.assignedOn ? (
                    <>
                      {" · "}
                      <span className="tabular-nums">{String(record.assignedOn).slice(0, 10)}</span>
                    </>
                  ) : null}
                </span>
              </span>
            ) : (
              dash
            ),
        },
        {
          name: "Action",
          label: "Action",
          gridOmit: true,
          // ⚠️ ALL FOUR hovers emitted nothing: `hover:text-primary` (×2), `hover:text-success` and
          // `hover:text-error` are unregistered, so every button on this row was inert under the
          // pointer. They now carry their colour outright and dim on hover, which needs no palette
          // variant at all.
          render: (_t: unknown, record: CompanyAssetModel) => {
            const btn = "rounded p-1 transition-opacity hover:opacity-70";
            return (
              <span className="flex items-center gap-1.5">
                {record.status === "Available" && (
                  <button type="button" title={t("Assign")} onClick={() => { setAssignFor(record); setPickId(""); setPickName(""); }} className={`${btn} text-primary`}>
                    <UserPlus size={15} />
                  </button>
                )}
                {record.status === "Assigned" && (
                  <button type="button" title={t("Return to pool")} onClick={() => record.id && returnCompanyAsset(record.id).then(refresh)} className={`${btn} text-success`}>
                    <Undo2 size={15} />
                  </button>
                )}
                <button type="button" title={t("Edit")} onClick={() => record.id && editHandler(record.id)} className={`${btn} text-primary`}>
                  <Pencil size={15} />
                </button>
                <button type="button" title={t("Delete")} onClick={() => record.id && deleteCompanyAsset(record.id).then(refresh)} className={`${btn} text-error`}>
                  <Trash2 size={15} />
                </button>
              </span>
            );
          },
        },
      ] as DataTableColumnModel[],
    [editHandler, t],
  );

  return (
    <div className="space-y-2">
      {action && (
        /* border-success/20 and border-error/20 are the registered widths — /30 and /40 are not. */
        <p
          className={`flex items-start justify-between gap-2 rounded-lg border px-3 py-2 text-xs ${
            action.ok
              ? "border-success/20 bg-success/15 text-success"
              : "border-error/20 bg-error/15 text-error"
          }`}
        >
          <span>{action.message}</span>
          <button
            type="button"
            onClick={() => setAction(null)}
            className="shrink-0 font-semibold transition-opacity hover:opacity-70"
            aria-label={t("Dismiss") ?? "Dismiss"}
          >
            <X size={13} />
          </button>
        </p>
      )}
      <EntityListShell
        listKey="companyAssets"
        listLabel="Company Assets"
        columns={columns}
        listFilters={STATUS_FILTER}
        {...list}
      />
      {/*
        ⚠️ The shared Modal, not a hand-rolled `fixed inset-0` overlay. It was the only dialog on
        this screen built by hand, and both its close and cancel buttons used `hover:bg-secondary/40`
        — unregistered, so neither responded to the pointer.
        ⚠️ Safe for the EmployeePicker inside it: HRMS's Modal is a plain positioned div with
        role="dialog", NOT a native <dialog>, so it has no top layer and the picker's dropdown
        (a child, z-30) renders above the dialog body rather than behind it.
      */}
      {assignFor && (
        <Modal
          title={t("Assign asset")}
          description={`${assignFor.name ?? ""}${assignFor.serialNo ? ` · ${assignFor.serialNo}` : ""}`}
          visible
          size="sm"
          onClose={() => setAssignFor(null)}
          footer={
            <div className="flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setAssignFor(null)}
                className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground transition-opacity hover:opacity-70"
              >
                {t("Cancel")}
              </button>
              <button
                type="button"
                disabled={busy || !pickId}
                onClick={confirmAssign}
                className="rounded-md bg-primary px-3.5 py-1.5 text-sm font-semibold text-on-accent transition-opacity hover:opacity-90 disabled:opacity-50"
              >
                {busy ? t("Assigning…") : t("Assign")}
              </button>
            </div>
          }
        >
          <label className="mb-1 block text-xs font-medium text-muted">{t("Employee")}</label>
          <EmployeePicker
            value={pickId}
            displayValue={pickName}
            onSelect={(id, name) => { setPickId(id); setPickName(name); }}
          />
        </Modal>
      )}
    </div>
  );
}

export default CompanyAssetList;
