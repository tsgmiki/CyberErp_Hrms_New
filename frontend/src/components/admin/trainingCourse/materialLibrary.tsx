"use client";
import { memo, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { FolderOpen, Upload, Trash2, Download, Info } from "lucide-react";
import {
  getCourseFiles, uploadCourseFile, deleteCourseFile, previewCourseFile,
} from "@/services/admin/courseFile";
import type { CourseFileModel } from "@/models";
import { toast } from "@/components/common/toast";
import { confirm } from "@/components/common/dialog";
import Loading from "@/components/common/loader/loader";

const MAX_MB = 25;

const size = (bytes: number) =>
  bytes >= 1024 * 1024
    ? `${(bytes / 1024 / 1024).toFixed(1)} MB`
    : `${Math.max(1, Math.round(bytes / 1024))} KB`;

const day = (v?: string | null) =>
  v ? new Date(v).toLocaleDateString(undefined, { day: "2-digit", month: "short", year: "numeric" }) : "";

/**
 * The course's material library — the files a Document module serves.
 *
 * <p>Owned by the COURSE rather than by a version, so a revision that keeps the same handbook points
 * at the same file instead of a copy of it. Files are immutable once uploaded: that is what lets a
 * published version stay frozen, and it means "replace this PDF" is "upload the new one and point
 * the draft at it".</p>
 *
 * <p>Video is deliberately not accepted here. A Video module is a URL because streaming media out of
 * SQL Server does not scale, and allowing an upload would undo that one file at a time.</p>
 */
function MaterialLibrary({ trainingCourseId }: { trainingCourseId?: string }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const inputRef = useRef<HTMLInputElement>(null);
  const [busy, setBusy] = useState(false);
  const [description, setDescription] = useState("");

  const enabled = !!trainingCourseId;

  const { data, isLoading } = useQuery({
    queryKey: ["courseFiles", trainingCourseId],
    queryFn: () => getCourseFiles(trainingCourseId as string),
    enabled,
  });

  if (!enabled) {
    return (
      <div className="rounded-lg border border-dashed border-border bg-card/40 p-4 text-sm text-muted">
        {t("Save the course first — material attaches to a saved course.")}
      </div>
    );
  }
  if (isLoading) return <Loading />;

  const files = data ?? [];

  const refresh = () => {
    queryClient.invalidateQueries({ queryKey: ["courseFiles", trainingCourseId] });
    // The content editor's document picker reads the same list.
    queryClient.invalidateQueries({ queryKey: ["courseVersions", trainingCourseId] });
  };

  const pick = async (file?: File | null) => {
    if (!file) return;
    // Checked here as well as on the server so a 30 MB upload is refused before it is sent rather
    // than after the whole thing has crossed the wire.
    if (file.size > MAX_MB * 1024 * 1024) {
      toast.error(`${t("That file is")} ${size(file.size)} — ${t("the limit is")} ${MAX_MB} MB.`);
      return;
    }
    setBusy(true);
    const res = await uploadCourseFile(trainingCourseId as string, file, description.trim() || undefined);
    setBusy(false);
    if (inputRef.current) inputRef.current.value = "";
    if (res.ok) {
      setDescription("");
      toast.success(res.message);
      refresh();
    } else {
      toast.error(res.message);
    }
  };

  const remove = async (file: CourseFileModel) => {
    const ok = await confirm({
      title: t("Delete this file?"),
      message: file.usedByModules > 0
        // The server refuses this, but saying so first is better than letting them click into it.
        ? t("It is used by {{n}} module(s) and cannot be deleted while they point at it.", { n: file.usedByModules })
        : t("This cannot be undone. Any future module would need it uploaded again."),
      confirmLabel: t("Delete") ?? "Delete",
      variant: "destructive",
    });
    if (!ok) return;
    setBusy(true);
    const res = await deleteCourseFile(file.id);
    setBusy(false);
    if (res.ok) { toast.success(res.message); refresh(); }
    else toast.error(res.message);
  };

  return (
    <div className="rounded-lg border border-border bg-card p-4">
      <div className="mb-3 flex items-start gap-2">
        <span className="mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
          <FolderOpen className="h-4 w-4" />
        </span>
        <div className="min-w-0">
          <h3 className="text-sm font-semibold text-foreground">{t("Course Material")}</h3>
          <p className="text-xs text-muted">
            {t("PDFs, decks and procedures a Document module serves. Uploaded once per course and shared by every version that uses them.")}
          </p>
        </div>
      </div>

      <div className="mb-3 flex flex-wrap items-center gap-2">
        <input
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder={t("What this file is (optional)") ?? ""}
          className="min-w-0 flex-1 rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none"
        />
        <input
          ref={inputRef}
          type="file"
          accept=".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.txt,.csv,.md,.jpg,.jpeg,.png,.webp,.gif"
          onChange={(e) => void pick(e.target.files?.[0])}
          className="hidden"
        />
        <button
          type="button" disabled={busy} onClick={() => inputRef.current?.click()}
          className="inline-flex shrink-0 items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-40"
        >
          <Upload className="h-3.5 w-3.5" /> {busy ? t("Uploading…") : t("Add File")}
        </button>
      </div>

      {files.length === 0 ? (
        <p className="rounded-md border border-dashed border-border px-3 py-5 text-center text-xs text-muted">
          {t("No material yet. Add a PDF or a deck, then reference it from a Document module.")}
        </p>
      ) : (
        <div className="space-y-1.5">
          {files.map((f) => (
            <div key={f.id} className="flex flex-wrap items-center gap-2 rounded-md border border-border bg-secondary/20 px-2.5 py-2">
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm text-foreground">{f.fileName}</p>
                <p className="truncate text-[11px] text-muted">
                  {[size(f.fileSize), day(f.uploadedOn), f.description].filter(Boolean).join(" · ")}
                </p>
              </div>
              {/* In-use count doubles as the explanation for why Delete will refuse. */}
              {f.usedByModules > 0 && (
                <span className="shrink-0 rounded-full bg-primary/10 px-2 py-0.5 text-[11px] font-semibold text-primary">
                  {t("used by")} {f.usedByModules}
                </span>
              )}
              <button
                type="button" onClick={() => void previewCourseFile(f.id, f.fileName)}
                title={t("Download a copy") ?? undefined}
                className="shrink-0 rounded p-1 text-muted hover:bg-secondary"
              >
                <Download className="h-3.5 w-3.5" />
              </button>
              <button
                type="button" disabled={busy} onClick={() => void remove(f)}
                title={t("Delete") ?? undefined}
                className="shrink-0 rounded p-1 text-error hover:bg-error/10 disabled:opacity-40"
              >
                <Trash2 className="h-3.5 w-3.5" />
              </button>
            </div>
          ))}
        </div>
      )}

      <p className="mt-2 flex items-start gap-1.5 text-[11px] text-muted">
        <Info className="mt-0.5 h-3 w-3 shrink-0" />
        {t("Files cannot be edited after upload — that is what keeps a published version's material fixed. To replace one, upload the new file and point a draft module at it. Video belongs on a Video module as a URL.")}
      </p>
    </div>
  );
}

export default memo(MaterialLibrary);
