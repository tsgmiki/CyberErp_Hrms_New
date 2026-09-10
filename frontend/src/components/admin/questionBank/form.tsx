"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { QuestionBankModel, QuestionModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Save, ListChecks } from "lucide-react";
import saveQuestionBank from "@/services/admin/questionBank/save";
import getQuestionBank from "@/services/admin/questionBank/get";
import { getBankQuestions, setBankQuestions } from "@/services/admin/questionBank/questions";
import QuestionEditor from "./questionEditor";
import { toast } from "@/components/common/toast";
import Loading from "../../common/loader/loader";
import { activeStatusOptions, activeId, activeLabel } from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);
const NEW_DEFAULTS: QuestionBankModel = { isActive: true };

/**
 * The questions in the bank.
 *
 * <p>Saved through its own endpoint rather than with the bank form, because it needs a bank id to
 * attach to — the same rule the course's competency and content sections follow.</p>
 */
function BankQuestionsSection({ questionBankId }: { questionBankId?: string }) {
  const queryClient = useQueryClient();
  const [questions, setQuestions] = useState<QuestionModel[]>([]);
  const [dirty, setDirty] = useState(false);
  const [busy, setBusy] = useState(false);

  const enabled = !!questionBankId;

  const { data, isLoading } = useQuery({
    queryKey: ["bankQuestions", questionBankId],
    queryFn: () => getBankQuestions(questionBankId as string),
    enabled,
  });

  // Re-seed from the server after every save, so the editor shows what was stored rather than the
  // keystrokes that produced it.
  useEffect(() => {
    setQuestions((data ?? []).map((q) => ({ ...q, options: q.options.map((o) => ({ ...o })) })));
    setDirty(false);
  }, [data]);

  if (!enabled) {
    return (
      <div className="rounded-lg border border-dashed border-border bg-card/40 p-4 text-sm text-muted">
        Save the bank first — questions attach to a saved bank.
      </div>
    );
  }
  if (isLoading) return <Loading />;

  const save = async () => {
    setBusy(true);
    const res = await setBankQuestions(questionBankId as string, questions);
    setBusy(false);
    if (res.status === "success") {
      toast.success(res.message);
      queryClient.invalidateQueries({ queryKey: ["bankQuestions", questionBankId] });
      // The list shows a question count.
      queryClient.invalidateQueries({ queryKey: ["questionBanks"] });
    } else {
      toast.error(res.message);
    }
  };

  return (
    <div className="rounded-lg border border-border bg-card p-4">
      <div className="mb-3 flex items-start justify-between gap-3">
        <div className="flex items-start gap-2">
          <span className="mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
            <ListChecks className="h-4 w-4" />
          </span>
          <div>
            <h3 className="text-sm font-semibold text-foreground">Questions</h3>
            <p className="text-xs text-muted">
              Written once here, then imported into any course quiz. Importing copies them, so editing
              a bank never changes a quiz that already exists.
            </p>
          </div>
        </div>
        <button
          type="button"
          disabled={busy || !dirty}
          onClick={save}
          className="inline-flex shrink-0 items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-40"
        >
          <Save className="h-3.5 w-3.5" />
          {busy ? "Saving…" : dirty ? "Save Questions" : "Saved"}
        </button>
      </div>

      <QuestionEditor
        questions={questions}
        onChange={(next) => { setQuestions(next); setDirty(true); }}
        emptyHint="No questions yet. Add the first one."
      />
    </div>
  );
}

function QuestionBankForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<QuestionBankModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["questionBank", id],
    queryFn: () => getQuestionBank(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveQuestionBank(fd);
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

  useEffect(() => {
    if (typeof record != "undefined" && record != null) setFormData(record);
    else if (!id) setFormData({ ...NEW_DEFAULTS });
  }, [record, id]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({ ...NEW_DEFAULTS });
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["questionBanks"] });
      queryClient.invalidateQueries({ queryKey: ["questionBank"] });
      setId("");
    }
  }, [formState]);

  return (
    <div className="text-white">
      {pending && <Loading />}
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[30%]",
          isPending: isLoading,
          SubmitButton: "top",
          formId: "questionBankForm",
          components: [
            { name: "name", label: "Name", placeholder: "e.g. Cold Chain SOP", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
            {
              name: "isActive", label: "Status", type: "dropDown", onSelect: selectHandler,
              value: activeId(formData.isActive), displayValue: activeLabel(formData.isActive),
              data: activeStatusOptions as never,
            },
            { name: "description", label: "Description", placeholder: "What this bank covers", value: formData.description, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
      <div className="mt-3">
        <BankQuestionsSection questionBankId={formData.id || id} />
      </div>
    </div>
  );
}
export default QuestionBankForm;
