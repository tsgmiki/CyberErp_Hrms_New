"use client";
import { memo } from "react";
import CalibrationCreate from "./create";
import CalibrationWorkspace from "./workspace";

/** Dispatcher: no id → create a session; an id → the calibration workspace. */
function CalibrationForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  if (!id) return <CalibrationCreate onCreated={setId} />;
  return <CalibrationWorkspace id={id} />;
}

export default memo(CalibrationForm);
