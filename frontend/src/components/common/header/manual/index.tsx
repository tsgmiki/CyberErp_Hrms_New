import { BookA, Check, ExternalLink, PanelTop } from "lucide-react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { appConfig } from "@/config/appConfig";
import {
  HeaderDropdown,
  HeaderDropdownContent,
  HeaderDropdownItem,
  HeaderDropdownSeparator,
  HeaderDropdownTrigger,
} from "../dropdown";
import { useManualPreference, type ManualOpenMode } from "./useManualPreference";

/**
 * The system manual, in the header.
 *
 * <p>⚠️ This used to be a bare <c>&lt;a href="/manual"&gt;</c> pointing at a route that was never
 * registered, so every click fell through the SPA's history fallback and rendered Not Found. It now
 * opens the PDF at {@link appConfig.manualUrl} the way each user prefers, and the preference lives
 * in this menu rather than on a settings page — it is a property of this button, and nobody goes
 * looking in Settings for how a header icon behaves.</p>
 *
 * <p>Built on the same `HeaderDropdown` the language and theme switchers use, so it inherits their
 * outside-click, Escape and portal-positioning behaviour rather than re-implementing it.</p>
 */
function Manual() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [mode, setMode] = useManualPreference();

  const openManual = () => {
    if (mode === "tab") {
      // ⚠️ noopener/noreferrer. A tab opened without it keeps a `window.opener` handle that can
      // navigate the app it came from; a PDF has no need of one, so it does not get one.
      window.open(appConfig.manualUrl, "_blank", "noopener,noreferrer");
      return;
    }
    navigate("/manual");
  };

  const modes: { key: ManualOpenMode; label: string; Icon: typeof PanelTop }[] = [
    { key: "embedded", label: t("In this page"), Icon: PanelTop },
    { key: "tab", label: t("In a new tab"), Icon: ExternalLink },
  ];

  return (
    <HeaderDropdown>
      <HeaderDropdownTrigger>
        <button
          type="button"
          className="w-8 h-8 flex items-center justify-center rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted transition-colors"
          title={t("Manual") ?? "Manual"}
          aria-label={t("Manual") ?? "Manual"}
        >
          <BookA className="w-4 h-4" />
        </button>
      </HeaderDropdownTrigger>
      <HeaderDropdownContent align="end" className="w-52">
        <HeaderDropdownItem onClick={openManual} className="gap-2 text-xs font-medium">
          <BookA className="h-3.5 w-3.5 shrink-0" />
          {t("Open manual")}
        </HeaderDropdownItem>

        <HeaderDropdownSeparator />

        {/* The setting, stated as the question it answers rather than as a field name. */}
        <p className="px-2 py-1 text-[10px] font-semibold uppercase tracking-wide text-muted-foreground">
          {t("Open it")}
        </p>
        {modes.map(({ key, label, Icon }) => (
          <HeaderDropdownItem
            key={key}
            onClick={() => setMode(key)}
            className={`gap-2 text-xs ${mode === key ? "bg-accent" : ""}`}
          >
            <Icon className="h-3.5 w-3.5 shrink-0" />
            <span className="flex-1 text-left">{label}</span>
            {mode === key ? <Check className="h-3.5 w-3.5 shrink-0 text-primary" /> : null}
          </HeaderDropdownItem>
        ))}
      </HeaderDropdownContent>
    </HeaderDropdown>
  );
}

export default Manual;
