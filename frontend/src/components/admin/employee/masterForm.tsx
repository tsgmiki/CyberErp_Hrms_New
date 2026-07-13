"use client";
import { memo, useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from "react";
import { useTranslation } from "react-i18next";
import {
  Camera,
  User,
  Phone,
  IdCard,
  Briefcase,
  ListPlus,
  ShieldCheck,
  Save,
  Loader2,
} from "lucide-react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import type { EmployeeModel, FormComponentModel } from "@/models";
import FormFieldRenderer from "@/components/common/formProvider/formUtility";
import { StatusMessage } from "../../common/statusMessage/status";
import saveEmployee from "@/services/admin/employee/save";
import getEmployee from "@/services/admin/employee/get";
import { employeePhotoUrl, uploadEmployeePhoto } from "@/services/admin/employee/photo";
import getAllEmployeeField from "@/services/admin/employeeField/getAll";
import { buildCustomFieldComponents } from "./customFieldConfigs";
import getAllPosition from "@/services/admin/position/getAll";
import getAllJobGrade from "@/services/admin/jobGrade/getAll";
import getAllSalaryScale from "@/services/admin/salaryScale/getAll";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";
import {
  genderOptions,
  maritalStatusOptions,
  employmentStatusOptions,
  employmentNatureOptions,
  yesNoOptions,
} from "@/constants/orgStructure";

const lookupParam = { ...parameterInitialData, take: 100 };
// Only the Employee form's own custom fields — child forms fetch their own owner type (HC021).
const activeFieldParam = { ...parameterInitialData, take: 200, status: "true", ownerType: "Employee" };
// Unique per-form id — avoids the FormProvider default-id collision when two forms mount together.
const FORM_ID = "employeeMasterForm";

const toDateInput = (v?: string) => (v ? v.slice(0, 10) : v);

/** A titled card grouping related fields — the core of the enterprise layout. */
function SectionCard({
  icon,
  title,
  description,
  children,
}: {
  icon: ReactNode;
  title: string;
  description?: string;
  children: ReactNode;
}) {
  const { t } = useTranslation();
  return (
    <section className="rounded-xl border border-border bg-card shadow-sm">
      <header className="flex items-start gap-3 border-b border-border px-5 py-3.5">
        <span className="mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
          {icon}
        </span>
        <div className="min-w-0">
          <h3 className="text-sm font-semibold text-foreground">{t(title)}</h3>
          {description ? <p className="mt-0.5 text-xs text-muted">{t(description)}</p> : null}
        </div>
      </header>
      <div className="grid grid-cols-1 gap-x-5 gap-y-4 p-5 sm:grid-cols-2">{children}</div>
    </section>
  );
}

/** Renders one field via the shared renderer in the modern label-above-input (auth) layout. */
const Field = memo(({ config }: { config: FormComponentModel }) => (
  <FormFieldRenderer component={{ ...config, layout: "auth" }} />
));
Field.displayName = "EmployeeField";

interface Props {
  id: string;
  /** Org unit selected in the tree — limits the Position dropdown to that unit. */
  orgUnitId?: string;
  orgUnitName?: string;
  onSaved: (savedId: string, isNew: boolean) => void;
}

function MasterForm({ id, orgUnitId, orgUnitName, onSaved }: Props) {
  const { t } = useTranslation();
  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as EmployeeModel);
  const [customData, setCustomData] = useState<Record<string, string>>({});
  const [photoFile, setPhotoFile] = useState<File | null>(null);
  const [photoPreview, setPhotoPreview] = useState<string | null>(null);
  const [photoCacheBust] = useState(() => Date.now());
  const fileInputRef = useRef<HTMLInputElement>(null);
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["employee", id],
    queryFn: () => getEmployee(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const { data: fieldDefs } = useQuery({
    queryKey: ["employeeFields", activeFieldParam],
    queryFn: () => getAllEmployeeField(activeFieldParam),
  });

  // Positions — scoped to the org unit selected in the tree (backend: parentId → unit).
  // Only vacant (open) positions are offered for assignment.
  const [positionParam, setPositionParam] = useState({
    ...lookupParam,
    isVacant: true,
    ...(orgUnitId ? { parentId: orgUnitId } : {}),
  });
  useEffect(() => {
    setPositionParam((p) => ({ ...p, parentId: orgUnitId || undefined, skip: 0 }));
  }, [orgUnitId]);
  const { data: positions, isLoading: positionsLoading } = useQuery({
    queryKey: ["positions", positionParam],
    queryFn: () => getAllPosition(positionParam),
  });

  const [gradeParam, setGradeParam] = useState({ ...lookupParam });
  const { data: grades, isLoading: gradesLoading } = useQuery({
    queryKey: ["jobGrades", gradeParam],
    queryFn: () => getAllJobGrade(gradeParam),
  });

  // Salary scales are scoped to the selected job grade.
  const [scaleParam, setScaleParam] = useState({ ...lookupParam });
  const { data: scales, isLoading: scalesLoading } = useQuery({
    queryKey: ["salaryScales", "byGrade", formData.jobGradeId, scaleParam],
    queryFn: () => getAllSalaryScale({ ...scaleParam, jobGradeId: formData.jobGradeId }),
    enabled: !!formData.jobGradeId,
  });

  // Dropdown shows vacant positions; on edit the employee's current (now occupied) position is
  // absent from that list, so re-add it to keep the existing placement visible and selectable.
  const positionOptions = useMemo(() => {
    const opts = (positions?.data ?? []).map((p) => ({
      id: p.id,
      name: `${p.code} — ${p.positionClassTitle ?? ""}${p.organizationUnitName ? ` · ${p.organizationUnitName}` : ""}`,
    }));
    if (formData.positionId && !opts.some((o) => o.id === formData.positionId)) {
      opts.unshift({
        id: formData.positionId,
        name: `${formData.positionCode ?? ""}${formData.positionClassTitle ? ` — ${formData.positionClassTitle}` : ""} (current)`.trim(),
      });
    }
    return opts;
  }, [positions, formData.positionId, formData.positionCode, formData.positionClassTitle]);

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);
  const selectHandler = useCallback((name: string, r: any) => {
    setFormData((p) => ({ ...p, [name]: r.id }));
  }, []);
  // Changing the job grade re-scopes the salary scale, so any previously-chosen scale is cleared.
  const jobGradeSelectHandler = useCallback((_name: string, r: any) => {
    setFormData((p) => ({
      ...p,
      jobGradeId: r.id,
      jobGradeName: r.name,
      salaryScaleId: undefined,
      salaryScaleStep: undefined,
    }));
  }, []);
  // Picking a salary scale records it and auto-fills the (still editable) salary.
  const salaryScaleSelectHandler = useCallback(
    (_name: string, r: any) => {
      const scale = (scales?.data ?? []).find((s) => s.id === r.id);
      setFormData((p) => ({
        ...p,
        salaryScaleId: r.id,
        salaryScaleStep: scale?.step,
        salary: scale?.salary ?? p.salary,
      }));
    },
    [scales],
  );
  const customChangeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setCustomData((p) => ({ ...p, [name]: value }));
  }, []);
  const customSelectHandler = useCallback((name: string, r: any) => {
    setCustomData((p) => ({ ...p, [name]: r.id }));
  }, []);

  useEffect(() => {
    if (typeof record != "undefined" && record != null) {
      setFormData({
        ...record,
        dateOfBirth: toDateInput(record.dateOfBirth),
        hireDate: toDateInput(record.hireDate),
      });
      const custom: Record<string, string> = {};
      for (const [k, v] of Object.entries(record.customFields ?? {})) custom[k] = v ?? "";
      setCustomData(custom);
    }
  }, [record]);

  const pickPhoto = (file: File | null) => {
    setPhotoFile(file);
    if (photoPreview) URL.revokeObjectURL(photoPreview);
    setPhotoPreview(file ? URL.createObjectURL(file) : null);
  };

  const submitHandler = async (e: any) => {
    e.preventDefault();
    setIsLoading(true);
    const isNew = !formData.id;
    const result = await saveEmployee({ ...formData, customFields: customData });

    // Attach the picked photo once the employee exists (HC015/HC023).
    if (result.status === "success" && result.id && photoFile) {
      const photoResult = await uploadEmployeePhoto(result.id, photoFile);
      if (photoResult.status === "error") {
        result.message = `Saved, but photo failed: ${photoResult.message}`;
      } else {
        setPhotoFile(null);
      }
    }

    setFormState(result);
    setIsLoading(false);
    if (result.status === "success" && result.id) {
      queryClient.invalidateQueries({ queryKey: ["employees"] });
      queryClient.invalidateQueries({ queryKey: ["employee", result.id] });
      onSaved(result.id, isNew);
    }
  };

  // Dynamic custom fields (HC021) → field configs for the "Additional Information" card. The master
  // form binds values to its own `customData` state, so it uses the shared renderer with no prefix.
  const customFieldConfigs = useMemo<FormComponentModel[]>(
    () => buildCustomFieldComponents(fieldDefs?.data, customData, customChangeHandler, customSelectHandler, "", formState?.zodErrors),
    [fieldDefs, customData, formState, customChangeHandler, customSelectHandler],
  );

  const existingPhoto = formData.id && formData.photoUrl ? employeePhotoUrl(formData.id, photoCacheBust) : null;
  const photoSrc = photoPreview ?? existingPhoto;
  const isNew = !formData.id;
  const previewName =
    [formData.firstName, formData.fatherName, formData.grandFatherName].filter(Boolean).join(" ").trim();
  const onProbation = formData.isProbation === true || formData.isProbation === "true";
  const isContract = formData.employmentNature === "Contract";

  return (
    <div className="relative">
      {pending && (
        <div className="absolute inset-0 z-20 flex items-center justify-center bg-card/60 backdrop-blur-[1px]">
          <Loading />
        </div>
      )}

      {/* Sticky action bar — the primary Save action is always reachable while scrolling. */}
      <div className="sticky top-0 z-10 -mx-1 mb-4 flex items-center justify-between gap-3 border-b border-border bg-background/85 px-4 py-2.5 backdrop-blur">
        <div className="min-w-0">
          <h2 className="truncate text-sm font-semibold text-foreground">
            {isNew ? t("New Employee") : previewName || t("Edit Employee")}
          </h2>
          <p className="text-[11px] text-muted">
            {isNew ? t("Create an employee profile") : t("Update the employee profile")}
          </p>
        </div>
        <button
          type="submit"
          form={FORM_ID}
          disabled={isLoading}
          className="inline-flex shrink-0 items-center gap-1.5 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-on-accent shadow-sm transition hover:opacity-90 disabled:opacity-50"
        >
          {isLoading ? <Loader2 size={15} className="animate-spin" /> : <Save size={15} />}
          {t("Save")}
        </button>
      </div>

      <form id={FORM_ID} onSubmit={submitHandler} className="space-y-4 px-4 pb-6">
        {/* Identity header — photo + live name/number/role summary (enterprise profile pattern). */}
        <section className="flex flex-wrap items-center gap-4 rounded-xl border border-border bg-gradient-to-br from-primary/5 to-card p-4 shadow-sm">
          <div className="relative">
            {photoSrc ? (
              <img src={photoSrc} alt="" className="h-20 w-20 rounded-full border border-border object-cover" />
            ) : (
              <span className="flex h-20 w-20 items-center justify-center rounded-full bg-primary/10">
                <Camera className="h-7 w-7 text-primary/60" />
              </span>
            )}
            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              title={photoSrc ? t("Change Photo") : t("Upload Photo")}
              className="absolute -bottom-1 -right-1 flex h-7 w-7 items-center justify-center rounded-full border border-border bg-card text-primary shadow-sm hover:bg-primary/10"
            >
              <Camera size={13} />
            </button>
          </div>
          <div className="min-w-0 flex-1">
            <h1 className="truncate text-lg font-semibold text-foreground">
              {previewName || t("New Employee")}
            </h1>
            <div className="mt-1 flex flex-wrap items-center gap-1.5">
              {formData.employeeNumber ? (
                <span className="rounded bg-secondary px-2 py-0.5 text-xs font-medium text-muted">
                  #{formData.employeeNumber}
                </span>
              ) : null}
              <span className="rounded bg-info/15 px-2 py-0.5 text-xs font-medium text-info">
                {t(formData.employmentStatus ?? "Active")}
              </span>
              {formData.isManagerial ? (
                <span className="inline-flex items-center gap-1 rounded bg-primary/15 px-2 py-0.5 text-xs font-semibold text-primary">
                  <ShieldCheck size={12} /> {t("Managerial")}
                </span>
              ) : null}
              {onProbation ? (
                <span className="rounded bg-warning/15 px-2 py-0.5 text-xs font-medium text-warning">
                  {t("On Probation")}
                </span>
              ) : null}
            </div>
            <p className="mt-1 text-[11px] text-muted">{t("JPG, PNG or WEBP · max 2 MB")}</p>
            {photoFile ? <p className="truncate text-[11px] text-primary">{photoFile.name}</p> : null}
          </div>
          <input
            ref={fileInputRef}
            type="file"
            accept="image/jpeg,image/png,image/webp"
            className="hidden"
            onChange={(e) => pickPhoto(e.target.files?.[0] ?? null)}
          />
        </section>

        {/* Personal details */}
        <SectionCard icon={<User size={16} />} title="Personal Details" description="Stored on the shared person record.">
          <Field config={{ name: "firstName", label: "First Name", required: true, value: formData.firstName, onChange: changeHandler, error: formState?.zodErrors?.firstName, type: "text" }} />
          <Field config={{ name: "firstNameA", label: "First Name (Amharic)", value: formData.firstNameA, onChange: changeHandler, type: "text" }} />
          <Field config={{ name: "fatherName", label: "Father Name", value: formData.fatherName, onChange: changeHandler, error: formState?.zodErrors?.fatherName, type: "text" }} />
          <Field config={{ name: "fatherNameA", label: "Father Name (Amharic)", value: formData.fatherNameA, onChange: changeHandler, type: "text" }} />
          <Field config={{ name: "grandFatherName", label: "Grandfather Name", required: true, value: formData.grandFatherName, onChange: changeHandler, error: formState?.zodErrors?.grandFatherName, type: "text" }} />
          <Field config={{ name: "grandFatherNameA", label: "Grandfather Name (Amharic)", value: formData.grandFatherNameA, onChange: changeHandler, type: "text" }} />
          <Field config={{ name: "gender", label: "Gender", required: true, type: "dropDown", onSelect: selectHandler, value: formData.gender, displayValue: formData.gender, error: formState?.zodErrors?.gender, data: genderOptions as never }} />
          <Field config={{ name: "maritalStatus", label: "Marital Status", required: true, type: "dropDown", onSelect: selectHandler, value: formData.maritalStatus, displayValue: formData.maritalStatus, error: formState?.zodErrors?.maritalStatus, data: maritalStatusOptions as never }} />
          <Field config={{ name: "dateOfBirth", label: "Date of Birth", value: formData.dateOfBirth, onChange: changeHandler, type: "date" }} />
          <Field config={{ name: "placeOfBirth", label: "Place of Birth", value: formData.placeOfBirth, onChange: changeHandler, type: "text" }} />
          <Field config={{ name: "spouseName", label: "Spouse Name", value: formData.spouseName, onChange: changeHandler, type: "text" }} />
        </SectionCard>

        {/* Contact */}
        <SectionCard icon={<Phone size={16} />} title="Contact">
          <Field config={{ name: "phoneNumber", label: "Phone Number", value: formData.phoneNumber, onChange: changeHandler, type: "text" }} />
          <Field config={{ name: "email", label: "Email", value: formData.email, onChange: changeHandler, error: formState?.zodErrors?.email, type: "text" }} />
          <Field config={{ name: "locationName", label: "Location / Address", value: formData.locationName, onChange: changeHandler, type: "textarea", colSpan: "full" }} />
        </SectionCard>

        {/* Identification */}
        <SectionCard icon={<IdCard size={16} />} title="Identification">
          <Field config={{ name: "employeeNumber", label: "Employee Number", required: true, value: formData.employeeNumber, onChange: changeHandler, error: formState?.zodErrors?.employeeNumber, type: "text" }} />
          <Field config={{ name: "nationalId", label: "National ID", value: formData.nationalId, onChange: changeHandler, type: "text" }} />
          <Field config={{ name: "tin", label: "TIN Number", value: formData.tin, onChange: changeHandler, type: "text" }} />
          <Field config={{ name: "pensionNumber", label: "Pension Number", value: formData.pensionNumber, onChange: changeHandler, type: "text" }} />
        </SectionCard>

        {/* Employment & placement */}
        <SectionCard icon={<Briefcase size={16} />} title="Employment & Placement" description="Position, pay point, and employment terms.">
          <Field config={{ name: "positionId", label: orgUnitName ? `Position (${orgUnitName})` : "Position", type: "dropDown", onSelect: selectHandler, value: formData.positionId, displayValue: formData.positionCode, param: positionParam, setParam: setPositionParam as any, isLoading: positionsLoading, data: positionOptions as never }} />
          <Field config={{ name: "jobGradeId", label: "Job Grade (filter)", type: "dropDown", onSelect: jobGradeSelectHandler, value: formData.jobGradeId, displayValue: formData.jobGradeName, placeholder: "Filter salary scales by grade", param: gradeParam, setParam: setGradeParam as any, isLoading: gradesLoading, data: (grades?.data ?? []).map((g) => ({ id: g.id, name: g.name })) as never }} />
          <Field config={{ name: "salaryScaleId", label: "Salary Scale (Step)", type: "dropDown", onSelect: salaryScaleSelectHandler, value: formData.salaryScaleId, displayValue: formData.salaryScaleStep, error: formState?.zodErrors?.salaryScaleId, disabled: !formData.jobGradeId, placeholder: formData.jobGradeId ? "Select a step" : "Select a job grade first", param: scaleParam, setParam: setScaleParam as any, isLoading: scalesLoading, data: (scales?.data ?? []).map((s) => ({ id: s.id, name: `${s.step ?? "Step"} — ${s.salary != null ? Number(s.salary).toLocaleString(undefined, { minimumFractionDigits: 2 }) : ""}` })) as never }} />
          <Field config={{ name: "salary", label: "Salary", value: formData.salary, onChange: changeHandler, error: formState?.zodErrors?.salary, inputType: "number", type: "text" }} />
          <Field config={{ name: "employmentStatus", label: "Employment Status", type: "dropDown", onSelect: selectHandler, value: formData.employmentStatus ?? "Active", displayValue: formData.employmentStatus ?? "Active", data: employmentStatusOptions as never }} />
          <Field config={{ name: "hireDate", label: "Hire Date", value: formData.hireDate, onChange: changeHandler, type: "date" }} />
          <Field config={{ name: "employmentNature", label: "Employment Nature", type: "dropDown", onSelect: selectHandler, value: formData.employmentNature ?? "Permanent", displayValue: formData.employmentNature ?? "Permanent", data: employmentNatureOptions as never }} />
          {isContract ? (
            <Field config={{ name: "contractPeriod", label: "Contract Period (months)", required: true, value: formData.contractPeriod, onChange: changeHandler, error: formState?.zodErrors?.contractPeriod, inputType: "number", type: "text" }} />
          ) : null}
          <Field config={{ name: "isProbation", label: "On Probation", type: "dropDown", onSelect: selectHandler, value: onProbation ? "true" : "false", displayValue: onProbation ? "Yes" : "No", data: yesNoOptions as never }} />
          {onProbation ? (
            <Field config={{ name: "probationEndDate", label: "Probation End Date", required: true, value: formData.probationEndDate, onChange: changeHandler, error: formState?.zodErrors?.probationEndDate, type: "date" }} />
          ) : null}

          {/* Managerial flag — a prominent, self-explaining control (drives leave entitlement / unit leadership). */}
          <label className="col-span-full flex cursor-pointer items-center gap-3 rounded-lg border border-border bg-secondary/30 p-3 transition hover:border-primary/40">
            <input
              type="checkbox"
              name="isManagerial"
              checked={!!formData.isManagerial}
              onChange={(e) => setFormData((p) => ({ ...p, isManagerial: e.target.checked }))}
              className="h-4 w-4 shrink-0 accent-primary"
            />
            <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
              <ShieldCheck size={16} />
            </span>
            <span className="min-w-0">
              <span className="block text-sm font-medium text-foreground">{t("Managerial position")}</span>
              <span className="block text-xs text-muted">
                {t("Managerial staff receive the managerial leave entitlement and can be assigned to head a unit.")}
              </span>
            </span>
          </label>
        </SectionCard>

        {/* Dynamic custom fields */}
        {customFieldConfigs.length > 0 ? (
          <SectionCard icon={<ListPlus size={16} />} title="Additional Information" description="Organization-defined custom fields.">
            {customFieldConfigs.map((cfg) => (
              <Field key={cfg.name} config={cfg} />
            ))}
          </SectionCard>
        ) : null}

        <input hidden name="id" value={formData.id ?? ""} readOnly />
      </form>

      <div className="px-4">
        <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
      </div>
    </div>
  );
}
export default memo(MasterForm);
