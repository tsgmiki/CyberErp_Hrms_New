"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { ClearanceDepartmentModel, WorkflowApproverModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { X, UserRound, Shield } from "lucide-react";
import {
  saveClearanceDepartment,
  getClearanceDepartment,
} from "@/services/admin/clearanceDepartment";
import getAllUser from "@/services/admin/user/getAll";
import getAllRole from "@/services/admin/role/getAll";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";
import { activeStatusOptions, activeId, activeLabel } from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);
const lookupParam = { ...parameterInitialData, take: 100 };

function ClearanceDepartmentForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;
  const { t } = useTranslation();

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<ClearanceDepartmentModel>({ isActive: true, sortOrder: 0 });
  const [approvers, setApprovers] = useState<WorkflowApproverModel[]>([]);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["clearanceDepartment", id],
    queryFn: () => getClearanceDepartment(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const { data: users } = useQuery({
    queryKey: ["users", lookupParam],
    queryFn: () => getAllUser(lookupParam),
  });
  const { data: roles } = useQuery({
    queryKey: ["roles", lookupParam],
    queryFn: () => getAllRole(lookupParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    setIsLoading(true);
    const result = await saveClearanceDepartment({ ...formData, approvers });
    setFormState(result);
    setIsLoading(false);
  };

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);
  const selectHandler = useCallback((name: string, r: any) => {
    setFormData((p) => ({ ...p, [name]: r.id }));
  }, []);

  const addApprover = (approver: WorkflowApproverModel) =>
    setApprovers((p) =>
      p.some((a) => a.approverType === approver.approverType && a.approverId === approver.approverId)
        ? p
        : [...p, approver],
    );

  const removeApprover = (approver: WorkflowApproverModel) =>
    setApprovers((p) =>
      p.filter(
        (a) => !(a.approverType === approver.approverType && a.approverId === approver.approverId),
      ),
    );

  useEffect(() => {
    if (typeof record != "undefined" && record != null) {
      setFormData(record);
      setApprovers(
        (record.approvers ?? []).map((a) => ({
          approverType: a.approverType,
          approverId: a.approverId,
          displayName: a.displayName,
        })),
      );
    }
  }, [record]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({ isActive: true, sortOrder: 0 });
      setApprovers([]);
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["clearanceDepartments"] });
      setId("");
    }
  }, [formState]);

  const selectClass =
    "h-8 rounded-md border border-border bg-background px-2 text-xs text-foreground";

  return (
    <div className="text-foreground">
      {pending && <Loading />}
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[35%]",
          isPending: isLoading,
          SubmitButton: "top",
          components: [
            {
              name: "name", label: "Department Name", placeholder: "e.g. IT, Finance, Store",
              required: true, value: formData.name, onChange: changeHandler,
              error: formState?.zodErrors?.name, type: "text",
            },
            {
              name: "sortOrder", label: "Checklist Order", type: "text",
              placeholder: "0", value: formData.sortOrder, onChange: changeHandler,
            },
            {
              name: "isActive", label: "Status", type: "dropDown", onSelect: selectHandler,
              value: activeId(formData.isActive), displayValue: activeLabel(formData.isActive),
              data: activeStatusOptions as never,
            },
            {
              name: "description", label: "Clearance Requirement", required: true, type: "textarea",
              placeholder: "What must be returned / settled before this department clears",
              value: formData.description, onChange: changeHandler,
              error: formState?.zodErrors?.description,
            },
          ],
        }}
      />

      {/* Authorized approvers (mirrors the workflow step approver picker) */}
      <div className="mt-4 rounded-lg border border-border bg-card p-4">
        <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
          <div>
            <h4 className="text-sm font-semibold text-foreground">{t("Authorized Approvers")}</h4>
            <p className="text-xs text-muted-foreground">
              {t("Assign users and/or roles. Any single authorized user's approval clears this department. No approvers = open to any user.")}
            </p>
          </div>
          <div className="flex items-center gap-2">
            <select
              className={selectClass}
              value=""
              onChange={(e) => {
                const u = (users?.data ?? []).find((x) => x.id === e.target.value);
                if (u?.id) addApprover({ approverType: "User", approverId: u.id, displayName: u.fullName });
              }}
            >
              <option value="">{t("+ Add user approver")}</option>
              {(users?.data ?? []).map((u) => (
                <option key={u.id} value={u.id}>{u.fullName}</option>
              ))}
            </select>
            <select
              className={selectClass}
              value=""
              onChange={(e) => {
                const r = (roles?.data ?? []).find((x) => x.id === e.target.value);
                if (r?.id) addApprover({ approverType: "Role", approverId: r.id, displayName: r.name });
              }}
            >
              <option value="">{t("+ Add role approver")}</option>
              {(roles?.data ?? []).map((r) => (
                <option key={r.id} value={r.id}>{r.name}</option>
              ))}
            </select>
          </div>
        </div>

        {/* Approver chips */}
        <div className="flex flex-wrap gap-1.5">
          {approvers.length === 0 && (
            <span className="text-[11px] italic text-muted">
              {t("Open department — any user may clear it")}
            </span>
          )}
          {approvers.map((a) => (
            <span
              key={`${a.approverType}:${a.approverId}`}
              className={`inline-flex items-center gap-1 rounded-full border px-2 py-0.5 text-xs ${
                a.approverType === "Role"
                  ? "border-info/40 bg-info/10 text-info"
                  : "border-primary/40 bg-primary/10 text-primary"
              }`}
            >
              {a.approverType === "Role" ? <Shield size={11} /> : <UserRound size={11} />}
              {a.displayName}
              <button
                type="button"
                onClick={() => removeApprover(a)}
                className="ml-0.5 rounded-full hover:opacity-70"
                aria-label={t("Remove")}
              >
                <X size={11} />
              </button>
            </span>
          ))}
        </div>
      </div>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}
export default ClearanceDepartmentForm;
