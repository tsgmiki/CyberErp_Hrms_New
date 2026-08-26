"use client";
import React, { memo, useCallback, useEffect, useMemo, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Plus, X, Users, AtSign } from "lucide-react";
import FormProviders from "@/components/common/formProvider/formProvider";
import { StatusMessage } from "../../common/statusMessage/status";
import DetailSection from "@/components/common/detailSection";
import {
  getNotificationTemplate,
  saveNotificationTemplate,
  getNotificationEvents,
} from "@/services/admin/notificationTemplate";
import getAllRole from "@/services/admin/role/getAll";
import getAllOrganizationUnit from "@/services/admin/organizationUnit/getAll";
import type {
  NotificationTemplateModel,
  NotificationRecipientModel,
} from "@/models";
import { parameterInitialData } from "@/constants/initialization";

const FormProvider = memo(FormProviders);

const CHANNELS = [
  { id: "Email", name: "E-mail" },
  { id: "Portal", name: "Portal alert" },
  { id: "Both", name: "E-mail and portal" },
];

const DELIVERY = [
  { id: "To", name: "To" },
  { id: "Cc", name: "Cc" },
  { id: "Bcc", name: "Bcc" },
];

/**
 * Every recipient rule the dispatcher understands. `needs` says what the rule must be given before
 * it can resolve — the row renders that input and nothing else, so an admin cannot save a Role rule
 * with no role or an Address rule with no address.
 */
const RECIPIENT_KINDS: {
  id: string;
  name: string;
  hint: string;
  needs?: "role" | "unit" | "address";
}[] = [
  { id: "Requester", name: "The requester", hint: "Whoever the record is about" },
  { id: "CurrentApprover", name: "Current approver", hint: "Whoever is deciding this step" },
  { id: "RequesterManager", name: "Requester's manager", hint: "From the org structure" },
  { id: "Role", name: "Everyone in a role", hint: "e.g. HR Admin", needs: "role" },
  { id: "OrganizationUnit", name: "Everyone in a unit", hint: "By position", needs: "unit" },
  { id: "AllEmployees", name: "All employees", hint: "Every active employee with an address" },
  { id: "Address", name: "A specific address", hint: "Distribution list or external party", needs: "address" },
  // Recruitment events are addressed to a CANDIDATE, who is not an employee and cannot be resolved
  // from org data. Without this rule a template would quietly cut them out of their own message.
  { id: "EventSubject", name: "Who the event is about", hint: "e.g. the candidate — set by the event itself" },
];

const BLANK: NotificationTemplateModel = {
  name: "",
  subject: "",
  body: "",
  channel: "Email",
  isActive: true,
  recipients: [],
};

interface Props {
  id: string;
  setId: (id: string) => void;
}

function NotificationTemplateForm({ id, setId }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const formRef = React.createRef<HTMLFormElement>();

  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);
  const [formData, setFormData] = useState<NotificationTemplateModel>({ ...BLANK });
  const [recipients, setRecipients] = useState<NotificationRecipientModel[]>([]);

  const { data: events } = useQuery({
    queryKey: ["notificationEvents"],
    queryFn: getNotificationEvents,
    staleTime: 5 * 60 * 1000,
  });

  const { data: roles } = useQuery({
    queryKey: ["rolesForRecipients"],
    queryFn: () => getAllRole({ ...parameterInitialData, take: 200 }),
    staleTime: 5 * 60 * 1000,
  });

  const { data: units } = useQuery({
    queryKey: ["unitsForRecipients"],
    queryFn: () => getAllOrganizationUnit({ ...parameterInitialData, take: 500 }),
    staleTime: 5 * 60 * 1000,
  });

  const { data: record } = useQuery({
    queryKey: ["notificationTemplate", id],
    queryFn: () => getNotificationTemplate(id),
    enabled: id !== "",
  });

  useEffect(() => {
    if (record) {
      setFormData({ ...BLANK, ...record });
      setRecipients(record.recipients ?? []);
    } else if (id === "") {
      setFormData({ ...BLANK });
      setRecipients([]);
    }
  }, [record, id]);

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);

  const bodyChange = useCallback(
    (html: string) => setFormData((p) => ({ ...p, body: html })),
    [],
  );

  const selectedEvent = useMemo(
    () => (events ?? []).find((e) => e.id === formData.notificationEventId),
    [events, formData.notificationEventId],
  );

  /**
   * Inserts a token at the end of the subject. Clicking beats typing here: a token the event does
   * not publish merges to blank at send time, and that failure is invisible until a real message
   * goes out thin.
   */
  const appendToken = (token: string) =>
    setFormData((p) => ({ ...p, subject: `${p.subject ?? ""}{{${token}}}` }));

  const addRecipient = () =>
    setRecipients((r) => [...r, { kind: "Requester", delivery: "To", isActive: true }]);

  const updateRecipient = (index: number, patch: Partial<NotificationRecipientModel>) =>
    setRecipients((r) => r.map((row, i) => (i === index ? { ...row, ...patch } : row)));

  const removeRecipient = (index: number) =>
    setRecipients((r) => r.filter((_, i) => i !== index));

  const submitHandler = async (e: any) => {
    e.preventDefault();
    setIsSaving(true);
    const chosen = (events ?? []).find((x) => x.id === formData.notificationEventId);
    const res = await saveNotificationTemplate({
      ...formData,
      id: id || undefined,
      eventKey: chosen?.eventKey,
      stepOrder: formData.stepOrder ? Number(formData.stepOrder) : null,
      recipients,
    });
    setIsSaving(false);
    setFormState(res.ok ? { status: "success", message: res.message } : { status: "error", message: res.message });
    if (res.ok) {
      queryClient.invalidateQueries({ queryKey: ["notificationTemplates"] });
      setId("");
    }
  };

  const roleOptions = (roles?.data ?? []).map((r: any) => ({ id: r.id, name: r.name }));
  const unitOptions = (units?.data ?? []).map((u: any) => ({ id: u.id, name: u.name }));

  return (
    <div>
      <FormProvider
        ref={formRef}
        form={{
          formId: "notificationTemplateForm",
          columnsNo: 2,
          submitHandler,
          isPending: isSaving,
          SubmitButton: "top",
          components: [
            {
              name: "eventBreak", label: "What this responds to", type: "break", colSpan: "full",
              sectionDescription:
                "Pick the moment that triggers the message. The tokens it offers appear below the subject.",
            },
            {
              name: "notificationEventId", label: "Event", type: "select", required: true,
              value: formData.notificationEventId, onChange: changeHandler,
              data: [{ id: "", name: t("Select an event") },
                ...(events ?? []).map((e) => ({ id: e.id, name: `${e.category} · ${e.name}` }))] as never,
              error: formState?.zodErrors?.notificationEventId,
            },
            {
              name: "name", label: "Template name", type: "text", required: true,
              placeholder: "e.g. Leave approved — notify requester",
              value: formData.name, onChange: changeHandler,
              error: formState?.zodErrors?.name,
            },
            {
              name: "messageBreak", label: "Message", type: "break", colSpan: "full",
              sectionDescription: "Subject and body. Use {{Token}} anywhere in either.",
            },
            {
              name: "subject", label: "Subject", type: "text", required: true, colSpan: "full",
              placeholder: "e.g. {{LeaveType}} approved for {{EmployeeName}}",
              value: formData.subject, onChange: changeHandler,
              error: formState?.zodErrors?.subject,
            },
            {
              name: "body", label: "", type: "editor", colSpan: "full",
              // The rich-text field reports through onHtmlChange, not onChange — it hands back the
              // HTML string rather than a DOM event.
              value: formData.body ?? "", onHtmlChange: bodyChange,
              error: formState?.zodErrors?.body,
            },
            {
              name: "scopeBreak", label: "Where it applies", type: "break", colSpan: "full",
              sectionDescription:
                "Leave blank to use this template everywhere the event fires. Narrow it to one workflow step to word that step differently — the most specific template wins.",
            },
            {
              name: "stepOrder", label: "Workflow step (optional)", type: "text", inputType: "number",
              placeholder: "e.g. 2",
              value: formData.stepOrder ?? "", onChange: changeHandler,
            },
            {
              name: "channel", label: "Channel", type: "select",
              value: formData.channel, onChange: changeHandler, data: CHANNELS as never,
            },
            {
              name: "isActive", label: "Active", type: "checkbox",
              value: formData.isActive ? "true" : "",
              onChange: (e: any) => setFormData((p) => ({ ...p, isActive: e.target.checked })),
            },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />

      {/* Token palette — what this event actually offers */}
      {selectedEvent && selectedEvent.tokens.length > 0 && (
        <div className="mt-3">
          <DetailSection title="Available tokens">
            <p className="mb-2 text-xs text-muted">
              {t("Click to add to the subject, or type it into the body. A token this event does not publish merges to blank.")}
            </p>
            <div className="flex flex-wrap gap-1.5">
              {selectedEvent.tokens.map((token) => (
                <button
                  key={token}
                  type="button"
                  onClick={() => appendToken(token)}
                  className="rounded-full border border-border bg-secondary px-2.5 py-1 text-[11px] font-medium text-foreground transition-colors transition-opacity hover:opacity-80"
                >
                  {`{{${token}}}`}
                </button>
              ))}
            </div>
          </DetailSection>
        </div>
      )}

      {/* Recipient rules — the "who receives it" half of the feature */}
      <div className="mt-3">
        <DetailSection title="Who receives it">
          <p className="mb-2 text-xs text-muted">
            {t("Rules resolve when the message is sent, so they keep working as people change role, manager or team. Add as many as you need.")}
          </p>

          <div className="space-y-2">
            {recipients.map((rule, index) => {
              const kind = RECIPIENT_KINDS.find((k) => k.id === rule.kind);
              return (
                <div
                  key={index}
                  className="flex flex-wrap items-center gap-2 rounded-lg border border-border bg-card p-2"
                >
                  <Users className="h-4 w-4 shrink-0 text-primary" />

                  <select
                    className="h-9 min-w-[190px] rounded-md border border-border bg-card px-2 text-[13px] text-foreground"
                    value={rule.kind}
                    onChange={(e) =>
                      updateRecipient(index, { kind: e.target.value, targetId: null, address: null })
                    }
                  >
                    {RECIPIENT_KINDS.map((k) => (
                      <option key={k.id} value={k.id}>{t(k.name)}</option>
                    ))}
                  </select>

                  {kind?.needs === "role" && (
                    <select
                      className="h-9 min-w-[170px] rounded-md border border-border bg-card px-2 text-[13px] text-foreground"
                      value={rule.targetId ?? ""}
                      onChange={(e) => updateRecipient(index, { targetId: e.target.value })}
                    >
                      <option value="">{t("Select a role")}</option>
                      {roleOptions.map((r) => (
                        <option key={r.id} value={r.id}>{r.name}</option>
                      ))}
                    </select>
                  )}

                  {kind?.needs === "unit" && (
                    <select
                      className="h-9 min-w-[170px] rounded-md border border-border bg-card px-2 text-[13px] text-foreground"
                      value={rule.targetId ?? ""}
                      onChange={(e) => updateRecipient(index, { targetId: e.target.value })}
                    >
                      <option value="">{t("Select a unit")}</option>
                      {unitOptions.map((u) => (
                        <option key={u.id} value={u.id}>{u.name}</option>
                      ))}
                    </select>
                  )}

                  {kind?.needs === "address" && (
                    <span className="inline-flex items-center gap-1">
                      <AtSign className="h-3.5 w-3.5 text-muted" />
                      <input
                        type="email"
                        className="h-9 min-w-[220px] rounded-md border border-border bg-card px-2 text-[13px] text-foreground"
                        placeholder="hr@company.com"
                        value={rule.address ?? ""}
                        onChange={(e) => updateRecipient(index, { address: e.target.value })}
                      />
                    </span>
                  )}

                  <select
                    className="h-9 w-[90px] rounded-md border border-border bg-card px-2 text-[13px] text-foreground"
                    value={rule.delivery}
                    onChange={(e) => updateRecipient(index, { delivery: e.target.value })}
                  >
                    {DELIVERY.map((d) => (
                      <option key={d.id} value={d.id}>{d.name}</option>
                    ))}
                  </select>

                  <span className="min-w-0 flex-1 truncate text-[11px] text-muted">
                    {kind ? t(kind.hint) : ""}
                  </span>

                  <button
                    type="button"
                    onClick={() => removeRecipient(index)}
                    className="rounded-md p-1.5 text-error transition-opacity hover:opacity-70"
                    aria-label={t("Remove")}
                  >
                    <X className="h-4 w-4" />
                  </button>
                </div>
              );
            })}
          </div>

          <button
            type="button"
            onClick={addRecipient}
            className="mt-2 inline-flex items-center gap-1.5 rounded-md border border-primary bg-primary/10 px-3 py-1.5 text-xs font-medium text-primary transition-opacity hover:opacity-80"
          >
            <Plus size={14} /> {t("Add recipient")}
          </button>

          {recipients.length === 0 && (
            <p className="mt-2 rounded-md border border-warning/20 bg-warning/15 px-3 py-2 text-xs text-warning">
              {t("With no recipient rule this template will never send. Add at least one.")}
            </p>
          )}
        </DetailSection>
      </div>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default NotificationTemplateForm;
