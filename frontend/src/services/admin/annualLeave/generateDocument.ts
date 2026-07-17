import type { GeneratedDocumentModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Render a template's merged, print-ready HTML for one annual-leave request (header + detail). */
export default async function generateAnnualLeaveDocument(
  templateId: string,
  annualLeaveId: string,
): Promise<GeneratedDocumentModel> {
  const response = await fetch(
    `${API_BASE_URL}/DocumentTemplate/${templateId}/generate-annual-leave/${annualLeaveId}`,
    { credentials: "include" },
  );
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || "Failed to generate document");
  }
  return (await response.json()) as GeneratedDocumentModel;
}
