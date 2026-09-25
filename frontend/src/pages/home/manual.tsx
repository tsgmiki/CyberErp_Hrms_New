import { memo, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { BookA, Download, ExternalLink, TriangleAlert } from "lucide-react";
import { appConfig } from "@/config/appConfig";

/**
 * The system manual, embedded in the application shell.
 *
 * <p>⚠️ Rendered in an <c>&lt;iframe&gt;</c> through the BROWSER's own PDF viewer, deliberately NOT
 * through the app's `documentViewer` component. That component pulls its pdf.js worker from the
 * unpkg CDN, and the manual is precisely the page somebody opens when the network or the
 * deployment is already misbehaving — a help page that needs the public internet to render is a
 * help page that fails when it is needed. The native viewer costs nothing in bundle size, needs no
 * CDN, and brings its own zoom, search, print and download toolbar.</p>
 */
function ManualPage() {
  const { t } = useTranslation();
  const url = appConfig.manualUrl;
  const [state, setState] = useState<"checking" | "ready" | "missing">("checking");

  useEffect(() => {
    let cancelled = false;

    // A manual hosted elsewhere cannot be probed — a cross-origin HEAD is blocked by CORS and would
    // report a perfectly good file as missing. Trust the iframe to render it instead.
    const absolute = /^https?:\/\//i.test(url);
    if (absolute && !url.startsWith(window.location.origin)) {
      setState("ready");
      return;
    }

    fetch(url, { method: "HEAD" })
      .then((r) => {
        // ⚠️ `r.ok` alone is not enough. A file missing under the SPA's own origin does not 404 —
        // it falls through the history fallback, which answers 200 with index.html. The content
        // type is what separates "the PDF is there" from "you are looking at the app again".
        const isPdf = (r.headers.get("content-type") ?? "").toLowerCase().includes("pdf");
        if (!cancelled) setState(r.ok && isPdf ? "ready" : "missing");
      })
      .catch(() => {
        if (!cancelled) setState("missing");
      });

    return () => {
      cancelled = true;
    };
  }, [url]);

  return (
    <div className="flex h-[calc(100vh-7rem)] min-h-[420px] flex-col gap-2 p-2">
      <div className="flex flex-wrap items-center gap-2">
        <BookA className="h-4 w-4 shrink-0 text-primary" />
        <h1 className="flex-1 truncate text-sm font-semibold text-foreground">
          {t("System Manual")}
        </h1>
        {state === "ready" && (
          <>
            <a
              href={url}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-1.5 rounded-md border border-border px-2.5 py-1 text-xs text-muted hover:text-foreground"
            >
              <ExternalLink className="h-3.5 w-3.5" /> {t("Open in a new tab")}
            </a>
            <a
              href={url}
              download
              className="inline-flex items-center gap-1.5 rounded-md border border-border px-2.5 py-1 text-xs text-muted hover:text-foreground"
            >
              <Download className="h-3.5 w-3.5" /> {t("Download")}
            </a>
          </>
        )}
      </div>

      {state === "missing" ? (
        // Names the exact file it looked for. "The manual is unavailable" sends somebody to open a
        // ticket; this sends them to drop a file in a folder.
        <div className="flex flex-1 flex-col items-center justify-center gap-2 rounded-lg border border-dashed border-border p-6 text-center">
          <TriangleAlert className="h-6 w-6 text-warning" />
          <p className="text-sm font-semibold text-foreground">{t("The manual has not been published yet.")}</p>
          <p className="max-w-lg text-xs text-muted">
            {t("Place the PDF at this address to publish it — no rebuild is needed:")}
          </p>
          <code className="rounded bg-secondary/40 px-2 py-1 text-xs tabular-nums text-foreground">{url}</code>
          <p className="max-w-lg text-[11px] text-muted">
            {t("Drop the file into the deployed app's /manuals folder, or set VITE_MANUAL_URL to point somewhere else.")}
          </p>
        </div>
      ) : (
        <iframe
          // Keyed on the URL so switching the configured manual re-mounts the viewer rather than
          // leaving the previous document on screen.
          key={url}
          src={url}
          title={t("System Manual") ?? "System Manual"}
          className={`flex-1 rounded-lg border border-border bg-card ${state === "checking" ? "opacity-0" : ""}`}
        />
      )}
    </div>
  );
}

export default memo(ManualPage);
