"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { keepPreviousData, useQuery, useQueryClient } from "@tanstack/react-query";
import { Megaphone, Plus, X, Pin, Pencil, Trash2 } from "lucide-react";
import { getAllAnnouncements, saveAnnouncement, deleteAnnouncement } from "@/services/admin/engagement";
import getAllBranches from "@/services/admin/branch/getAll";
import getAllOrganizationUnits from "@/services/admin/organizationUnit/getAll";
import type { AnnouncementModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";
import Loading from "../../common/loader/loader";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

const emptyForm: AnnouncementModel = { audience: "All", isPinned: false, isActive: true };

/** HC206 — announcements admin: create/target/schedule/pin; employees read them on the News feed. */
function Announcement() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState<AnnouncementModel>({ ...emptyForm });
  const [busy, setBusy] = useState(false);
  const [msg, setMsg] = useState("");

  const [param] = useState({ ...parameterInitialData, take: 50 });
  const { data, isLoading } = useQuery({
    queryKey: ["announcements", param],
    queryFn: () => getAllAnnouncements(param),
    placeholderData: keepPreviousData,
  });
  const items = data?.data ?? [];

  // Small master lists (branches/units) for the audience target dropdowns.
  const { data: branches } = useQuery({
    queryKey: ["branchOptions"],
    queryFn: () => getAllBranches({ ...parameterInitialData, take: 200, sortCol: "name", dir: "asc" }),
    staleTime: 60_000,
    enabled: showForm && form.audience === "Branch",
  });
  const { data: units } = useQuery({
    queryKey: ["unitOptions"],
    queryFn: () => getAllOrganizationUnits({ ...parameterInitialData, take: 500, sortCol: "name", dir: "asc" }),
    staleTime: 60_000,
    enabled: showForm && form.audience === "Unit",
  });

  const refresh = (message: string) => {
    setMsg(message);
    queryClient.invalidateQueries({ queryKey: ["announcements"] });
    queryClient.invalidateQueries({ queryKey: ["announcementFeed"] });
  };

  const set = (patch: Partial<AnnouncementModel>) => setForm((f) => ({ ...f, ...patch }));

  const edit = (a: AnnouncementModel) => {
    setForm({
      ...a,
      publishFrom: a.publishFrom ? String(a.publishFrom).slice(0, 10) : "",
      publishUntil: a.publishUntil ? String(a.publishUntil).slice(0, 10) : "",
    });
    setShowForm(true);
  };

  const submit = async () => {
    setBusy(true);
    const res = await saveAnnouncement({
      ...form,
      branchId: form.audience === "Branch" ? form.branchId : undefined,
      organizationUnitId: form.audience === "Unit" ? form.organizationUnitId : undefined,
      publishFrom: form.publishFrom || undefined,
      publishUntil: form.publishUntil || undefined,
    });
    setBusy(false);
    refresh(res.message);
    if (res.ok) {
      setForm({ ...emptyForm });
      setShowForm(false);
    }
  };

  const targetInvalid =
    (form.audience === "Branch" && !form.branchId) || (form.audience === "Unit" && !form.organizationUnitId);

  return (
    <div className="flex h-full min-h-0 flex-col p-3">
      <div className="mb-3 flex items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><Megaphone className="h-5 w-5" /></span>
          <div>
            <h1 className="text-base font-semibold text-foreground">{t("Announcements")}</h1>
            <p className="text-xs text-muted">{t("Publish news to everyone, a branch, or an organizational unit (incl. its sub-units).")}</p>
          </div>
        </div>
        <button type="button" onClick={() => { setForm({ ...emptyForm }); setShowForm((v) => !v); }}
          className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-sm font-semibold text-on-accent hover:opacity-90">
          {showForm ? <X size={14} /> : <Plus size={14} />} {showForm ? t("Cancel") : t("New Announcement")}
        </button>
      </div>

      {msg && <p className="mb-2 rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{msg}</p>}

      {showForm && (
        <div className="mb-3 rounded-lg border border-border bg-card p-4">
          <div className="grid grid-cols-1 gap-3 md:grid-cols-4">
            <div className="md:col-span-4">
              <label className={LABEL}>{t("Title")} *</label>
              <input type="text" className={INPUT} value={form.title ?? ""} onChange={(e) => set({ title: e.target.value })} />
            </div>
            <div className="md:col-span-4">
              <label className={LABEL}>{t("Body")} *</label>
              <textarea className={INPUT} rows={4} value={form.body ?? ""} onChange={(e) => set({ body: e.target.value })} />
            </div>
            <div>
              <label className={LABEL}>{t("Audience")}</label>
              <select className={INPUT} value={form.audience} onChange={(e) => set({ audience: e.target.value, branchId: undefined, organizationUnitId: undefined })}>
                <option value="All">{t("Everyone")}</option>
                <option value="Branch">{t("One branch")}</option>
                <option value="Unit">{t("One unit (and sub-units)")}</option>
              </select>
            </div>
            {form.audience === "Branch" && (
              <div>
                <label className={LABEL}>{t("Branch")} *</label>
                <select className={INPUT} value={form.branchId ?? ""} onChange={(e) => set({ branchId: e.target.value || undefined })}>
                  <option value="">{t("Select…")}</option>
                  {(branches?.data ?? []).map((b) => <option key={b.id} value={b.id}>{b.name}</option>)}
                </select>
              </div>
            )}
            {form.audience === "Unit" && (
              <div>
                <label className={LABEL}>{t("Organization unit")} *</label>
                <select className={INPUT} value={form.organizationUnitId ?? ""} onChange={(e) => set({ organizationUnitId: e.target.value || undefined })}>
                  <option value="">{t("Select…")}</option>
                  {(units?.data ?? []).map((u) => <option key={u.id} value={u.id}>{u.name}</option>)}
                </select>
              </div>
            )}
            <div>
              <label className={LABEL}>{t("Publish from")}</label>
              <input type="date" className={INPUT} value={form.publishFrom ?? ""} onChange={(e) => set({ publishFrom: e.target.value })} />
            </div>
            <div>
              <label className={LABEL}>{t("Publish until")}</label>
              <input type="date" className={INPUT} value={form.publishUntil ?? ""} onChange={(e) => set({ publishUntil: e.target.value })} />
            </div>
            <div className="flex items-end gap-4 pb-2 text-xs text-muted md:col-span-4">
              <label className="flex items-center gap-2">
                <input type="checkbox" checked={!!form.isPinned} onChange={(e) => set({ isPinned: e.target.checked })} />
                {t("Pin to top of the feed")}
              </label>
              <label className="flex items-center gap-2">
                <input type="checkbox" checked={!!form.isActive} onChange={(e) => set({ isActive: e.target.checked })} />
                {t("Active")}
              </label>
            </div>
          </div>
          <div className="mt-3 flex justify-end">
            <button type="button" disabled={busy || !form.title?.trim() || !form.body?.trim() || targetInvalid} onClick={submit}
              className="rounded-md bg-primary px-3.5 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
              {busy ? t("Saving…") : form.id ? t("Update Announcement") : t("Publish Announcement")}
            </button>
          </div>
        </div>
      )}

      {isLoading ? (
        <Loading />
      ) : items.length === 0 ? (
        <p className="rounded-lg border border-dashed border-border bg-card/40 p-8 text-center text-sm text-muted">{t("No announcements yet.")}</p>
      ) : (
        <div className="min-h-0 flex-1 overflow-auto rounded-lg border border-border bg-card">
          <table className="w-full text-sm">
            <thead className="sticky top-0 bg-secondary/30 text-left text-xs uppercase tracking-wide text-muted">
              <tr>
                <th className="px-3 py-2">{t("Title")}</th>
                <th className="px-3 py-2">{t("Audience")}</th>
                <th className="px-3 py-2">{t("Window")}</th>
                <th className="px-3 py-2">{t("Status")}</th>
                <th className="px-3 py-2 text-right">{t("Actions")}</th>
              </tr>
            </thead>
            <tbody>
              {items.map((a) => (
                <tr key={a.id} className="border-t border-border/60 hover:bg-secondary/20">
                  <td className="px-3 py-2 font-medium text-foreground">
                    <span className="inline-flex items-center gap-1.5">
                      {a.isPinned && <Pin size={12} className="text-warning" />}{a.title}
                    </span>
                  </td>
                  <td className="px-3 py-2 text-muted">
                    {a.audience === "All" ? t("Everyone") : a.audience === "Branch" ? `${t("Branch")}: ${a.branchName ?? ""}` : `${t("Unit")}: ${a.organizationUnitName ?? ""}`}
                  </td>
                  <td className="px-3 py-2 text-muted">
                    {a.publishFrom ? String(a.publishFrom).slice(0, 10) : "—"} → {a.publishUntil ? String(a.publishUntil).slice(0, 10) : "—"}
                  </td>
                  <td className="px-3 py-2">
                    <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${a.isActive ? "bg-success/15 text-success" : "bg-muted/30 text-muted"}`}>
                      {a.isActive ? t("Active") : t("Inactive")}
                    </span>
                  </td>
                  <td className="px-3 py-2 text-right">
                    <button type="button" title={t("Edit")} onClick={() => edit(a)} className="rounded p-1 text-muted hover:text-primary"><Pencil size={14} /></button>
                    <button type="button" title={t("Delete")} onClick={() => a.id && deleteAnnouncement(a.id).then((r) => refresh(r.message))}
                      className="rounded p-1 text-muted hover:text-error"><Trash2 size={14} /></button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default memo(Announcement);
