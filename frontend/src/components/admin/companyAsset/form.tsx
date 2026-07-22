"use client";
import { memo, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Save, Package } from "lucide-react";
import type { CompanyAssetModel } from "@/models";
import { getAllCompanyAssets, saveCompanyAsset } from "@/services/admin/companyAsset";
import { StatusMessage } from "../../common/statusMessage/status";
import { assetCategoryOptions } from "@/constants/orgStructure";
import { parameterInitialData } from "@/constants/initialization";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

function CompanyAssetForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [meta, setMeta] = useState<CompanyAssetModel>({ category: "ITEquipment" });
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  // Resolve the edited row from the paged list (no dedicated by-id endpoint).
  const [listParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: assets } = useQuery({ queryKey: ["companyAssets", listParam], queryFn: () => getAllCompanyAssets(listParam) });

  useEffect(() => {
    if (id) {
      const record = (assets?.data ?? []).find((a) => a.id === id);
      if (record) setMeta(record);
    } else {
      setMeta({ category: "ITEquipment" });
    }
  }, [id, assets]);

  const set = (name: keyof CompanyAssetModel, value: unknown) => setMeta((p) => ({ ...p, [name]: value }));

  const submit = async () => {
    setIsSaving(true);
    const result = await saveCompanyAsset({
      id: meta.id,
      name: meta.name,
      category: meta.category,
      serialNo: meta.serialNo || undefined,
      description: meta.description || undefined,
    });
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      queryClient.invalidateQueries({ queryKey: ["companyAssets"] });
      setId("");
    }
  };

  return (
    <div className="space-y-4 text-foreground">
      <div className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-3 flex items-center gap-2 text-sm font-semibold">
          <Package size={16} className="text-primary" /> {t("Asset")}
        </h3>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div>
            <label className={LABEL}>{t("Name")} *</label>
            <input type="text" className={INPUT} placeholder={t("e.g. ThinkPad T14")} value={meta.name ?? ""} onChange={(e) => set("name", e.target.value)} />
          </div>
          <div>
            <label className={LABEL}>{t("Category")} *</label>
            <select className={INPUT} value={meta.category ?? "ITEquipment"} onChange={(e) => set("category", e.target.value)}>
              {assetCategoryOptions.map((o) => (
                <option key={o.id} value={o.id}>{o.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label className={LABEL}>{t("Serial No.")}</label>
            <input type="text" className={INPUT} value={meta.serialNo ?? ""} onChange={(e) => set("serialNo", e.target.value)} />
          </div>
          <div className="sm:col-span-2">
            <label className={LABEL}>{t("Description")}</label>
            <textarea className={INPUT} rows={2} value={meta.description ?? ""} onChange={(e) => set("description", e.target.value)} />
          </div>
        </div>
        <div className="mt-4 flex justify-end">
          <button
            type="button"
            disabled={!meta.name?.trim() || isSaving}
            onClick={submit}
            className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50"
          >
            <Save size={14} /> {isSaving ? t("Saving…") : t("Save Asset")}
          </button>
        </div>
      </div>
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default memo(CompanyAssetForm);
