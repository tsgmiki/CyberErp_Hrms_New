import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { Building2 } from "lucide-react";

/*
 * Enterprise auth shell — the SAP IAS / Microsoft (Azure AD, Dynamics 365) login anatomy:
 * a full-viewport BRANDED BACKDROP (deep primary gradient, quiet geometry + dot grid), the
 * product mark top-left, ONE ELEVATED SIGN-IN CARD centered on it (brand accent bar, in-card
 * product mark, heading, the form, a divided footer note), and a slim legal footer at the
 * bottom. No marketing panels — the card is the entire experience.
 * Pages compose it unchanged (children = the FormProvider form — pass `frameless: true` so it
 * doesn't draw its own field frame; footer = the note below the form; `title`/`subtitle` = the
 * card heading).
 *
 * SHARED DESIGN: this is intentionally identical to the Home portal's auth shell
 * (`Home/frontend/src/components/auth/authLayout/authLayout.tsx`) so every CyberERP subsystem
 * presents one sign-in experience. The brand text comes from each app's own `BrandPrefix` /
 * `BrandAccent` translations, so HRMS reads "CyberHRMS" where the portal reads "CyberHome" —
 * same layout, correct identity. Keep the two files in step when either changes.
 */

interface AuthLayoutProps {
  children: ReactNode;
  maxWidth?: "sm" | "md" | "lg";
  footer?: ReactNode;
  title?: string;
  subtitle?: string;
}

const widthClasses = {
  sm: "max-w-[440px]",
  md: "max-w-2xl",
  lg: "max-w-3xl",
};

function AuthLayout({ children, maxWidth = "sm", footer, title, subtitle }: AuthLayoutProps) {
  const { t } = useTranslation();
  const year = new Date().getFullYear();

  return (
    <div
      className="relative flex min-h-screen flex-col overflow-hidden"
      style={{
        background:
          "linear-gradient(165deg, var(--primary) 0%, color-mix(in srgb, var(--primary-hover) 72%, #000) 100%)",
      }}
    >
      {/* Backdrop geometry — soft glows, a faint dot grid, thin outlined circles. No imagery. */}
      <div
        aria-hidden
        className="pointer-events-none absolute inset-0"
        style={{
          background:
            "radial-gradient(900px 480px at 85% -10%, rgba(255,255,255,0.09), transparent 60%), radial-gradient(700px 420px at -10% 110%, rgba(255,255,255,0.06), transparent 60%)",
        }}
      />
      <div
        aria-hidden
        className="pointer-events-none absolute inset-0 opacity-[0.22]"
        style={{
          backgroundImage: "radial-gradient(rgba(255,255,255,0.35) 1px, transparent 1px)",
          backgroundSize: "26px 26px",
          maskImage: "radial-gradient(ellipse 70% 60% at 50% 40%, black, transparent)",
          WebkitMaskImage: "radial-gradient(ellipse 70% 60% at 50% 40%, black, transparent)",
        }}
      />
      <div aria-hidden className="pointer-events-none absolute -right-40 top-1/4 h-[420px] w-[420px] rounded-full border border-white/10" />
      <div aria-hidden className="pointer-events-none absolute -left-24 -bottom-24 h-72 w-72 rounded-full border border-white/[0.08]" />

      {/* Product mark — top-left, SAP placement */}
      <header className="relative z-10 flex items-center gap-2.5 px-8 py-6">
        <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-white/15 backdrop-blur-sm">
          <Building2 className="h-4.5 w-4.5 text-white" />
        </span>
        <span className="font-display text-lg font-bold tracking-tight text-white">
          {t("BrandPrefix")}
          <span className="text-white/70">{t("BrandAccent")}</span>
        </span>
        <span className="ml-2 hidden border-l border-white/20 pl-2.5 text-[11px] font-medium uppercase tracking-[0.14em] text-white/55 sm:inline">
          {t("Enterprise Resource Planning")}
        </span>
      </header>

      {/* The sign-in card */}
      <main className="relative z-10 flex flex-1 items-center justify-center px-4 py-8">
        <div
          className={`relative w-full ${widthClasses[maxWidth]} overflow-hidden rounded-2xl bg-card shadow-2xl ring-1 ring-black/5`}
        >
          {/* Brand accent bar — the card's signature edge. */}
          <span aria-hidden className="absolute inset-x-0 top-0 h-1 bg-primary" />

          <div className="p-8 sm:p-9">
            {/* In-card product mark (Microsoft-style) — the identity travels with the card. */}
            <div className="mb-6 flex items-center gap-2.5">
              <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary text-white">
                <Building2 className="h-4.5 w-4.5" />
              </span>
              <span className="font-display text-[15px] font-bold tracking-tight text-foreground">
                {t("BrandPrefix")}
                <span className="text-primary">{t("BrandAccent")}</span>
              </span>
            </div>

            {(title || subtitle) && (
              <div className="mb-6">
                {title && (
                  <h1 className="font-display text-[22px] font-bold tracking-tight text-foreground">
                    {title}
                  </h1>
                )}
                {subtitle && <p className="mt-1.5 text-[13px] text-muted-foreground">{subtitle}</p>}
              </div>
            )}

            {children}

            {footer && <div className="mt-6 border-t border-border pt-4">{footer}</div>}
          </div>
        </div>
      </main>

      {/* Slim legal footer */}
      <footer className="relative z-10 px-4 pb-5 text-center text-[11px] text-white/55">
        © {year} {t("BrandPrefix")}{t("BrandAccent")} · {t("Enterprise Resource Planning")} ·{" "}
        {t("Cyber HRMS v1.0")}
      </footer>
    </div>
  );
}

export default AuthLayout;
