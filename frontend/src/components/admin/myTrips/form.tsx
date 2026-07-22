"use client";
import { memo, useCallback, useState } from "react";
import React from "react";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import FormProviders from "@/components/common/formProvider/formProvider";
import { StatusMessage } from "../../common/statusMessage/status";
import { requestTrip } from "@/services/admin/trip";

const FormProvider = memo(FormProviders);
const TYPE_OPTIONS = [{ id: "Local", name: "Local" }, { id: "International", name: "International" }];
const iso = (d: Date) => d.toISOString().slice(0, 10);
const NEW_DEFAULTS = { tripType: "Local", destination: "", purpose: "", startDate: iso(new Date()), endDate: iso(new Date()), advanceAmount: "" };

/** HC260 — the signed-in employee's business-trip request form. */
function MyTripRequestForm({ onDone }: { onDone: () => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const formRef = React.createRef<HTMLFormElement>();

  const [formState, setFormState] = useState<any>({});
  const [form, setForm] = useState({ ...NEW_DEFAULTS });
  const [busy, setBusy] = useState(false);

  const changeHandler = useCallback((e: any) => setForm((p) => ({ ...p, [e.target.name]: e.target.value })), []);
  const selectHandler = useCallback((name: string, r: any) => setForm((p) => ({ ...p, [name]: r.id })), []);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    if (!form.destination.trim() || !form.startDate || !form.endDate) {
      setFormState({ status: "error", message: t("Please fill the destination and dates."), zodErrors: {} });
      return;
    }
    setBusy(true);
    const res = await requestTrip({
      tripType: form.tripType,
      destination: form.destination.trim(),
      purpose: form.purpose.trim() || undefined,
      startDate: form.startDate,
      endDate: form.endDate,
      advanceAmount: form.advanceAmount !== "" ? Number(form.advanceAmount) : undefined,
    });
    setBusy(false);
    setFormState({ status: res.ok ? "success" : "error", message: res.message, zodErrors: {} });
    if (res.ok) {
      queryClient.invalidateQueries({ queryKey: ["myTrips"] });
      onDone();
    }
  };

  return (
    <div className="text-white">
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[35%]",
          isPending: busy,
          SubmitButton: "top",
          submitBtnTitle: "Submit request",
          components: [
            { name: "tripType", label: "Trip Type", type: "dropDown", onSelect: selectHandler, value: form.tripType, displayValue: t(form.tripType), data: TYPE_OPTIONS as never },
            { name: "destination", label: "Destination", required: true, value: form.destination, onChange: changeHandler, type: "text" },
            { name: "startDate", label: "Start Date", required: true, type: "date", value: form.startDate, onChange: changeHandler },
            { name: "endDate", label: "End Date", required: true, type: "date", value: form.endDate, onChange: changeHandler },
            { name: "advanceAmount", label: "Advance (blank = per-diem)", value: form.advanceAmount, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "purpose", label: "Purpose", value: form.purpose, onChange: changeHandler, type: "text", colSpan: "full" },
            { name: "perDiemNote", type: "custom", colSpan: "full", customChildren: <p className="text-xs text-muted">{t("The per-diem is computed from your job grade and the trip type.")}</p> },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default MyTripRequestForm;
