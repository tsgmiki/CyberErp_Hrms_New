import type { MergeFieldModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Catalog of merge tokens (standard employee fields + active custom fields) for the editor palette. */
export default async function getMergeFields(): Promise<MergeFieldModel[]> {
  const response = await fetch(`${API_BASE_URL}/DocumentTemplate/merge-fields`, {
    credentials: "include",
  });
  if (!response.ok) return [];
  return (await response.json()) as MergeFieldModel[];
}
