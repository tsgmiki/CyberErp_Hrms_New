# System manual

Put the manual PDF **in this folder**. The header's 📖 Manual button serves it from here.

```
public/manuals/user-manual.pdf     <-- the default name the app looks for
```

## Publishing a manual

1. Name the PDF `user-manual.pdf` and drop it in this folder.
2. That is all. `npm run build` copies `public/` into `dist/` verbatim.

**A published site needs no rebuild.** The same file dropped into the deployed `dist/manuals/`
folder is live on the next request — which is the point of keeping it here rather than importing it
into the bundle.

## Using a different name or location

Set `VITE_MANUAL_URL` in `.env.development` / `.env.production`:

```ini
# a different file name in this folder
VITE_MANUAL_URL=/manuals/cyber-erp-hrms-v2.pdf

# or one manual shared by every subsystem, served from anywhere
VITE_MANUAL_URL=https://docs.example.com/cyber-erp/manual.pdf
```

An absolute cross-origin URL is served straight to the viewer without a reachability check — a
cross-origin `HEAD` is blocked by CORS and would report a perfectly good file as missing.

## If the page says "The manual has not been published yet"

The app asked for the address shown on that screen and did not get a PDF back. Either the file is
not there under that exact name (it is case-sensitive on Linux hosts), or the web server is not
serving `.pdf` — IIS needs a `application/pdf` MIME mapping, which is present by default.

> Why the content type is checked and not just the status code: a missing file under the SPA's own
> origin does not return 404. It falls through the history fallback and returns **200 with
> index.html**, so a status-only check would show a blank viewer instead of this message.
