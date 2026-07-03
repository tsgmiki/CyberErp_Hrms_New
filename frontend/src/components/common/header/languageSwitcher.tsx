import { Globe } from "lucide-react";
import { useTranslation } from "react-i18next";
import { startTransition } from "react";
import { setUserLocale } from "@/services/language";
import {
  HeaderDropdown,
  HeaderDropdownContent,
  HeaderDropdownItem,
  HeaderDropdownTrigger,
} from "./dropdown";

const languages = [
  { code: "en", label: "English", flag: "🇺🇸" },
  { code: "am", label: "አማርኛ", flag: "🇪🇹" },
];

function LanguageSwitcher() {
  const { i18n } = useTranslation();

  const onLocaleChange = (code: string) => {
    startTransition(() => {
      setUserLocale(code);
    });
  };

  return (
    <HeaderDropdown>
      <HeaderDropdownTrigger>
        <button
          type="button"
          className="w-8 h-8 flex items-center justify-center rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted transition-colors"
          aria-label="Change language"
        >
          <Globe className="w-4 h-4" />
        </button>
      </HeaderDropdownTrigger>
      <HeaderDropdownContent align="end" className="w-40">
        {languages.map((lang) => (
          <HeaderDropdownItem
            key={lang.code}
            onClick={() => onLocaleChange(lang.code)}
            className={`gap-2 text-xs ${i18n.language === lang.code ? "bg-accent" : ""}`}
          >
            <span>{lang.flag}</span>
            {lang.label}
          </HeaderDropdownItem>
        ))}
      </HeaderDropdownContent>
    </HeaderDropdown>
  );
}

export default LanguageSwitcher;
