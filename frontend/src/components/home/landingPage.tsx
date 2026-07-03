import type { ModuleModel } from "@/models";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Building2, ChevronRight, LogOut } from "lucide-react";
import { useMemo, useState } from "react";
import store from "@/store";
import { useAuth } from "@/context/AuthContext";
import BrandTitle from "@/components/common/brand/brandTitle";
import {
  buildLandingSubsystems,
  type LandingSubsystemCard,
} from "./buildLandingSubsystems";

interface LandingPageProps {
  modules: ModuleModel[];
}

function LandingHeader() {
  const { user, logout } = useAuth();
  const { t } = useTranslation();

  const initials =
    user?.fullName
      ?.split(" ")
      .map((n) => n[0])
      .join("")
      .toUpperCase()
      .slice(0, 2) || "U";

  return (
    <header className="border-b border-border bg-card sticky top-0 z-10">
      <div className="max-w-7xl mx-auto px-6 h-14 flex items-center justify-between">
        <div className="flex items-center gap-2.5">
          <div className="w-8 h-8 rounded-lg bg-primary flex items-center justify-center shadow-sm">
            <Building2 className="w-4 h-4 text-primary-foreground" />
          </div>
          <BrandTitle size="sm" />
        </div>
        <div className="flex items-center gap-3">
          <div className="hidden sm:flex items-center gap-2">
            <div className="w-7 h-7 rounded-full bg-primary text-primary-foreground flex items-center justify-center text-xs font-semibold">
              {initials}
            </div>
            <span className="text-sm text-foreground font-medium">{user?.fullName}</span>
          </div>
          <button
            type="button"
            onClick={logout}
            className="inline-flex items-center h-8 px-3 rounded-md text-sm text-muted-foreground hover:text-foreground hover:bg-accent transition-colors"
          >
            <LogOut className="w-4 h-4 mr-1.5" />
            {t("SignOut", { defaultValue: "Logout" })}
          </button>
        </div>
      </div>
    </header>
  );
}

function SubsystemCard({
  card,
  onSelect,
}: {
  card: LandingSubsystemCard;
  onSelect: (id: string) => void;
}) {
  const { t } = useTranslation();
  const preview = card.previewItems.slice(0, 3);
  const remaining = card.totalItemCount - preview.length;

  return (
    <button
      type="button"
      onClick={() => onSelect(card.id)}
      className="group relative p-5 rounded-xl border border-border bg-card hover:border-primary/30 hover:shadow-lg transition-all duration-200 text-left"
    >
      <div className="flex items-start gap-4">
        <div className="w-11 h-11 rounded-xl bg-primary/5 flex items-center justify-center text-primary group-hover:bg-primary group-hover:text-primary-foreground transition-all duration-200 shrink-0 shadow-sm [&>svg]:w-5 [&>svg]:h-5">
          {card.icon}
        </div>
        <div className="flex-1 min-w-0">
          <div className="flex items-center justify-between gap-2">
            <h3 className="font-display font-semibold text-foreground">{t(card.title)}</h3>
            <ChevronRight className="w-4 h-4 text-muted-foreground/40 group-hover:text-primary group-hover:translate-x-0.5 transition-all shrink-0" />
          </div>
          <p className="text-[13px] text-muted-foreground leading-relaxed mt-1 line-clamp-2">
            {card.description}
          </p>
          {preview.length > 0 && (
            <div className="mt-3 flex gap-1.5 flex-wrap">
              {preview.map((item) => (
                <span
                  key={item}
                  className="text-[11px] px-2 py-0.5 rounded-full bg-muted text-muted-foreground font-medium"
                >
                  {t(item)}
                </span>
              ))}
              {remaining > 0 && (
                <span className="text-[11px] px-2 py-0.5 rounded-full bg-primary/5 text-primary font-medium">
                  +{remaining} {t("More", { defaultValue: "more" })}
                </span>
              )}
            </div>
          )}
        </div>
      </div>
    </button>
  );
}

export default function LandingPage({ modules }: LandingPageProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { user } = useAuth();
  const [searchTerm, setSearchTerm] = useState("");

  const subsystems = useMemo(() => buildLandingSubsystems(modules), [modules]);

  const filteredSubsystems = useMemo(() => {
    if (!searchTerm.trim()) return subsystems;
    const term = searchTerm.toLowerCase();
    return subsystems.filter(
      (card) =>
        card.title.toLowerCase().includes(term) ||
        card.description.toLowerCase().includes(term) ||
        card.previewItems.some((item) => item.toLowerCase().includes(term)),
    );
  }, [subsystems, searchTerm]);

  const firstName = user?.fullName?.split(" ")[0] ?? t("User", { defaultValue: "User" });

  const handleSelectSubsystem = (subsystem: string) => {
    store.ModuleData.value = { name: subsystem };
    navigate("/");
  };

  return (
    <div className="min-h-screen bg-background flex flex-col">
      <LandingHeader />

      <main className="flex-1 max-w-7xl mx-auto px-6 py-10 w-full">
        <div className="mb-8">
          <p className="text-muted-foreground text-sm mb-1">
            {t("WelcomeBack", { defaultValue: "Welcome back" })}, {firstName}
          </p>
          <h1 className="text-2xl font-display font-bold text-foreground">
            {t("SelectModule", { defaultValue: "Select a Module" })}
          </h1>
        </div>

        {subsystems.length > 3 && (
          <div className="mb-6 max-w-md">
            <input
              type="search"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder={t("SearchModules", { defaultValue: "Search modules..." })}
              className="w-full h-9 px-3 rounded-lg border border-border bg-card text-sm text-foreground placeholder:text-muted-foreground outline-none focus:border-primary/50 focus:ring-2 focus:ring-primary/10"
            />
          </div>
        )}

        {filteredSubsystems.length === 0 ? (
          <div className="rounded-xl border border-border bg-card px-6 py-16 text-center">
            <p className="text-sm text-muted-foreground">
              {searchTerm
                ? t("NoModulesFound", { defaultValue: "No modules match your search." })
                : t("NoModulesAvailable", { defaultValue: "No modules are available." })}
            </p>
            {searchTerm ? (
              <button
                type="button"
                onClick={() => setSearchTerm("")}
                className="mt-3 text-sm font-medium text-primary hover:underline"
              >
                {t("ClearSearch", { defaultValue: "Clear search" })}
              </button>
            ) : null}
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {filteredSubsystems.map((card) => (
              <SubsystemCard key={card.id} card={card} onSelect={handleSelectSubsystem} />
            ))}
          </div>
        )}
      </main>
    </div>
  );
}
