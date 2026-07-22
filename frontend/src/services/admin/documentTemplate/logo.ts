const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface CompanyLogoInfo {
  hasLogo: boolean;
  contentType?: string | null;
}

/** Company logo image URL (per tenant). Cookies ride along automatically for the <img> request. */
export function companyLogoUrl(cacheBust?: number | string): string {
  const suffix = cacheBust ? `?cb=${cacheBust}` : "";
  return `${API_BASE_URL}/DocumentTemplate/logo${suffix}`;
}

export async function getCompanyLogoInfo(): Promise<CompanyLogoInfo> {
  const res = await fetch(`${API_BASE_URL}/DocumentTemplate/logo/info`, {
    credentials: "include",
  });
  if (!res.ok) return { hasLogo: false };
  return (await res.json()) as CompanyLogoInfo;
}

export async function uploadCompanyLogo(file: File): Promise<{ ok: boolean; message: string }> {
  const form = new FormData();
  form.append("file", file);
  const res = await fetch(`${API_BASE_URL}/DocumentTemplate/logo`, {
    method: "POST",
    credentials: "include",
    body: form,
  });
  const text = await res.text();
  let message = "Logo uploaded";
  try {
    message = JSON.parse(text)?.message ?? message;
  } catch {
    /* non-JSON error body */
    if (text) message = text;
  }
  return { ok: res.ok, message };
}

export async function deleteCompanyLogo(): Promise<boolean> {
  const res = await fetch(`${API_BASE_URL}/DocumentTemplate/logo`, {
    method: "DELETE",
    credentials: "include",
  });
  return res.ok;
}
