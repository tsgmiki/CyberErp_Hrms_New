import { useCallback, useState } from "react";
import {
  LayoutDashboard,
  Network,
  Briefcase,
  BriefcaseBusiness,
  Layers,
  Coins,
  Tags,
  MapPin,
  CalendarDays,
  CalendarClock,
  CalendarRange,
  CalendarCheck,
  Wallet,
  CalendarCog,
  SlidersHorizontal,
  BookOpenCheck,
  Building,
  ScrollText,
  Users,
  ListPlus,
  FileText,
  UsersRound,
  Building2,
  ShieldCheck,
  ChevronDown,
  GitPullRequestArrow,
  GitBranch,
  UserCog,
  KeyRound,
  UserCheck,
  UserX,
  ClipboardCheck,
  Target,
  ClipboardList,
  LayoutGrid,
  UserPlus,
  FilePlus2,
  Megaphone,
  Star,
  type LucideIcon,
} from "lucide-react";
import { useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import store from "@/store";
import type { SidebarNavigationModel } from "../utils/menuTypes";
import CategoryGroup from "./categoryGroup";
import ModuleListItem from "./moduleListItem";
import NavItem from "./navItem";
import MenuNavLink from "../navLink";

interface SidebarNavProps {
  collapsed: boolean;
  navigation: SidebarNavigationModel;
  isCategoryExpanded: (key: string) => boolean;
  toggleCategory: (key: string) => void;
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

/** Primary navigation. Grouped statically until the Module/Operation menu API exists. */
const MAIN_LINKS: NavLinkDef[] = [
  { to: "/", label: "Dashboard", Icon: LayoutDashboard, end: true },
];

const NAV_GROUPS: NavGroupDef[] = [
  {
    key: "personnel",
    label: "Personnel",
    Icon: UsersRound,
    links: [
      { to: "/employee", label: "Employees", Icon: Users },
      { to: "/terminationList", label: "Termination List", Icon: UserX },
      { to: "/employeeField", label: "Employee Fields", Icon: ListPlus },
      { to: "/documentTemplate", label: "Document Templates", Icon: FileText },
    ],
  },
  {
    key: "organization",
    label: "Organization",
    Icon: Building2,
    links: [
      { to: "/branch", label: "Branches", Icon: Building },
      { to: "/organizationUnit", label: "Organization Structure", Icon: Network },
      { to: "/positionClass", label: "Position Classes", Icon: BriefcaseBusiness },
      { to: "/position", label: "Positions", Icon: Briefcase },
      { to: "/jobGrade", label: "Job Grades", Icon: Layers },
      { to: "/salaryScale", label: "Salary Scale", Icon: Coins },
      { to: "/jobCategory", label: "Job Categories", Icon: Tags },
      { to: "/workLocation", label: "Work Locations", Icon: MapPin },
    ],
  },
  {
    key: "planning",
    label: "Planning",
    Icon: Target,
    links: [
      { to: "/workforcePlan", label: "Workforce Plans", Icon: ClipboardList },
      { to: "/establishmentOverview", label: "Establishment Overview", Icon: LayoutGrid },
    ],
  },
  {
    key: "recruitment",
    label: "Recruitment",
    Icon: UserPlus,
    links: [
      { to: "/hiringRequest", label: "Hiring Requests", Icon: FilePlus2 },
      { to: "/jobRequisition", label: "Job Requisitions", Icon: Megaphone },
      { to: "/candidate", label: "Candidates", Icon: Users },
      { to: "/jobApplication", label: "Applications", Icon: ClipboardList },
      { to: "/hireEmployee", label: "Hire Employee", Icon: UserCheck },
      { to: "/talentPool", label: "Talent Pool", Icon: Star },
    ],
  },
  {
    key: "attendanceLeave",
    label: "Attendance & Leave",
    Icon: CalendarRange,
    links: [
      { to: "/leaveRequest", label: "Leave Requests", Icon: CalendarCheck },
      { to: "/leaveBalance", label: "Leave Balances", Icon: Wallet },
      { to: "/annualLeaveLedger", label: "Annual Leave Ledger", Icon: BookOpenCheck },
      { to: "/leaveType", label: "Leave Types", Icon: CalendarDays },
      { to: "/annualLeaveSetting", label: "Leave Settings", Icon: SlidersHorizontal },
      { to: "/holiday", label: "Holidays", Icon: CalendarClock },
      { to: "/fiscalYear", label: "Fiscal Years", Icon: CalendarCog },
    ],
  },
  {
    key: "system",
    label: "System",
    Icon: ShieldCheck,
    links: [
      { to: "/workflow", label: "Workflow Tracking", Icon: GitPullRequestArrow },
      { to: "/workflowDefinition", label: "Workflow Definitions", Icon: GitBranch },
      { to: "/clearanceDepartment", label: "Clearance Departments", Icon: ClipboardCheck },
      { to: "/user", label: "Users", Icon: UserCog },
      { to: "/role", label: "Roles", Icon: KeyRound },
      { to: "/userRole", label: "User Roles", Icon: UserCheck },
      { to: "/auditLog", label: "Audit Trail", Icon: ScrollText },
    ],
  },
];

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

function SidebarNav({
  collapsed,
  navigation,
  isCategoryExpanded,
  toggleCategory,
}: SidebarNavProps) {
  const { isSubsystemSelected, entries } = navigation;
  const { pathname } = useLocation();

  const collapsedItems = entries.flatMap((entry) => entry.flatItems);

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

  const handleSelectSubsystem = (subsystem: string) => {
    store.ModuleData.value = { name: subsystem };
  };

  // A group stays open if it holds the active route, even when the user collapsed it.
  const groupHasActive = (group: NavGroupDef) =>
    group.links.some((l) => pathname === l.to || pathname.startsWith(`${l.to}/`));

  return (
    <nav className="sidebar-scroll flex-1 space-y-0.5 overflow-y-auto px-2.5 py-3">
      {MAIN_LINKS.map((def) => (
        <NavLinkRow key={def.to} def={def} collapsed={collapsed} />
      ))}

      {/* Collapsed rail: a flat, icon-only list with dividers between groups. */}
      {collapsed &&
        NAV_GROUPS.map((group) => (
          <div key={group.key}>
            <div className="mx-2 my-2 border-t border-sidebar-border" aria-hidden />
            {group.links.map((def) => (
              <NavLinkRow key={def.to} def={def} collapsed />
            ))}
          </div>
        ))}

      {/* Expanded: collapsible grouped sections. */}
      {!collapsed &&
        NAV_GROUPS.map((group) => (
          <NavGroup
            key={group.key}
            group={group}
            open={!closedGroups[group.key] || groupHasActive(group)}
            onToggle={() => toggleGroup(group.key)}
          />
        ))}

      {/* Dynamic module navigation (populated once the Module/Operation API exists). */}
      {collapsed && isSubsystemSelected && (
        <div className="space-y-0.5 pt-1">
          {collapsedItems.map((item) => (
            <NavItem key={item.id} item={item} collapsed />
          ))}
        </div>
      )}

      {!collapsed &&
        entries.map((entry) => {
          const showCategories = isSubsystemSelected;
          return (
            <div key={entry.id}>
              {!showCategories && (
                <ModuleListItem
                  entry={entry}
                  collapsed={collapsed}
                  onSelectSubsystem={!isSubsystemSelected ? handleSelectSubsystem : undefined}
                />
              )}
              {showCategories && (
                <div className="space-y-0.5">
                  {entry.categories.map((category) => (
                    <CategoryGroup
                      key={category.key}
                      category={category}
                      expanded={isCategoryExpanded(category.key)}
                      onToggle={() => toggleCategory(category.key)}
                    />
                  ))}
                </div>
              )}
            </div>
          );
        })}
    </nav>
  );
}

export default SidebarNav;
