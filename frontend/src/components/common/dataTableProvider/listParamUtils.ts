import { parameterInitialData } from "@/constants/initialization";
import type ParameterModel from "@/models/ParameterModel";

/** Merge list query patches without dropping required ParameterModel fields. */
export function patchListParam(
  prev: ParameterModel | undefined,
  patch: Partial<ParameterModel>,
): ParameterModel {
  return { ...parameterInitialData, ...prev, ...patch };
}
