import type { WorkflowModel } from "@/models";
import { signal } from "@preact/signals-react";

export interface WorkflowByVoucherType {
  voucherType: string;
  steps: WorkflowModel[];
  finalStatus: string;
  finalStep: number;
}

export const WorkflowData = signal({} as Record<string, WorkflowByVoucherType>);