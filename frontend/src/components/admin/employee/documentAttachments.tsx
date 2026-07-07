"use client";
import { useCallback, useRef, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Paperclip, Upload, Download, Trash2, FileText } from "lucide-react";
import { useTranslation } from "react-i18next";
import Loading from "../../common/loader/loader";
import {
  getDocuments,
  uploadDocument,
  deleteDocument,
  downloadDocument,
  type DocumentOwnerType,
} from "@/services/admin/employee/documents";
import type { EmployeeDocumentModel } from "@/models";

interface Props {
  employeeId: string;
  ownerType: DocumentOwnerType;
  ownerId: string;
}

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

/** Manage the files attached to one education/experience record (HC017/HC018). */
function DocumentAttachments({ employeeId, ownerType, ownerId }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const queryKey = ["employeeDocuments", ownerType, ownerId];
  const { data: docs, isLoading } = useQuery({
    queryKey,
    queryFn: () => getDocuments(ownerType, ownerId),
    enabled: !!ownerId,
  });

  const refreshParentCount = useCallback(() => {
    queryClient.invalidateQueries({
      queryKey: [ownerType === "Education" ? "employeeEducations" : "employeeExperiences", employeeId],
    });
  }, [queryClient, ownerType, employeeId]);

  const onPick = useCallback(
    async (e: React.ChangeEvent<HTMLInputElement>) => {
      const files = Array.from(e.target.files ?? []);
      e.target.value = "";
      if (!files.length) return;
      setBusy(true);
      setError(null);
      for (const file of files) {
        const res = await uploadDocument(employeeId, ownerType, ownerId, file);
        if (!res.ok) {
          setError(res.message);
          break;
        }
      }
      setBusy(false);
      queryClient.invalidateQueries({ queryKey });
      refreshParentCount();
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [employeeId, ownerType, ownerId, queryClient, refreshParentCount],
  );

  const onDelete = useCallback(
    async (id: string) => {
      setBusy(true);
      await deleteDocument(id);
      setBusy(false);
      queryClient.invalidateQueries({ queryKey });
      refreshParentCount();
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [queryClient, refreshParentCount],
  );

  return (
    <div className="mt-3 rounded-lg border border-border bg-card p-3">
      <div className="mb-2 flex items-center justify-between gap-2">
        <h4 className="flex items-center gap-1.5 text-sm font-semibold text-foreground">
          <Paperclip size={15} /> {t("Attachments")}
        </h4>
        <input
          ref={inputRef}
          type="file"
          multiple
          className="hidden"
          accept=".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.txt,.csv,.jpg,.jpeg,.png,.webp,.gif"
          onChange={onPick}
        />
        <button
          type="button"
          disabled={busy}
          onClick={() => inputRef.current?.click()}
          className="inline-flex items-center gap-1.5 rounded-md border border-primary/40 bg-primary/10 px-3 py-1.5 text-xs font-medium text-primary hover:bg-primary/20 disabled:opacity-50"
        >
          <Upload size={14} /> {t("Upload")}
        </button>
      </div>

      {error && <p className="mb-2 text-xs text-error">{error}</p>}
      {isLoading && <Loading />}

      {!isLoading && (docs?.length ?? 0) === 0 && (
        <p className="py-3 text-center text-xs text-muted">
          {t("No records yet. Use the add button above.")}
        </p>
      )}

      <ul className="space-y-1">
        {(docs ?? []).map((doc: EmployeeDocumentModel) => (
          <li
            key={doc.id}
            className="flex items-center gap-2 rounded-md border border-border/60 px-2.5 py-1.5 text-sm"
          >
            <FileText size={15} className="shrink-0 text-muted" />
            <span className="min-w-0 flex-1 truncate text-foreground" title={doc.fileName}>
              {doc.fileName}
            </span>
            <span className="shrink-0 text-xs text-muted">{formatSize(doc.fileSize)}</span>
            <button
              type="button"
              title={t("Download")}
              onClick={() => downloadDocument(doc.id, doc.fileName)}
              className="shrink-0 rounded p-1 text-primary hover:bg-primary/10"
            >
              <Download size={15} />
            </button>
            <button
              type="button"
              title={t("Delete")}
              disabled={busy}
              onClick={() => onDelete(doc.id)}
              className="shrink-0 rounded p-1 text-error hover:bg-error/10 disabled:opacity-50"
            >
              <Trash2 size={15} />
            </button>
          </li>
        ))}
      </ul>
    </div>
  );
}

export default DocumentAttachments;
