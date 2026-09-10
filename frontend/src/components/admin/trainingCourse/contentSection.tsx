"use client";
import { memo, useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
  Layers, Plus, Save, Trash2, ArrowUp, ArrowDown, Send, FileText, Video, Link2, Clock,
  ClipboardCheck, FolderOpen,
} from "lucide-react";
import QuizSection from "./quizSection";
import { getCourseFiles } from "@/services/admin/courseFile";
import {
  getCourseVersions, createCourseVersion, setCourseVersionModules, publishCourseVersion,
} from "@/services/admin/courseContent";
import type { ContentModuleKind, ContentModuleModel, CourseVersionModel } from "@/models";
import { toast } from "@/components/common/toast";
import { confirm } from "@/components/common/dialog";
import Loading from "@/components/common/loader/loader";

/**
 * The kinds an author can add here.
 *
 * `Document` serves a file from the course's own material library (logic §12.89). It stayed
 * unreachable until that library existed, because the only file table available was scoped to one
 * employee — course material stored there is readable by exactly one person.
 */
const KINDS: { id: ContentModuleKind; name: string; hint: string }[] = [
  { id: "Text", name: "Text", hint: "Written in place — no file, no hosting" },
  { id: "Document", name: "Document", hint: "A PDF or deck from this course's material" },
  { id: "Video", name: "Video", hint: "A hosted video, by URL" },
  { id: "Link", name: "Link", hint: "An article or a provider's own page" },
  { id: "Quiz", name: "Quiz", hint: "Graded — passing it is what completes the module" },
];

const KIND_ICON: Record<string, typeof FileText> = {
  Text: FileText, Video, Link: Link2, Document: FolderOpen, Quiz: ClipboardCheck,
};

const blank = (): ContentModuleModel => ({
  title: "", kind: "Text", body: "", externalUrl: "", estimatedMinutes: null, isRequired: true,
});

const day = (v?: string | null) =>
  v ? new Date(v).toLocaleDateString(undefined, { day: "2-digit", month: "short", year: "numeric" }) : "";

/** One module row in the draft editor. */
function ModuleRow({
  module, index, count, files, onChange, onMove, onRemove,
}: {
  module: ContentModuleModel;
  index: number;
  count: number;
  files: { id: string; fileName: string }[];
  onChange: (patch: Partial<ContentModuleModel>) => void;
  onMove: (delta: number) => void;
  onRemove: () => void;
}) {
  const { t } = useTranslation();
  const Icon = KIND_ICON[module.kind] ?? FileText;

  return (
    <div className="rounded-md border border-border bg-secondary/20 p-2.5">
      <div className="mb-2 flex items-center gap-2">
        <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded bg-primary/10 text-[11px] font-semibold text-primary">
          {index + 1}
        </span>
        <Icon className="h-4 w-4 shrink-0 text-muted" />
        <input
          value={module.title}
          onChange={(e) => onChange({ title: e.target.value })}
          placeholder={t("Module title") ?? ""}
          className="min-w-0 flex-1 rounded border border-border bg-card px-2 py-1 text-sm text-foreground focus:border-primary focus:outline-none"
        />
        <select
          value={module.kind}
          onChange={(e) => onChange({ kind: e.target.value as ContentModuleKind })}
          className="shrink-0 rounded border border-border bg-card px-2 py-1 text-xs text-foreground focus:border-primary focus:outline-none"
        >
          {KINDS.map((k) => (
            <option key={k.id} value={k.id}>{t(k.name)}</option>
          ))}
        </select>
        {/* Order is the sequence the learner walks, so it is edited here rather than by drag —
            keyboard-reachable and unambiguous. */}
        <button type="button" onClick={() => onMove(-1)} disabled={index === 0}
          title={t("Move up") ?? undefined}
          className="shrink-0 rounded p-1 text-muted hover:bg-secondary disabled:opacity-30">
          <ArrowUp className="h-3.5 w-3.5" />
        </button>
        <button type="button" onClick={() => onMove(1)} disabled={index === count - 1}
          title={t("Move down") ?? undefined}
          className="shrink-0 rounded p-1 text-muted hover:bg-secondary disabled:opacity-30">
          <ArrowDown className="h-3.5 w-3.5" />
        </button>
        <button type="button" onClick={onRemove} title={t("Remove") ?? undefined}
          className="shrink-0 rounded p-1 text-error hover:bg-error/10">
          <Trash2 className="h-3.5 w-3.5" />
        </button>
      </div>

      {module.kind === "Text" ? (
        <textarea
          value={module.body ?? ""}
          onChange={(e) => onChange({ body: e.target.value })}
          rows={4}
          placeholder={t("What the learner reads on this page") ?? ""}
          className="w-full rounded border border-border bg-card px-2 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none"
        />
      ) : module.kind === "Document" ? (
        files.length === 0 ? (
          // Pointing at nothing is the failure this picker exists to prevent, so the empty state
          // says where the file comes from rather than showing a dropdown with no options.
          <p className="rounded border border-dashed border-border px-2.5 py-2 text-xs text-muted">
            {t("No material uploaded yet — add a file in Course Material below, then pick it here.")}
          </p>
        ) : (
          <select
            value={module.courseFileId ?? ""}
            onChange={(e) => onChange({ courseFileId: e.target.value || null })}
            className="w-full rounded border border-border bg-card px-2 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none"
          >
            <option value="">{t("Choose a file…")}</option>
            {files.map((x) => (
              <option key={x.id} value={x.id}>{x.fileName}</option>
            ))}
          </select>
        )
      ) : module.kind === "Quiz" ? (
        // A quiz carries no content of its own — its content is the assessment below, which needs
        // this module's id and so appears only once the content has been saved.
        <QuizSection moduleId={module.id} moduleTitle={module.title} />
      ) : (
        <input
          value={module.externalUrl ?? ""}
          onChange={(e) => onChange({ externalUrl: e.target.value })}
          placeholder={module.kind === "Video" ? "https://… (video)" : "https://…"}
          className="w-full rounded border border-border bg-card px-2 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none"
        />
      )}

      <div className="mt-2 flex flex-wrap items-center gap-3 text-xs text-muted">
        <label className="inline-flex items-center gap-1.5">
          <Clock className="h-3.5 w-3.5" />
          <input
            type="number" min={0}
            value={module.estimatedMinutes ?? ""}
            onChange={(e) => onChange({
              estimatedMinutes: e.target.value === "" ? null : Number(e.target.value),
            })}
            placeholder={t("mins") ?? ""}
            className="w-16 rounded border border-border bg-card px-1.5 py-0.5 text-xs text-foreground focus:border-primary focus:outline-none"
          />
          {t("minutes")}
        </label>
        <label className="inline-flex items-center gap-1.5">
          <input
            type="checkbox" checked={module.isRequired}
            onChange={(e) => onChange({ isRequired: e.target.checked })}
            className="h-3.5 w-3.5 accent-primary"
          />
          {/* Required is what completion is measured against, so the consequence is spelled out. */}
          {module.isRequired ? t("Required to complete the course") : t("Optional — does not gate completion")}
        </label>
      </div>
    </div>
  );
}

/**
 * Course content: versions and the modules inside them.
 *
 * <p>Before this, a course was metadata with no material and "completed" was a value HR typed into
 * an enrolment. Content published here is what a learner actually works through, and finishing every
 * required module is what completes their enrolment.</p>
 *
 * <p>Editing is confined to the DRAFT. A published version is frozen so that "who is current on the
 * latest revision?" stays answerable — a revision opens a new version, which retires the old one on
 * publish.</p>
 */
function CourseContentSection({ trainingCourseId }: { trainingCourseId?: string }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [modules, setModules] = useState<ContentModuleModel[]>([]);
  const [dirty, setDirty] = useState(false);
  const [busy, setBusy] = useState(false);
  const [note, setNote] = useState("");

  const enabled = !!trainingCourseId;

  const { data: versions, isLoading } = useQuery({
    queryKey: ["courseVersions", trainingCourseId],
    queryFn: () => getCourseVersions(trainingCourseId as string),
    enabled,
  });

  // Loaded once for the whole editor rather than per Document module.
  const { data: courseFiles } = useQuery({
    queryKey: ["courseFiles", trainingCourseId],
    queryFn: () => getCourseFiles(trainingCourseId as string),
    enabled,
  });

  const draft = useMemo(
    () => (versions ?? []).find((v) => v.status === "Draft"),
    [versions],
  );
  const published = useMemo(
    () => (versions ?? []).find((v) => v.status === "Published"),
    [versions],
  );

  // Re-seed from the server whenever the draft changes, so a save leaves the editor showing what was
  // stored rather than the keystrokes that produced it.
  useEffect(() => {
    setModules(draft ? draft.modules.map((m) => ({ ...m })) : []);
    setDirty(false);
  }, [draft]);

  if (!enabled) {
    return (
      <div className="rounded-lg border border-dashed border-border bg-card/40 p-4 text-sm text-muted">
        {t("Save the course first — content attaches to a saved course.")}
      </div>
    );
  }
  if (isLoading) return <Loading />;

  const refresh = () => {
    queryClient.invalidateQueries({ queryKey: ["courseVersions", trainingCourseId] });
    // The catalogue shows whether a course has content to work through.
    queryClient.invalidateQueries({ queryKey: ["trainingCatalog"] });
  };

  const startDraft = async () => {
    setBusy(true);
    const res = await createCourseVersion(trainingCourseId as string, note.trim() || undefined);
    setBusy(false);
    if (res.status === "success") {
      setNote("");
      toast.success(t("Draft created"));
      refresh();
    } else toast.error(res.message);
  };

  const patch = (i: number, p: Partial<ContentModuleModel>) => {
    setModules((prev) => prev.map((m, idx) => (idx === i ? { ...m, ...p } : m)));
    setDirty(true);
  };

  const move = (i: number, delta: number) => {
    setModules((prev) => {
      const next = [...prev];
      const j = i + delta;
      if (j < 0 || j >= next.length) return prev;
      [next[i], next[j]] = [next[j], next[i]];
      return next;
    });
    setDirty(true);
  };

  const remove = (i: number) => {
    setModules((prev) => prev.filter((_, idx) => idx !== i));
    setDirty(true);
  };

  const add = () => {
    setModules((prev) => [...prev, blank()]);
    setDirty(true);
  };

  const save = async () => {
    if (!draft) return;
    setBusy(true);
    const res = await setCourseVersionModules(draft.id, modules);
    setBusy(false);
    if (res.status === "success") {
      toast.success(res.message);
      refresh();
    } else toast.error(res.message);
  };

  const publish = async () => {
    if (!draft) return;
    if (dirty) {
      toast.error(t("Save the content before publishing."));
      return;
    }
    const ok = await confirm({
      title: t("Publish this version?"),
      // Publishing is the irreversible step: content freezes and the live version changes under
      // everyone who has not yet finished, so the consequence is stated before the click counts.
      message: published
        ? t("v{{n}} goes live and v{{old}} is retired. Published content can no longer be edited — a further change needs a new version.", { n: draft.versionNumber, old: published.versionNumber })
        : t("v{{n}} goes live to everyone enrolled. Published content can no longer be edited — a further change needs a new version.", { n: draft.versionNumber }),
      confirmLabel: t("Publish"),
    });
    if (!ok) return;
    setBusy(true);
    const res = await publishCourseVersion(draft.id);
    setBusy(false);
    if (res.status === "success") {
      toast.success(res.message);
      refresh();
    } else toast.error(res.message);
  };

  const totalMinutes = modules.reduce((sum, m) => sum + (m.estimatedMinutes ?? 0), 0);
  const requiredCount = modules.filter((m) => m.isRequired).length;

  return (
    <div className="rounded-lg border border-border bg-card p-4">
      <div className="mb-3 flex items-start gap-2">
        <span className="mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
          <Layers className="h-4 w-4" />
        </span>
        <div className="min-w-0">
          <h3 className="text-sm font-semibold text-foreground">{t("Course Content")}</h3>
          <p className="text-xs text-muted">
            {t("What the learner works through. Finishing every required module completes their enrolment.")}
          </p>
        </div>
      </div>

      {/* What is live, stated plainly — an author's first question is always "what do learners see?" */}
      <div className="mb-3 rounded-md border border-border bg-secondary/20 px-3 py-2 text-xs">
        {published ? (
          <span className="text-foreground">
            <span className="font-semibold text-success">{t("Live")}: </span>
            {`v${published.versionNumber} · ${published.moduleCount} ${t("module(s)")} · ${published.requiredModuleCount} ${t("required")}`}
            {published.totalMinutes > 0 ? ` · ${published.totalMinutes} ${t("mins")}` : ""}
            {published.publishedOn ? ` · ${t("published")} ${day(published.publishedOn)}` : ""}
          </span>
        ) : (
          <span className="text-muted">
            {t("Nothing is live yet — learners see this course with no content to work through.")}
          </span>
        )}
      </div>

      {!draft ? (
        <div className="flex flex-wrap items-center gap-2">
          <input
            value={note}
            onChange={(e) => setNote(e.target.value)}
            placeholder={
              published
                ? (t("What changed in this revision (shown to learners asked to retake it)") ?? "")
                : (t("Optional note about this version") ?? "")
            }
            className="min-w-0 flex-1 rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none"
          />
          <button
            type="button" disabled={busy} onClick={startDraft}
            className="inline-flex shrink-0 items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-40"
          >
            <Plus className="h-3.5 w-3.5" />
            {published ? t("New Version") : t("Start Content")}
          </button>
        </div>
      ) : (
        <>
          <div className="mb-2 flex flex-wrap items-center justify-between gap-2">
            <p className="text-xs text-muted">
              <span className="rounded bg-warning/15 px-1.5 py-0.5 font-semibold text-warning">
                {t("Draft")} v{draft.versionNumber}
              </span>
              <span className="ml-2">
                {`${modules.length} ${t("module(s)")} · ${requiredCount} ${t("required")}`}
                {totalMinutes > 0 ? ` · ${totalMinutes} ${t("mins")}` : ""}
              </span>
              {draft.changeNote ? <span className="ml-2 italic">“{draft.changeNote}”</span> : null}
            </p>
            <div className="flex shrink-0 items-center gap-2">
              <button
                type="button" onClick={add}
                className="inline-flex items-center gap-1.5 rounded-md border border-border px-2.5 py-1.5 text-xs font-semibold text-foreground hover:bg-secondary"
              >
                <Plus className="h-3.5 w-3.5" /> {t("Add Module")}
              </button>
              <button
                type="button" disabled={busy || !dirty} onClick={save}
                className="inline-flex items-center gap-1.5 rounded-md bg-primary px-2.5 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-40"
              >
                <Save className="h-3.5 w-3.5" />
                {busy ? t("Saving…") : dirty ? t("Save Content") : t("Saved")}
              </button>
              <button
                type="button" disabled={busy || modules.length === 0} onClick={publish}
                className="inline-flex items-center gap-1.5 rounded-md bg-success px-2.5 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-40"
              >
                <Send className="h-3.5 w-3.5" /> {t("Publish")}
              </button>
            </div>
          </div>

          {modules.length === 0 ? (
            <p className="rounded-md border border-dashed border-border px-3 py-6 text-center text-xs text-muted">
              {t("No modules yet. Add the first page, video or link the learner should work through.")}
            </p>
          ) : (
            <div className="max-h-[30rem] space-y-2 overflow-auto pr-1">
              {modules.map((m, i) => (
                <ModuleRow
                  key={m.id ?? `new-${i}`}
                  module={m} index={i} count={modules.length}
                  files={courseFiles ?? []}
                  onChange={(p) => patch(i, p)}
                  onMove={(d) => move(i, d)}
                  onRemove={() => remove(i)}
                />
              ))}
            </div>
          )}

          <p className="mt-2 text-[11px] text-muted">
            {t("Modules are shown in this order. A Document module serves a file from this course's material library; a Video module is a URL.")}
          </p>
        </>
      )}

      {/* Retired versions are listed, not hidden: they are what older completions were earned on. */}
      {(versions ?? []).some((v: CourseVersionModel) => v.status === "Retired") && (
        <div className="mt-3 border-t border-border pt-2">
          <p className="mb-1 text-[11px] font-semibold uppercase tracking-wide text-muted">{t("Earlier versions")}</p>
          <div className="flex flex-wrap gap-1.5">
            {(versions ?? [])
              .filter((v) => v.status === "Retired")
              .map((v) => (
                <span key={v.id} className="rounded bg-secondary px-2 py-0.5 text-[11px] text-muted">
                  {`v${v.versionNumber} · ${v.moduleCount} ${t("module(s)")}`}
                  {v.publishedOn ? ` · ${day(v.publishedOn)}` : ""}
                </span>
              ))}
          </div>
        </div>
      )}
    </div>
  );
}

export default memo(CourseContentSection);
