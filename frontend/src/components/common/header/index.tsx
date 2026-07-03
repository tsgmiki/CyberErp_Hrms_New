import { lazy, memo, Suspense, useEffect, useState } from "react";
import HeaderSearch from "./search";

const ThemeSwitcher = memo(lazy(() => import("./themeSwitcher")));
const LanguageSwitcher = memo(lazy(() => import("./languageSwitcher")));
const Manual = memo(lazy(() => import("./manual")));
const Notification = memo(lazy(() => import("./notification")));
const Accounts = memo(lazy(() => import("./accounts")));

export const Header = () => {
  const [searchOpen, setSearchOpen] = useState(false);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.metaKey || e.ctrlKey) && e.key === "k") {
        e.preventDefault();
        e.stopPropagation();
        setSearchOpen(true);
      }
    };
    window.addEventListener("keydown", handleKeyDown, { capture: true });
    return () => window.removeEventListener("keydown", handleKeyDown, { capture: true });
  }, []);

  return (
    <>
      <header className="relative z-50 h-12 border-b border-border bg-card/80 backdrop-blur-sm flex items-center px-4 gap-1 shrink-0 overflow-visible">
        <HeaderSearch onOpen={() => setSearchOpen(true)} />

        <div className="flex-1" />

        <HeaderActions />
      </header>

      {searchOpen ? (
        <div
          className="fixed inset-0 z-50 flex items-start justify-center bg-black/40 pt-24"
          onClick={() => setSearchOpen(false)}
          role="presentation"
        >
          <SearchDialog onClose={() => setSearchOpen(false)} />
        </div>
      ) : null}
    </>
  );
};

function HeaderActions() {
  return (
    <div className="relative flex items-center gap-0.5 overflow-visible isolate">
      <Suspense fallback={null}>
        <ThemeSwitcher />
        <Manual />
        <LanguageSwitcher />
        <Notification />
      </Suspense>
      <div className="h-5 w-px mx-1.5 bg-border shrink-0" role="separator" aria-orientation="vertical" />
      <Suspense fallback={null}>
        <Accounts />
      </Suspense>
    </div>
  );
}

function SearchDialog({ onClose }: { onClose: () => void }) {
  return (
    <div
      className="w-full max-w-lg mx-4 rounded-lg border border-border bg-card p-4 shadow-lg"
      onClick={(e) => e.stopPropagation()}
      role="dialog"
      aria-label="Search"
    >
      <p className="text-sm text-muted-foreground">Global search coming soon.</p>
      <button
        type="button"
        onClick={onClose}
        className="mt-3 text-xs text-primary hover:underline"
      >
        Close
      </button>
    </div>
  );
}

export default Header;
