import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { ZodType } from "zod";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/**
 * Reads the record id out of the current URL when it is `/{base}/{guid}` and `base` names the same
 * resource this service saves. Returns "" in every other case — including `/x/new`, a nested child
 * route, or a resource mismatch — so the caller's own id is used untouched.
 *
 * This is a plain module (not a hook), so it reads `location` directly rather than `useParams`.
 */
function recoverIdFromRoute(resource: string): string {
  if (typeof window === "undefined") return "";
  const segments = window.location.pathname.split("/").filter(Boolean);
  if (segments.length !== 2) return "";
  const [base, candidate] = segments;
  if (base.toLowerCase() !== resource.toLowerCase()) return "";
  return /^[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}$/.test(candidate) ? candidate : "";
}

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
    /** Force the HTTP method instead of POST-create / PUT-update — e.g. an upsert endpoint that only
     * accepts POST (the record id still rides in the body so the server decides create vs update). */
    method?: "POST" | "PUT";
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

    // Last-resort id recovery. `createEntityGetById` swallows a failed fetch and resolves to
    // undefined, so a form opened at /{resource}/{guid} whose record didn't load renders with an
    // empty id — and an empty id here means POST, silently CREATING A DUPLICATE instead of failing
    // the update. Recover the id from the URL so the request stays a PUT and the server answers
    // honestly (404/409) rather than inserting a second row.
    //
    // Deliberately narrow: it fires ONLY when the route's own base segment names THIS resource.
    // That is what stops it misfiring on a child form hosted inside a parent's record URL (an
    // address form on /employee/{guid} has resource "EmployeeAddress" ≠ base "employee", so it is
    // left alone and still POSTs correctly).
    const routeId = recoverIdFromRoute(path);
    const effectiveId =
      formDataObj.id === undefined || formDataObj.id === "" || formDataObj.id === null
        ? routeId
        : formDataObj.id;

    const isUpdate =
      typeof effectiveId !== "undefined" && effectiveId !== "" && effectiveId !== null;

    const body: Record<string, unknown> = { ...formDataObj };
    if (!isUpdate) delete body.id;
    else body.id = effectiveId;

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
        method: options?.method ?? (isUpdate ? "PUT" : "POST"),
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
