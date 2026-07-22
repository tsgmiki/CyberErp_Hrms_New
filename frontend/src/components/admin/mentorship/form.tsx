"use client";
import { memo, useCallback, useEffect, useState } from "react";
import React from "react";
import { Handshake, CalendarClock } from "lucide-react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import type { MentorshipModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { FormUtility } from "@/components/common/formProvider/formUtility";
import { EntityFormTabs } from "@/components/common/tabs/entityFormTabs";
import EmployeePicker from "@/components/common/employeePicker";
import saveMentorship from "@/services/admin/mentorship/save";
import getMentorship from "@/services/admin/mentorship/get";
import { mentorshipContextOptions, mentorshipStatusOptions } from "@/constants/careerDevelopment";

const label = (opts: { id: string; name: string }[], v?: string) => opts.find((o) => o.id === v)?.name ?? (v ?? "");

function MentorshipForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as MentorshipModel);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["mentorship", id],
    queryFn: () => getMentorship(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    setFormState(await saveMentorship(fd));
    setIsLoading(false);
  };

  const changeHandler = useCallback((e: any) => setFormData((p) => ({ ...p, [e.target.name]: e.target.value })), []);
  const selectHandler = useCallback((name: string, r: any) => setFormData((p) => ({ ...p, [name]: r.id })), []);

  useEffect(() => { if (record) setFormData(record); }, [record]);
  useEffect(() => {
    if (formState.status == "success") {
      setFormData({} as MentorshipModel);
      formRef.current?.reset();
      queryClient.invalidateQueries({ queryKey: ["mentorships"] });
      setId("");
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formState]);

  return (
    <form ref={formRef} onSubmit={submitHandler} className="space-y-4">
      {pending && <Loading />}
      <EntityFormTabs
        hasId
        tabs={[
          {
            key: "pairing",
            label: "Pairing",
            Icon: Handshake,
            keepMounted: true,
            content: (
              <div className="grid min-h-[15rem] grid-cols-1 gap-4 sm:grid-cols-2">
                {/* Server-search picker — the employee table is never bulk-loaded (10k+ scale). */}
                <div>
                  <label className="mb-1 block text-xs font-medium text-muted">Mentor *</label>
                  <EmployeePicker
                    value={formData.mentorEmployeeId}
                    displayValue={formData.mentorName}
                    onSelect={(eid, name) => setFormData((p) => ({ ...p, mentorEmployeeId: eid, mentorName: name }))}
                  />
                  {formState?.zodErrors?.mentorEmployeeId && (
                    <p className="mt-1 text-xs text-error">{formState.zodErrors.mentorEmployeeId[0]}</p>
                  )}
                </div>
                <div>
                  <label className="mb-1 block text-xs font-medium text-muted">Mentee *</label>
                  <EmployeePicker
                    value={formData.menteeEmployeeId}
                    displayValue={formData.menteeName}
                    onSelect={(eid, name) => setFormData((p) => ({ ...p, menteeEmployeeId: eid, menteeName: name }))}
                  />
                  {formState?.zodErrors?.menteeEmployeeId && (
                    <p className="mt-1 text-xs text-error">{formState.zodErrors.menteeEmployeeId[0]}</p>
                  )}
                </div>
                <FormUtility component={{ name: "context", label: "Context", required: true, type: "dropDown", layout: "auth", value: formData.context ?? "General", displayValue: label(mentorshipContextOptions, formData.context ?? "General"), data: mentorshipContextOptions as never, onSelect: selectHandler }} />
                <FormUtility component={{ name: "status", label: "Status", required: true, type: "dropDown", layout: "auth", value: formData.status ?? "Active", displayValue: label(mentorshipStatusOptions, formData.status ?? "Active"), data: mentorshipStatusOptions as never, onSelect: selectHandler }} />
              </div>
            ),
          },
          {
            key: "schedule",
            label: "Schedule & Notes",
            Icon: CalendarClock,
            keepMounted: true,
            content: (
              <div className="grid min-h-[15rem] grid-cols-1 gap-4 sm:grid-cols-2">
                <FormUtility component={{ name: "startDate", label: "Start Date", type: "date", layout: "auth", value: formData.startDate?.slice(0, 10), onChange: changeHandler }} />
                <FormUtility component={{ name: "endDate", label: "End Date", type: "date", layout: "auth", value: formData.endDate?.slice(0, 10), onChange: changeHandler }} />
                <div className="sm:col-span-2">
                  <FormUtility component={{ name: "notes", label: "Notes", type: "textarea", layout: "auth", value: formData.notes, onChange: changeHandler }} />
                </div>
              </div>
            ),
          },
        ]}
      />

      <input type="hidden" name="id" value={formData.id ?? ""} readOnly />
      {/* EmployeePicker holds no named input — these carry the ids into the FormData submit. */}
      <input type="hidden" name="mentorEmployeeId" value={formData.mentorEmployeeId ?? ""} readOnly />
      <input type="hidden" name="menteeEmployeeId" value={formData.menteeEmployeeId ?? ""} readOnly />

      <div className="flex items-center justify-end border-t border-border pt-3">
        <button type="submit" disabled={isLoading} className="rounded-lg bg-primary px-5 py-2 text-sm font-semibold text-on-accent transition-colors hover:bg-primary-hover disabled:opacity-50">{isLoading ? "Saving…" : "Save Mentorship"}</button>
      </div>
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </form>
  );
}

export default memo(MentorshipForm);
