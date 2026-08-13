"use client";

import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { Paperclip, Download, X, FileText } from "lucide-react";
import type { OtherLeaveAttachmentModel } from "@/models";
import { downloadOtherLeaveAttachment } from "@/services/admin/otherLeave";

/** 5 MB per file — the server rejects anything larger, so say so before the upload is attempted. */
export const MAX_ATTACHMENT_BYTES = 5 * 1024 * 1024;

const prettySize = (bytes?: number) => {
  if (!bytes) return "";
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
};

/**
 * File picker for a NEW request: choose supporting documents (medical certificate, death
 * certificate…) and see what is staged before submitting.
 *
 * <p>Oversized files are rejected HERE, with the offending name, rather than being uploaded and
 * bounced by the server — a 6 MB scan that fails after the upload wastes the wait and reports the
 * problem too late to be obviously about that one file.</p>
 */
export const AttachmentPicker = memo(function AttachmentPicker({
  files, setFiles, error, setError,
}: {
  files: File[];
  setFiles: (f: File[]) => void;
  error: string | null;
  setError: (e: string | null) => void;
}) {
  const { t } = useTranslation();

  const pick = (e: React.ChangeEvent<HTMLInputElement>) => {
    const picked = Array.from(e.target.files ?? []);
    const tooBig = picked.find((f) => f.size > MAX_ATTACHMENT_BYTES);
    if (tooBig) {
      setError(`${tooBig.name} — ${t("exceeds the 5 MB limit.")}`);
      e.target.value = "";
      return;
    }
    setError(null);
    setFiles([...files, ...picked]);
    // Reset so re-picking the SAME file still fires change (browsers suppress an identical value).
    e.target.value = "";
  };

  return (
    <div>
      <label className="mb-1 block text-xs font-medium text-muted">
        {t("Supporting document")}{" "}
        <span className="font-normal">{t("(optional — max 5 MB per file)")}</span>
      </label>
      <input
        type="file"
        multiple
        onChange={pick}
        className="block w-full cursor-pointer rounded-md border border-border bg-card text-sm text-foreground file:mr-3 file:cursor-pointer file:rounded-l-md file:border-0 file:bg-secondary file:px-3 file:py-1.5 file:text-xs file:font-semibold file:text-foreground"
      />
      {error && <p className="mt-1 text-xs text-error">{error}</p>}

      {files.length > 0 && (
        <ul className="mt-2 space-y-1">
          {files.map((f, i) => (
            <li
              key={`${f.name}-${i}`}
              className="flex items-center gap-2 rounded-md border border-border bg-secondary/20 px-2.5 py-1.5 text-xs"
            >
              <FileText size={13} className="shrink-0 text-primary" />
              <span className="min-w-0 flex-1 truncate text-foreground">{f.name}</span>
              <span className="shrink-0 tabular-nums text-muted">{prettySize(f.size)}</span>
              <button
                type="button"
                title={t("Remove") ?? undefined}
                onClick={() => setFiles(files.filter((_, idx) => idx !== i))}
                className="shrink-0 text-muted hover:text-error"
              ><X size={13} /></button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
});

/**
 * The documents already attached to a request, with a download for each.
 *
 * <p>Rendered wherever the request is READ — the requester's own view, the approver's review popup,
 * and the HR admin's Other Leave tab on the employee profile — so "the evidence" is never a
 * different screen from "the request". The bytes are fetched only when a row is clicked; the list
 * itself carries metadata only.</p>
 */
export const AttachmentList = memo(function AttachmentList({
  attachments, emptyHint = true,
}: {
  attachments?: OtherLeaveAttachmentModel[];
  emptyHint?: boolean;
}) {
  const { t } = useTranslation();
  const [busy, setBusy] = useState<string | null>(null);
  const [failed, setFailed] = useState<string | null>(null);
  const rows = attachments ?? [];

  const get = async (a: OtherLeaveAttachmentModel) => {
    if (!a.id || busy) return;
    setBusy(a.id);
    setFailed(null);
    const ok = await downloadOtherLeaveAttachment(a.id, a.fileName || "attachment");
    if (!ok) setFailed(t("The document could not be downloaded.") as string);
    setBusy(null);
  };

  if (rows.length === 0) {
    return emptyHint ? (
      <p className="text-xs text-muted">{t("No supporting document was attached.")}</p>
    ) : null;
  }

  return (
    <div className="space-y-1">
      {rows.map((a) => (
        <button
          key={a.id}
          type="button"
          onClick={() => get(a)}
          disabled={busy === a.id}
          className="flex w-full items-center gap-2 rounded-md border border-border bg-card px-2.5 py-1.5 text-left text-xs transition-colors hover:border-primary/50 disabled:opacity-50"
        >
          <Paperclip size={13} className="shrink-0 text-primary" />
          <span className="min-w-0 flex-1 truncate text-foreground">{a.fileName}</span>
          {a.fileSize ? (
            <span className="shrink-0 tabular-nums text-muted">{prettySize(a.fileSize)}</span>
          ) : null}
          <Download size={13} className="shrink-0 text-muted" />
        </button>
      ))}
      {failed && <p className="text-xs text-error">{failed}</p>}
    </div>
  );
});
