"use client";
import { memo } from "react";
import AppraisalGenerate from "./generate";
import AppraisalScoring from "./scoring";

/** Dispatcher: no id → the Generate form; an id → the scoring workspace. */
function AppraisalForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  if (!id) return <AppraisalGenerate onGenerated={setId} />;
  return <AppraisalScoring id={id} setId={setId} />;
}

export default memo(AppraisalForm);
