import type { GeneratedDocumentModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Render a template's merged, print-ready HTML for a specific employee (HC022). */
export default async function generateDocument(
  templateId: string,
  employeeId: string,
): Promise<GeneratedDocumentModel> {
  const response = await fetch(
    `${API_BASE_URL}/DocumentTemplate/${templateId}/generate/${employeeId}`,
    { credentials: "include" },
  );
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || "Failed to generate document");
  }
  return (await response.json()) as GeneratedDocumentModel;
}
