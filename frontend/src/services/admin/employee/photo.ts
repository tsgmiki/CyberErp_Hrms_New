const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** URL of the stored employee photo (auth cookie rides along — same-site). */
export function employeePhotoUrl(employeeId: string, cacheBust?: number) {
  return `${API_BASE_URL}/Employee/${employeeId}/photo${cacheBust ? `?v=${cacheBust}` : ""}`;
}

export interface PhotoUploadResult {
  status: "success" | "error";
  message: string;
}

/** Uploads / replaces the employee photo (JPG, PNG or WEBP, max 2 MB). */
export async function uploadEmployeePhoto(employeeId: string, file: File): Promise<PhotoUploadResult> {
  const body = new FormData();
  body.append("file", file);
  try {
    const response = await fetch(`${API_BASE_URL}/Employee/${employeeId}/photo`, {
      method: "POST",
      credentials: "include",
      body,
    });
    if (!response.ok) {
      const text = await response.text();
      let message = "Photo upload failed.";
      try {
        message = JSON.parse(text).message ?? message;
      } catch {
        /* keep default */
      }
      return { status: "error", message };
    }
    return { status: "success", message: "Photo uploaded" };
  } catch {
    return { status: "error", message: "Network error while uploading photo." };
  }
}
