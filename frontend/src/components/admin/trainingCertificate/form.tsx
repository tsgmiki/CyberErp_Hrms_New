"use client";
import { memo, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Save, ScrollText } from "lucide-react";
import type { TrainingCertificateModel } from "@/models";
import { getAllTrainingCertificates, saveCertificate } from "@/services/admin/trainingCertificate";
import getAllTrainingCourse from "@/services/admin/trainingCourse/getAll";
import EmployeePicker from "@/components/common/employeePicker";
import { StatusMessage } from "../../common/statusMessage/status";
import { parameterInitialData } from "@/constants/initialization";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none disabled:opacity-60";
const LABEL = "block text-xs font-medium text-muted mb-1";

/** Manual record of a certificate (admin) — externally earned credentials, or edits to issued ones. */
function TrainingCertificateForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [meta, setMeta] = useState<TrainingCertificateModel>({});
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  // Resolve the edited row from the scoped list (no dedicated by-id endpoint needed).
  const [listParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: certificates } = useQuery({ queryKey: ["trainingCertificates", listParam], queryFn: () => getAllTrainingCertificates(listParam) });

  const [courseParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: courses } = useQuery({ queryKey: ["trainingCourses", courseParam], queryFn: () => getAllTrainingCourse(courseParam) });

  useEffect(() => {
    if (id) {
      const record = (certificates?.data ?? []).find((c) => c.id === id);
      if (record) setMeta({ ...record, issuedOn: record.issuedOn?.slice(0, 10), expiresOn: record.expiresOn?.slice(0, 10) });
    } else {
      setMeta({});
    }
  }, [id, certificates]);

  const set = (name: keyof TrainingCertificateModel, value: unknown) => setMeta((p) => ({ ...p, [name]: value }));

  const submit = async () => {
    setIsSaving(true);
    const result = await saveCertificate({
      id: meta.id,
      employeeId: meta.employeeId,
      trainingCourseId: meta.trainingCourseId || undefined,
      certificateNo: meta.certificateNo || undefined,
      title: meta.title,
      issuedOn: meta.issuedOn,
      expiresOn: meta.expiresOn || undefined,
      notes: meta.notes || undefined,
    });
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      queryClient.invalidateQueries({ queryKey: ["trainingCertificates"] });
      setId("");
    }
  };

  const canSave = !!meta.employeeId && !!meta.title && !!meta.issuedOn;

  return (
    <div className="space-y-4 text-foreground">
      <div className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-1 flex items-center gap-2 text-sm font-semibold">
          <ScrollText size={16} className="text-primary" /> {t("Certificate")}
        </h3>
        <p className="mb-3 text-xs text-muted">
          {t("Completion certificates are issued from the session's participants list; record external credentials here.")}
        </p>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div>
            <label className={LABEL}>{t("Employee")} *</label>
            <EmployeePicker
              value={meta.employeeId}
              displayValue={meta.employeeName}
              disabled={!!id}
              onSelect={(eid, name) => setMeta((p) => ({ ...p, employeeId: eid, employeeName: name }))}
            />
          </div>
          <div>
            <label className={LABEL}>{t("Title")} *</label>
            <input type="text" className={INPUT} placeholder={t("e.g. PMP Certification")} value={meta.title ?? ""} onChange={(e) => set("title", e.target.value)} />
          </div>
          <div>
            <label className={LABEL}>{t("Certificate No.")}</label>
            <input type="text" className={INPUT} disabled={!!id} placeholder={t("Blank = auto-generated")} value={meta.certificateNo ?? ""} onChange={(e) => set("certificateNo", e.target.value)} />
          </div>
          <div>
            <label className={LABEL}>{t("Related Course")}</label>
            <select className={INPUT} disabled={!!id} value={meta.trainingCourseId ?? ""} onChange={(e) => set("trainingCourseId", e.target.value || undefined)}>
              <option value="">{t("None (external)")}</option>
              {(courses?.data ?? []).map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
              {meta.trainingCourseId && !(courses?.data ?? []).some((c) => c.id === meta.trainingCourseId) && (
                <option value={meta.trainingCourseId}>{meta.courseName}</option>
              )}
            </select>
          </div>
          <div>
            <label className={LABEL}>{t("Issued On")} *</label>
            <input type="date" className={INPUT} value={meta.issuedOn ?? ""} onChange={(e) => set("issuedOn", e.target.value)} />
          </div>
          <div>
            <label className={LABEL}>{t("Expires On")}</label>
            <input type="date" className={INPUT} value={meta.expiresOn ?? ""} onChange={(e) => set("expiresOn", e.target.value)} />
          </div>
          <div className="sm:col-span-2">
            <label className={LABEL}>{t("Notes")}</label>
            <textarea className={INPUT} rows={2} value={meta.notes ?? ""} onChange={(e) => set("notes", e.target.value)} />
          </div>
        </div>
        <div className="mt-4 flex justify-end">
          <button
            type="button"
            disabled={!canSave || isSaving}
            onClick={submit}
            className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50"
          >
            <Save size={14} /> {isSaving ? t("Saving…") : t("Save Certificate")}
          </button>
        </div>
      </div>
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default memo(TrainingCertificateForm);
