import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { ZodType } from "zod";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface SaveResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
}

/**
 * Factory for the standard create/update service (POST/PUT to `/{resource}`, id in body).
 * Mirrors the hand-written `admin/module/save.ts` but coerces booleans and drops empty
 * optional fields so nullable numeric/guid columns bind correctly on the .NET side.
 */
export function createSaveService(
  resource: string,
  schema: ZodType,
  options?: {
    booleanFields?: string[];
    /** Coerce to a whole number before sending (invalid/empty → dropped). Prevents "55.5" → int? bind failures. */
    integerFields?: string[];
    /** Coerce to a number before sending (invalid/empty → dropped). */
    numberFields?: string[];
    /** Gather `cf_`-prefixed fields (dynamic custom fields, HC021) into a nested `customFields` dict. */
    customFields?: boolean;
  },
) {
  const path = resource.replace(/^\//, "").replace(/\/$/, "");
  return async function save(formData: FormData): Promise<SaveResult> {
    const formDataObj: Record<string, unknown> = Object.fromEntries(formData);

    const result = schema.safeParse(formDataObj);
    if (!result.success) {
      return {
        status: "error",
        message: "Validation failed",
        zodErrors: result.error.flatten().fieldErrors,
      };
    }

    const isUpdate =
      typeof formDataObj.id !== "undefined" &&
      formDataObj.id !== "" &&
      formDataObj.id !== null;

    const body: Record<string, unknown> = { ...formDataObj };
    if (!isUpdate) delete body.id;

    // Gather dynamic custom fields (HC021) into a nested dict BEFORE the empty-drop below, so blank
    // values survive as "" for the backend's required-field validation. `cf_bloodType` → customFields.bloodType.
    if (options?.customFields) {
      const cf: Record<string, string> = {};
      for (const key of Object.keys(body)) {
        if (key.startsWith("cf_")) {
          cf[key.slice(3)] = body[key] == null ? "" : String(body[key]);
          delete body[key];
        }
      }
      body.customFields = cf;
    }

    for (const field of options?.booleanFields ?? []) {
      if (field in body) body[field] = body[field] === "true" || body[field] === "on" || body[field] === true;
    }
    // Drop empty optional fields → nullable columns receive null instead of "".
    for (const key of Object.keys(body)) {
      if (body[key] === "") delete body[key];
    }
    // Coerce numeric fields to real JSON numbers so nullable int/decimal columns bind
    // reliably (a stray decimal in an int field would otherwise fail JSON deserialization).
    for (const field of options?.integerFields ?? []) {
      if (field in body) {
        const n = Math.trunc(Number(body[field]));
        if (Number.isFinite(n)) body[field] = n;
        else delete body[field];
      }
    }
    for (const field of options?.numberFields ?? []) {
      if (field in body) {
        const n = Number(body[field]);
        if (Number.isFinite(n)) body[field] = n;
        else delete body[field];
      }
    }

    try {
      const response = await fetch(`${API_BASE_URL}/${path}`, {
        method: isUpdate ? "PUT" : "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
      });

      if (!response.ok) {
        const text = await response.text();
        const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
        const message = errorMessageParser(parsed.errors || parsed);
        return { status: "error", message, zodErrors: {} };
      }

      return { status: "success", message: "Successfully saved", zodErrors: {} };
    } catch {
      return { status: "error", message: "Network error", zodErrors: {} };
    }
  };
}
