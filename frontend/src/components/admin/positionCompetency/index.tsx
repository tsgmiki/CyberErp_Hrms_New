"use client";
import { memo, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { Plus, Trash2, Save, Target } from "lucide-react";
import InventoryLayout from "@/components/common/inventoryLayout";
import getAllPosition from "@/services/admin/position/getAll";
import getAllCompetency from "@/services/admin/competency/getAll";
import { getPositionCompetencies, savePositionCompetencies } from "@/services/admin/positionCompetency";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

interface EditableRow {
  _key: number;
  competencyId?: string;
  weight?: number | string;
}

const num = (v: unknown): number => {
  const n = Number(v);
  return Number.isFinite(n) ? n : 0;
};

function PositionCompetency() {
  const { t } = useTranslation();
  const keyCounter = useRef(0);
  const nextKey = () => ++keyCounter.current;

  const [positionId, setPositionId] = useState("");
  const [rows, setRows] = useState<EditableRow[]>([]);
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  const [positionParam] = useState({ ...parameterInitialData, take: 500 });
  const { data: positions, isLoading: isPositionsLoading } = useQuery({
    queryKey: ["positions", positionParam],
    queryFn: () => getAllPosition(positionParam),
  });

  const [competencyParam] = useState({ ...parameterInitialData, take: 500 });
  const { data: competencies, isLoading: isCompetenciesLoading } = useQuery({
    queryKey: ["competencies", competencyParam],
    queryFn: () => getAllCompetency(competencyParam),
  });

  const { data: assigned, isLoading: isAssignedLoading } = useQuery({
    queryKey: ["positionCompetencies", positionId],
    queryFn: () => getPositionCompetencies(positionId),
    enabled: positionId !== "",
  });

  useEffect(() => {
    if (assigned) {
      setRows(assigned.map((a) => ({ _key: nextKey(), competencyId: a.competencyId, weight: a.weight })));
    } else {
      setRows([]);
    }
  }, [assigned]);

  const addRow = () => setRows((p) => [...p, { _key: nextKey(), competencyId: "", weight: 0 }]);
  const updateRow = (key: number, patch: Partial<EditableRow>) =>
    setRows((p) => p.map((r) => (r._key === key ? { ...r, ...patch } : r)));
  const removeRow = (key: number) => setRows((p) => p.filter((r) => r._key !== key));

  const totalWeight = rows.reduce((sum, r) => sum + num(r.weight), 0);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSaving(true);
    const result = await savePositionCompetencies({
      positionId,
      items: rows
        .filter((r) => r.competencyId)
        .map((r) => ({ competencyId: r.competencyId as string, weight: num(r.weight) })),
    });
    setFormState(result);
    setIsSaving(false);
  };

  return (
    <InventoryLayout
      title="Position Competencies"
      headerDescription="Assign weighted competencies to a position"
      headerIcon={<Target className="h-6 w-6 text-primary" />}
      showForm={false}
      hideAdd
      hideBack
      tableTitle="Position Competencies"
      onList={() => {}}
      onAdd={() => {}}
    >
      <form onSubmit={submit} className="space-y-5 p-4 text-foreground">
        {/* Position picker */}
        <section className="rounded-lg border border-border bg-card p-4">
          <label className={LABEL}>{t("Position")} *</label>
          {isPositionsLoading ? (
            <Loading />
          ) : (
            <select
              className={`${INPUT} sm:max-w-md`}
              value={positionId}
              onChange={(e) => {
                setPositionId(e.target.value);
                setFormState({});
              }}
              required
            >
              <option value="">{t("Select a position")}</option>
              {(positions?.data ?? []).map((p) => (
                <option key={p.id} value={p.id}>
                  {p.code} — {p.positionClassTitle ?? ""}
                </option>
              ))}
            </select>
          )}
        </section>

        {/* Competency rows */}
        {positionId && (
          <section className="rounded-lg border border-border bg-card p-4">
            <div className="mb-3 flex items-center justify-between">
              <div>
                <h3 className="text-sm font-semibold">{t("Competencies")}</h3>
                <p className="text-xs text-muted">{t("Total weight")}: {totalWeight}%</p>
              </div>
              <button type="button" onClick={addRow} className="inline-flex items-center gap-1 rounded bg-primary px-2.5 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90">
                <Plus className="h-3.5 w-3.5" /> {t("Add Competency")}
              </button>
            </div>

            {isAssignedLoading || isCompetenciesLoading ? (
              <Loading />
            ) : rows.length === 0 ? (
              <p className="py-6 text-center text-sm text-muted">{t("No competencies assigned yet.")}</p>
            ) : (
              <div className="space-y-2">
                {rows.map((r) => (
                  <div key={r._key} className="grid grid-cols-1 items-end gap-2 rounded-md border border-border/70 bg-secondary/20 p-2.5 md:grid-cols-[1fr_130px_auto]">
                    <div>
                      <label className={LABEL}>{t("Competency")} *</label>
                      <select className={INPUT} value={r.competencyId ?? ""} onChange={(e) => updateRow(r._key, { competencyId: e.target.value })} required>
                        <option value="">{t("Select competency")}</option>
                        {(competencies?.data ?? []).map((c) => (
                          <option key={c.id} value={c.id}>{c.name}</option>
                        ))}
                      </select>
                    </div>
                    <div>
                      <label className={LABEL}>{t("Weight (%)")}</label>
                      <input type="number" step="any" className={INPUT} value={r.weight ?? ""} onChange={(e) => updateRow(r._key, { weight: e.target.value })} />
                    </div>
                    <div className="flex items-center pb-1">
                      <button type="button" onClick={() => removeRow(r._key)} className="rounded p-1 text-error hover:bg-error/10" title={t("Remove") ?? ""}>
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}

            <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

            <div className="mt-4 flex justify-end">
              <button type="submit" disabled={isSaving} className="inline-flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
                <Save className="h-4 w-4" /> {isSaving ? t("Saving…") : t("Save Competencies")}
              </button>
            </div>
          </section>
        )}
      </form>
    </InventoryLayout>
  );
}

export default memo(PositionCompetency);
