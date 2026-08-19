"use client";
import { memo, useCallback, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Mail, DatabaseBackup, KeyRound } from "lucide-react";
import FormProviders from "@/components/common/formProvider/formProvider";
import Loading from "@/components/common/loader/loader";
import DetailSection from "@/components/common/detailSection";
import { StatusMessage } from "@/components/common/statusMessage/status";
import type { SettingModel, TestEmailResultModel } from "@/models";
import { getSetting, saveSetting, sendTestEmail } from "@/services/admin/setting";

const FormProvider = memo(FormProviders);

/**
 * These MUST mirror the backend's "no row saved yet" fallbacks (GetSetting), so an unconfigured
 * screen shows what the deployment is actually doing rather than a blank slate.
 */
const DEFAULTS: SettingModel = {
  smtpHost: "",
  smtpPort: 587,
  smtpUser: "",
  smtpUseTls: true,
  autoBackup: false,
  backupFrequency: "daily",
  backupRetentionDays: 30,
};

const FREQUENCY_OPTIONS = [
  { id: "daily", name: "Daily" },
  { id: "weekly", name: "Weekly" },
  { id: "monthly", name: "Monthly" },
];

const NOTES = [
  {
    icon: Mail,
    title: "Which relay is shown",
    body: "The host, port and user reported here are the ones the server will ACTUALLY use — the "
      + "saved row where there is one, otherwise the Email configuration section. Saving makes the "
      + "stored values take precedence, so a blank host does not mean no mail is being sent.",
  },
  {
    icon: KeyRound,
    title: "Where the password lives",
    body: "The SMTP password is never part of this screen, in either direction. It is deployment "
      + "configuration (user-secrets locally, an environment variable elsewhere) and is never sent "
      + "to a browser; this screen can only report whether one is set.",
  },
  {
    icon: DatabaseBackup,
    title: "Backups",
    body: "The schedule and retention window recorded here are what the backup job reads. "
      + "Retention is counted in days and applies only while automatic backup is on.",
  },
];

/**
 * The deployment's single settings row. There is no id in the URL and no list: the endpoint upserts
 * the one row, so this form always edits "the" settings.
 */
function SettingForm() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const formRef = useRef<HTMLFormElement>(null);
  const testFormRef = useRef<HTMLFormElement>(null);

  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);
  const [formData, setFormData] = useState<SettingModel>({ ...DEFAULTS });

  const [testTo, setTestTo] = useState("");
  const [isTesting, setIsTesting] = useState(false);
  const [testResult, setTestResult] = useState<TestEmailResultModel | null>(null);

  const { data: setting, isLoading } = useQuery({
    queryKey: ["setting"],
    queryFn: getSetting,
  });

  useEffect(() => {
    if (setting) setFormData({ ...DEFAULTS, ...setting });
  }, [setting]);

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);

  const toggle = useCallback(
    (name: keyof SettingModel) => (e: any) => {
      setFormData((p) => ({ ...p, [name]: e.target.checked }));
    },
    [],
  );

  const submitHandler = async (e: any) => {
    e.preventDefault();
    setIsSaving(true);
    const res = await saveSetting(formData);
    setIsSaving(false);
    setFormState(
      res.ok
        ? { status: "success", message: res.message }
        : { status: "error", message: res.message },
    );
    // Re-read rather than trust the local copy: the server reports the RESOLVED relay, which can
    // differ from what was just typed (configuration still supplies anything left blank).
    if (res.ok) queryClient.invalidateQueries({ queryKey: ["setting"] });
  };

  const testHandler = async (e: any) => {
    e.preventDefault();
    setIsTesting(true);
    setTestResult(null);
    setTestResult(await sendTestEmail(testTo));
    setIsTesting(false);
  };

  if (isLoading) return <Loading />;

  const emailDisabled = setting?.emailEnabled === false;
  const missingPassword =
    !emailDisabled && !!formData.smtpUser?.trim() && setting?.hasSmtpPassword === false;
  const noHost = !setting?.smtpHost?.trim();

  return (
    <div>
      <FormProvider
        ref={formRef}
        form={{
          // ⚠️ A unique id, NOT the default: two FormProviders are mounted on this screen and each
          // submit button targets its form by id, so sharing the default id makes Save submit the
          // OTHER form.
          formId: "settingForm",
          columnsNo: 2,
          labelWidth: "w-[40%]",
          submitHandler,
          isPending: isSaving,
          SubmitButton: "top",
          submitBtnTitle: "Save",
          components: [
            {
              name: "smtpBreak", label: "Outbound e-mail (SMTP)", type: "break", colSpan: "full",
              sectionDescription:
                "The relay every notification and alert this system sends is delivered through.",
            },
            {
              name: "smtpHost", label: "SMTP host", type: "text",
              placeholder: "e.g. smtp.office365.com",
              value: formData.smtpHost, onChange: changeHandler,
              error: formState?.zodErrors?.smtpHost,
            },
            {
              name: "smtpPort", label: "Port", type: "text", inputType: "number",
              placeholder: "587",
              value: formData.smtpPort, onChange: changeHandler,
              error: formState?.zodErrors?.smtpPort,
            },
            {
              name: "smtpUser", label: "User name", type: "text",
              placeholder: "e.g. hr-notifications@company.com",
              value: formData.smtpUser, onChange: changeHandler,
              error: formState?.zodErrors?.smtpUser,
            },
            {
              name: "smtpUseTls", label: "Use TLS", type: "checkbox",
              value: formData.smtpUseTls ? "true" : "",
              onChange: toggle("smtpUseTls"),
            },
            {
              name: "backupBreak", label: "Database backup", type: "break", colSpan: "full",
              sectionDescription:
                "How often the database is backed up, and how long copies are kept.",
            },
            {
              name: "autoBackup", label: "Automatic backup", type: "checkbox",
              value: formData.autoBackup ? "true" : "",
              onChange: toggle("autoBackup"),
            },
            {
              name: "backupFrequency", label: "Frequency", type: "select",
              value: formData.backupFrequency, onChange: changeHandler,
              disabled: !formData.autoBackup,
              data: FREQUENCY_OPTIONS as never,
            },
            {
              name: "backupRetentionDays", label: "Keep backups for (days)",
              type: "text", inputType: "number", placeholder: "e.g. 30",
              value: formData.backupRetentionDays, onChange: changeHandler,
              disabled: !formData.autoBackup,
              error: formState?.zodErrors?.backupRetentionDays,
            },
          ],
        }}
      />

      {emailDisabled && (
        <p className="mt-2 rounded-md border border-warning/20 bg-warning/15 px-3 py-2 text-xs text-warning">
          {t("E-mail is switched off for this deployment (Email:Enabled is false), so nothing is sent no matter what is saved here. That switch is configuration, not a setting on this screen.")}
        </p>
      )}

      {missingPassword && (
        <p className="mt-2 rounded-md border border-warning/20 bg-warning/15 px-3 py-2 text-xs text-warning">
          {t("An SMTP user is set but no password is configured (Email:Password, from user-secrets or an environment variable), so the relay will reject every message. The password cannot be set from this screen.")}
        </p>
      )}

      {noHost && (
        <p className="mt-2 rounded-md border border-info/20 bg-info/15 px-3 py-2 text-xs text-muted">
          {t("No SMTP host is configured here or in the Email configuration section, so outbound mail cannot be delivered.")}
        </p>
      )}

      <div className="mt-3">
        <DetailSection title="Send a test message">
          <p className="mb-2 text-xs leading-relaxed text-muted">
            {t("Queues one message so you can confirm the relay works, and reports which host and user were actually used. Save first — the test uses the SAVED settings, not what is currently on screen.")}
          </p>
          <FormProvider
            ref={testFormRef}
            form={{
              // Distinct from "settingForm" above — see the note there.
              formId: "settingTestEmailForm",
              columnsNo: 2,
              frameless: true,
              labelWidth: "w-[40%]",
              submitHandler: testHandler,
              isPending: isTesting,
              SubmitButton: "bottom",
              submitBtnTitle: "Send test message",
              components: [
                {
                  name: "testTo", label: "Send to", type: "text", inputType: "email",
                  placeholder: "you@company.com",
                  value: testTo,
                  onChange: (e: any) => setTestTo(e.target.value),
                },
              ],
            }}
          />
          {testResult && (
            <div
              className={
                "mt-2 rounded-md border px-3 py-2 text-xs " +
                (testResult.queued
                  ? "border-success/20 bg-success/15 text-success"
                  : "border-warning/20 bg-warning/15 text-warning")
              }
            >
              <p>{t(testResult.message)}</p>
              {testResult.resolvedHost && (
                <p className="mt-1 text-muted">
                  {t("Relay")}: {testResult.resolvedHost}
                  {testResult.resolvedUser ? " — " + testResult.resolvedUser : ""}
                </p>
              )}
            </div>
          )}
        </DetailSection>
      </div>

      <div className="mt-3">
        <DetailSection title="About these settings">
          <div className="grid gap-3 sm:grid-cols-3">
            {NOTES.map(({ icon: Icon, title, body }) => (
              <div key={title} className="rounded-md border border-border bg-secondary p-3">
                <p className="flex items-center gap-2 text-sm font-semibold text-foreground">
                  <Icon size={15} className="shrink-0 text-primary" />
                  {t(title)}
                </p>
                <p className="mt-1 text-xs leading-relaxed text-muted">{t(body)}</p>
              </div>
            ))}
          </div>
        </DetailSection>
      </div>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default SettingForm;
