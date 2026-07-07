"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { Camera } from "lucide-react";
import type { EmployeeModel, FormComponentModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveEmployee from "@/services/admin/employee/save";
import getEmployee from "@/services/admin/employee/get";
import { employeePhotoUrl, uploadEmployeePhoto } from "@/services/admin/employee/photo";
import getAllEmployeeField from "@/services/admin/employeeField/getAll";
import getAllPosition from "@/services/admin/position/getAll";
import getAllJobGrade from "@/services/admin/jobGrade/getAll";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";
import {
  genderOptions,
  maritalStatusOptions,
  employmentStatusOptions,
  yesNoOptions,
} from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);
const lookupParam = { ...parameterInitialData, take: 100 };
const activeFieldParam = { ...parameterInitialData, take: 200, status: "true" };

const toDateInput = (v?: string) => (v ? v.slice(0, 10) : v);

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
  const formRef = React.createRef<HTMLFormElement>();
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

  const customComponents = useMemo<FormComponentModel[]>(() => {
    const defs = fieldDefs?.data ?? [];
    if (defs.length === 0) return [];
    const rendered: FormComponentModel[] = [
      { type: "break", label: "Additional Information", name: "breakCustom" },
    ];
    for (const def of defs) {
      const name = def.name ?? "";
      const common = {
        name,
        label: def.label,
        required: def.isRequired,
        error: formState?.zodErrors?.[name],
      };
      switch (def.dataType) {
        case "Number":
          rendered.push({ ...common, type: "text", inputType: "number", value: customData[name], onChange: customChangeHandler });
          break;
        case "Date":
          rendered.push({ ...common, type: "date", value: customData[name], onChange: customChangeHandler });
          break;
        case "Boolean":
          rendered.push({
            ...common, type: "dropDown", onSelect: customSelectHandler,
            value: customData[name],
            displayValue: customData[name] === "true" ? "Yes" : customData[name] === "false" ? "No" : "",
            data: yesNoOptions as never,
          });
          break;
        case "Select":
          rendered.push({
            ...common, type: "dropDown", onSelect: customSelectHandler,
            value: customData[name], displayValue: customData[name],
            data: (def.options ?? "")
              .split(",")
              .map((o) => o.trim())
              .filter(Boolean)
              .map((o) => ({ id: o, name: o })) as never,
          });
          break;
        default:
          rendered.push({ ...common, type: "text", value: customData[name], onChange: customChangeHandler });
      }
    }
    return rendered;
  }, [fieldDefs, customData, formState, customChangeHandler, customSelectHandler]);

  const existingPhoto = formData.id && formData.photoUrl ? employeePhotoUrl(formData.id, photoCacheBust) : null;
  const photoSrc = photoPreview ?? existingPhoto;

  return (
    <div>
      {pending && <Loading />}

      {/* Photo picker (HC015) — uploaded on save */}
      <div className="mx-4 mb-1 mt-2 flex items-center gap-4 rounded-lg border border-border bg-card p-3">
        <div className="relative">
          {photoSrc ? (
            <img src={photoSrc} alt="" className="h-16 w-16 rounded-full border border-border object-cover" />
          ) : (
            <span className="flex h-16 w-16 items-center justify-center rounded-full bg-primary/10">
              <Camera className="h-6 w-6 text-primary/60" />
            </span>
          )}
        </div>
        <div className="min-w-0">
          <button
            type="button"
            onClick={() => fileInputRef.current?.click()}
            className="rounded border border-border bg-secondary px-3 py-1.5 text-xs font-semibold text-foreground hover:bg-primary/10"
          >
            {photoSrc ? t("Change Photo") : t("Upload Photo")}
          </button>
          <p className="mt-1 text-[11px] text-muted">{t("JPG, PNG or WEBP · max 2 MB")}</p>
          {photoFile && <p className="truncate text-[11px] text-primary">{photoFile.name}</p>}
        </div>
        <input
          ref={fileInputRef}
          type="file"
          accept="image/jpeg,image/png,image/webp"
          className="hidden"
          onChange={(e) => pickPhoto(e.target.files?.[0] ?? null)}
        />
      </div>

      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[35%]",
          isPending: isLoading,
          SubmitButton: "top",
          components: [
            { type: "break", label: "Personal Details", name: "breakPersonal",
              sectionDescription: "Stored on the shared person record (Core.CorePerson)." },
            { name: "firstName", label: "First Name", required: true, value: formData.firstName, onChange: changeHandler, error: formState?.zodErrors?.firstName, type: "text" },
            { name: "firstNameA", label: "First Name (Amharic)", value: formData.firstNameA, onChange: changeHandler, type: "text" },
            { name: "fatherName", label: "Father Name", value: formData.fatherName, onChange: changeHandler, error: formState?.zodErrors?.fatherName, type: "text" },
            { name: "fatherNameA", label: "Father Name (Amharic)", value: formData.fatherNameA, onChange: changeHandler, type: "text" },
            { name: "grandFatherName", label: "Grandfather Name", required: true, value: formData.grandFatherName, onChange: changeHandler, error: formState?.zodErrors?.grandFatherName, type: "text" },
            { name: "grandFatherNameA", label: "Grandfather Name (Amharic)", value: formData.grandFatherNameA, onChange: changeHandler, type: "text" },
            {
              name: "gender", label: "Gender", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.gender, displayValue: formData.gender,
              error: formState?.zodErrors?.gender, data: genderOptions as never,
            },
            {
              name: "maritalStatus", label: "Marital Status", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.maritalStatus, displayValue: formData.maritalStatus,
              error: formState?.zodErrors?.maritalStatus, data: maritalStatusOptions as never,
            },
            { name: "dateOfBirth", label: "Date of Birth", value: formData.dateOfBirth, onChange: changeHandler, type: "date" },
            { name: "placeOfBirth", label: "Place of Birth", value: formData.placeOfBirth, onChange: changeHandler, type: "text" },
            { name: "spouseName", label: "Spouse Name", value: formData.spouseName, onChange: changeHandler, type: "text" },
            { name: "phoneNumber", label: "Phone Number", value: formData.phoneNumber, onChange: changeHandler, type: "text" },
            { name: "email", label: "Email", value: formData.email, onChange: changeHandler, error: formState?.zodErrors?.email, type: "text" },
            { name: "locationName", label: "Location / Address", value: formData.locationName, onChange: changeHandler, type: "textarea", colSpan: "full" },

            { type: "break", label: "Identification", name: "breakIdentification" },
            { name: "employeeNumber", label: "Employee Number", required: true, value: formData.employeeNumber, onChange: changeHandler, error: formState?.zodErrors?.employeeNumber, type: "text" },
            { name: "nationalId", label: "National ID", value: formData.nationalId, onChange: changeHandler, type: "text" },
            { name: "tin", label: "TIN Number", value: formData.tin, onChange: changeHandler, type: "text" },
            { name: "pensionNumber", label: "Pension Number", value: formData.pensionNumber, onChange: changeHandler, type: "text" },

            { type: "break", label: "Employment & Placement", name: "breakEmployment" },
            {
              // The organization unit is derived from the position — no unit selector here.
              name: "positionId",
              label: orgUnitName ? `Position (${orgUnitName})` : "Position",
              type: "dropDown", onSelect: selectHandler,
              value: formData.positionId, displayValue: formData.positionCode,
              param: positionParam, setParam: setPositionParam as any, isLoading: positionsLoading,
              data: positionOptions as never,
            },
            {
              name: "jobGradeId", label: "Job Grade", type: "dropDown", onSelect: selectHandler,
              value: formData.jobGradeId, displayValue: formData.jobGradeName,
              param: gradeParam, setParam: setGradeParam as any, isLoading: gradesLoading,
              data: (grades?.data ?? []).map((g) => ({ id: g.id, name: g.name })) as never,
            },
            { name: "salary", label: "Salary", value: formData.salary, onChange: changeHandler, error: formState?.zodErrors?.salary, inputType: "number", type: "text" },
            {
              name: "employmentStatus", label: "Employment Status", type: "dropDown", onSelect: selectHandler,
              value: formData.employmentStatus ?? "Active", displayValue: formData.employmentStatus ?? "Active",
              data: employmentStatusOptions as never,
            },
            { name: "hireDate", label: "Hire Date", value: formData.hireDate, onChange: changeHandler, type: "date" },

            ...customComponents,
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}
export default MasterForm;
