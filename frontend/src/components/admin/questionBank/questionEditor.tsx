"use client";
import { memo } from "react";
import { useTranslation } from "react-i18next";
import { Plus, Trash2, ArrowUp, ArrowDown, CircleDot, CheckSquare, ToggleLeft } from "lucide-react";
import type { QuestionKind, QuestionModel, QuestionOptionModel } from "@/models";

/**
 * The question editor, shared by the bank library and the quiz on a course module.
 *
 * <p>One editor because a bank question and a quiz question are the same thing at different moments
 * — importing copies one into the other. Two editors would drift, and the validation rules that make
 * a question gradable would end up written twice.</p>
 *
 * <p>Controlled: the parent owns the list and decides when to save. Nothing here calls an API.</p>
 */

const KINDS: { id: QuestionKind; name: string; icon: typeof CircleDot; hint: string }[] = [
  { id: "SingleChoice", name: "Single choice", icon: CircleDot, hint: "One right answer" },
  { id: "MultipleChoice", name: "Select all", icon: CheckSquare, hint: "Marked all-or-nothing" },
  { id: "TrueFalse", name: "True / false", icon: ToggleLeft, hint: "Exactly two options" },
];

export const blankQuestion = (): QuestionModel => ({
  text: "",
  kind: "SingleChoice",
  points: 1,
  explanation: "",
  options: [
    { text: "", isCorrect: true },
    { text: "", isCorrect: false },
  ],
});

const trueFalseOptions = (): QuestionOptionModel[] => [
  { text: "True", isCorrect: true },
  { text: "False", isCorrect: false },
];

function QuestionRow({
  question, index, count, onChange, onMove, onRemove,
}: {
  question: QuestionModel;
  index: number;
  count: number;
  onChange: (patch: Partial<QuestionModel>) => void;
  onMove: (delta: number) => void;
  onRemove: () => void;
}) {
  const { t } = useTranslation();
  const single = question.kind !== "MultipleChoice";

  const setKind = (kind: QuestionKind) => {
    // Switching to true/false replaces the options rather than trying to reshape whatever was there
    // — a two-option list with fixed labels is the whole point of the type.
    if (kind === "TrueFalse") {
      onChange({ kind, options: trueFalseOptions() });
      return;
    }
    // Leaving single-choice for select-all keeps the ticks; the reverse must leave exactly one, or
    // the server rejects the question on save.
    if (kind !== "MultipleChoice" && question.options.filter((o) => o.isCorrect).length > 1) {
      let first = true;
      onChange({
        kind,
        options: question.options.map((o) => {
          if (o.isCorrect && first) { first = false; return o; }
          return { ...o, isCorrect: false };
        }),
      });
      return;
    }
    onChange({ kind });
  };

  const patchOption = (i: number, patch: Partial<QuestionOptionModel>) => {
    onChange({
      options: question.options.map((o, idx) => {
        if (idx !== i) {
          // A single-answer question can only ever have one tick, so ticking one unticks the rest.
          return single && patch.isCorrect ? { ...o, isCorrect: false } : o;
        }
        return { ...o, ...patch };
      }),
    });
  };

  const addOption = () =>
    onChange({ options: [...question.options, { text: "", isCorrect: false }] });

  const removeOption = (i: number) =>
    onChange({ options: question.options.filter((_, idx) => idx !== i) });

  return (
    <div className="rounded-md border border-border bg-secondary/20 p-2.5">
      <div className="mb-2 flex items-start gap-2">
        <span className="mt-1 flex h-6 w-6 shrink-0 items-center justify-center rounded bg-primary/10 text-[11px] font-semibold text-primary">
          {index + 1}
        </span>
        <textarea
          value={question.text}
          onChange={(e) => onChange({ text: e.target.value })}
          rows={2}
          placeholder={t("What are you asking?") ?? ""}
          className="min-w-0 flex-1 rounded border border-border bg-card px-2 py-1 text-sm text-foreground focus:border-primary focus:outline-none"
        />
        <div className="flex shrink-0 flex-col gap-1">
          <div className="flex gap-1">
            <button type="button" onClick={() => onMove(-1)} disabled={index === 0}
              title={t("Move up") ?? undefined}
              className="rounded p-1 text-muted hover:bg-secondary disabled:opacity-30">
              <ArrowUp className="h-3.5 w-3.5" />
            </button>
            <button type="button" onClick={() => onMove(1)} disabled={index === count - 1}
              title={t("Move down") ?? undefined}
              className="rounded p-1 text-muted hover:bg-secondary disabled:opacity-30">
              <ArrowDown className="h-3.5 w-3.5" />
            </button>
            <button type="button" onClick={onRemove} title={t("Remove") ?? undefined}
              className="rounded p-1 text-error hover:bg-error/10">
              <Trash2 className="h-3.5 w-3.5" />
            </button>
          </div>
        </div>
      </div>

      <div className="mb-2 flex flex-wrap items-center gap-3 text-xs text-muted">
        <select
          value={question.kind}
          onChange={(e) => setKind(e.target.value as QuestionKind)}
          className="rounded border border-border bg-card px-2 py-1 text-xs text-foreground focus:border-primary focus:outline-none"
        >
          {KINDS.map((k) => (
            <option key={k.id} value={k.id}>{t(k.name)}</option>
          ))}
        </select>
        <span>{t(KINDS.find((k) => k.id === question.kind)?.hint ?? "")}</span>
        <label className="inline-flex items-center gap-1.5">
          <input
            type="number" min={0.5} step={0.5}
            value={question.points}
            onChange={(e) => onChange({ points: Number(e.target.value) })}
            className="w-16 rounded border border-border bg-card px-1.5 py-0.5 text-xs text-foreground focus:border-primary focus:outline-none"
          />
          {t("marks")}
        </label>
      </div>

      <div className="space-y-1">
        {question.options.map((o, i) => (
          <div key={i} className="flex items-center gap-2">
            <input
              type={single ? "radio" : "checkbox"}
              checked={o.isCorrect}
              onChange={(e) => patchOption(i, { isCorrect: e.target.checked })}
              name={single ? `correct-${index}` : undefined}
              className="h-4 w-4 shrink-0 accent-primary"
              aria-label={t("Correct answer") ?? "Correct answer"}
            />
            <input
              value={o.text}
              onChange={(e) => patchOption(i, { text: e.target.value })}
              readOnly={question.kind === "TrueFalse"}
              placeholder={`${t("Option")} ${i + 1}`}
              className="min-w-0 flex-1 rounded border border-border bg-card px-2 py-1 text-sm text-foreground focus:border-primary focus:outline-none read-only:text-muted"
            />
            {question.kind !== "TrueFalse" && question.options.length > 2 && (
              <button type="button" onClick={() => removeOption(i)}
                title={t("Remove option") ?? undefined}
                className="shrink-0 rounded p-1 text-muted hover:text-error">
                <Trash2 className="h-3.5 w-3.5" />
              </button>
            )}
          </div>
        ))}
      </div>

      {question.kind !== "TrueFalse" && (
        <button type="button" onClick={addOption}
          className="mt-1.5 text-[11px] font-medium text-primary hover:underline">
          + {t("Add option")}
        </button>
      )}

      <input
        value={question.explanation ?? ""}
        onChange={(e) => onChange({ explanation: e.target.value })}
        placeholder={t("Why this is the answer — shown when the quiz reveals it (optional)") ?? ""}
        className="mt-2 w-full rounded border border-border bg-card px-2 py-1 text-xs text-foreground focus:border-primary focus:outline-none"
      />

      {/* The rule that most often bites, said where it is broken rather than only on save. */}
      {question.options.every((o) => !o.isCorrect) && (
        <p className="mt-1.5 text-[11px] font-medium text-error">
          {t("Mark the correct answer — a question nobody can get right will be rejected.")}
        </p>
      )}
    </div>
  );
}

function QuestionEditor({
  questions, onChange, emptyHint,
}: {
  questions: QuestionModel[];
  onChange: (next: QuestionModel[]) => void;
  emptyHint?: string;
}) {
  const { t } = useTranslation();

  const patch = (i: number, p: Partial<QuestionModel>) =>
    onChange(questions.map((q, idx) => (idx === i ? { ...q, ...p } : q)));

  const move = (i: number, delta: number) => {
    const j = i + delta;
    if (j < 0 || j >= questions.length) return;
    const next = [...questions];
    [next[i], next[j]] = [next[j], next[i]];
    onChange(next);
  };

  const totalMarks = questions.reduce((sum, q) => sum + (Number(q.points) || 0), 0);

  return (
    <div>
      <div className="mb-2 flex flex-wrap items-center justify-between gap-2">
        <p className="text-xs text-muted">
          {questions.length === 0
            ? (emptyHint ?? t("No questions yet."))
            : `${questions.length} ${t("question(s)")} · ${totalMarks} ${t("marks")}`}
        </p>
        <button
          type="button"
          onClick={() => onChange([...questions, blankQuestion()])}
          className="inline-flex shrink-0 items-center gap-1.5 rounded-md border border-border px-2.5 py-1.5 text-xs font-semibold text-foreground hover:bg-secondary"
        >
          <Plus className="h-3.5 w-3.5" /> {t("Add Question")}
        </button>
      </div>

      {questions.length > 0 && (
        <div className="max-h-[34rem] space-y-2 overflow-auto pr-1">
          {questions.map((q, i) => (
            <QuestionRow
              key={q.id ?? `new-${i}`}
              question={q} index={i} count={questions.length}
              onChange={(p) => patch(i, p)}
              onMove={(d) => move(i, d)}
              onRemove={() => onChange(questions.filter((_, idx) => idx !== i))}
            />
          ))}
        </div>
      )}
    </div>
  );
}

export default memo(QuestionEditor);
