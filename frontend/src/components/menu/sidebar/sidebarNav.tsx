import { useCallback, useMemo, useState } from "react";
import { LayoutDashboard, ChevronDown, PanelsTopLeft, type LucideIcon } from "lucide-react";
import { useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import type { ModuleModel } from "@/models";
import { useMenuModules } from "../hooks/useMenuModules";
import { resolveNavIcon } from "../utils/lucideIconMap";
import MenuNavLink from "../navLink";

interface SidebarNavProps {
  collapsed: boolean;
}

interface NavLinkDef {
  to: string;
  label: string;
  Icon: LucideIcon;
  end?: boolean;
}

interface NavGroupDef {
  key: string;
  label: string;
  Icon: LucideIcon;
  links: NavLinkDef[];
}

/**
 * The ONLY fixed entry is the app home. Every group and link below it is generated at runtime
 * from the coreSubsystem / coreModule / coreOperation tables (via GET /Module/WithOperations) —
 * there is no hardcoded menu. Configure it under System → Menu Modules / Menu Operations.
 */
const HOME_LINK: NavLinkDef = { to: "/", label: "Dashboard", Icon: LayoutDashboard, end: true };

const ROW_BASE =
  "group relative flex items-center rounded-lg text-[13px] font-medium text-sidebar-foreground transition-colors hover:bg-sidebar-accent hover:text-foreground";
const ROW_ACTIVE =
  "bg-primary/10 text-primary font-semibold before:absolute before:left-0 before:top-1/2 before:h-5 before:w-[3px] before:-translate-y-1/2 before:rounded-r-full before:bg-primary";

function NavLinkRow({ def, collapsed }: { def: NavLinkDef; collapsed: boolean }) {
  const { t } = useTranslation();
  const { to, label, Icon, end } = def;
  return (
    <MenuNavLink
      to={to}
      end={end}
      title={collapsed ? t(label) : undefined}
      className={`${ROW_BASE} ${collapsed ? "justify-center px-0 py-2.5" : "gap-2.5 px-3 py-2"}`}
      activeClassName={ROW_ACTIVE}
    >
      <Icon className="h-[18px] w-[18px] shrink-0" />
      {!collapsed && <span className="truncate">{t(label)}</span>}
    </MenuNavLink>
  );
}

function NavGroup({
  group,
  open,
  onToggle,
}: {
  group: NavGroupDef;
  open: boolean;
  onToggle: () => void;
}) {
  const { t } = useTranslation();
  const { Icon } = group;
  return (
    <div className="pt-2">
      <button
        type="button"
        onClick={onToggle}
        aria-expanded={open}
        className="flex w-full items-center gap-2 rounded-lg px-3 py-1.5 text-[10px] font-bold uppercase tracking-wider text-muted-foreground/70 transition-colors hover:text-foreground"
      >
        <Icon className="h-3.5 w-3.5 shrink-0" />
        <span className="truncate">{t(group.label)}</span>
        <ChevronDown
          className={`ml-auto h-3.5 w-3.5 shrink-0 transition-transform duration-200 ${open ? "" : "-rotate-90"}`}
        />
      </button>
      {open && (
        <div className="mt-0.5 space-y-0.5">
          {group.links.map((def) => (
            <NavLinkRow key={def.to} def={def} collapsed={false} />
          ))}
        </div>
      )}
    </div>
  );
}

/**
 * Maps the DB menu feed onto the sidebar's group shape: each coreModule becomes a collapsible
 * group, each of its role-visible coreOperation rows becomes a link. Icons are lucide-react
 * names stored on the rows. Order follows the SortOrder applied server-side.
 */
function buildDynamicGroups(modules: ModuleModel[] | undefined): NavGroupDef[] {
  return (modules ?? [])
    .map((m) => ({
      key: m.id ?? m.name ?? "",
      label: m.name ?? "",
      Icon: resolveNavIcon(m.icon),
      links: (m.operations ?? [])
        .filter((op) => op.canView !== false && op.link)
        .map((op) => ({
          to: op.link!.startsWith("/") ? op.link! : `/${op.link}`,
          label: op.name ?? "",
          Icon: resolveNavIcon(op.icon),
        })),
    }))
    .filter((g) => g.links.length > 0);
}

function SidebarNav({ collapsed }: SidebarNavProps) {
  const { t } = useTranslation();
  const { pathname } = useLocation();
  const { modules, isLoading } = useMenuModules();

  const navGroups = useMemo(() => buildDynamicGroups(modules), [modules]);

  const [closedGroups, setClosedGroups] = useState<Record<string, boolean>>(() => {
    try {
      return JSON.parse(localStorage.getItem("sidebarClosedGroups") || "{}");
    } catch {
      return {};
    }
  });

  const toggleGroup = useCallback((key: string) => {
    setClosedGroups((prev) => {
      const next = { ...prev, [key]: !prev[key] };
      try {
        localStorage.setItem("sidebarClosedGroups", JSON.stringify(next));
      } catch {
        /* ignore storage errors */
      }
      return next;
    });
  }, []);

  // A group stays open if it holds the active route, even when the user collapsed it.
  const groupHasActive = (group: NavGroupDef) =>
    group.links.some((l) => pathname === l.to || pathname.startsWith(`${l.to}/`));

  const showLoading = isLoading && navGroups.length === 0;
  const showEmpty = !isLoading && navGroups.length === 0;

  return (
    <nav className="sidebar-scroll flex-1 space-y-0.5 overflow-y-auto px-2.5 py-3">
      <NavLinkRow def={HOME_LINK} collapsed={collapsed} />

      {/* Menu still loading — placeholder rows so the sidebar doesn't flash empty. */}
      {showLoading && !collapsed && (
        <div className="space-y-2 px-1 pt-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="h-7 animate-pulse rounded-md bg-sidebar-accent/50" />
          ))}
        </div>
      )}

      {/* No rows in coreModule/coreOperation for this tenant — offer a bootstrap link (admins). */}
      {showEmpty && !collapsed && (
        <div className="px-1 pt-4 text-xs text-muted-foreground">
          <p className="mb-2 px-2">{t("No menu has been configured yet.")}</p>
          <MenuNavLink
            to="/module"
            className={`${ROW_BASE} gap-2.5 px-3 py-2`}
            activeClassName={ROW_ACTIVE}
          >
            <PanelsTopLeft className="h-[18px] w-[18px] shrink-0" />
            <span className="truncate">{t("Set up menu")}</span>
          </MenuNavLink>
        </div>
      )}

      {/* Collapsed rail: a flat, icon-only list with dividers between groups. */}
      {collapsed &&
        navGroups.map((group) => (
          <div key={group.key}>
            <div className="mx-2 my-2 border-t border-sidebar-border" aria-hidden />
            {group.links.map((def) => (
              <NavLinkRow key={def.to} def={def} collapsed />
            ))}
          </div>
        ))}

      {/* Expanded: collapsible grouped sections, one per module. */}
      {!collapsed &&
        navGroups.map((group) => (
          <NavGroup
            key={group.key}
            group={group}
            open={!closedGroups[group.key] || groupHasActive(group)}
            onToggle={() => toggleGroup(group.key)}
          />
        ))}
    </nav>
  );
}

export default SidebarNav;
