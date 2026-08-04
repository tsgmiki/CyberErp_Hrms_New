import { memo } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Briefcase, Building, CalendarClock, ChevronRight, GitPullRequestArrow, Network, Users } from "lucide-react";
import { Card } from "./shared";

const LINKS = [
  { to: "/employee", label: "Employees", Icon: Users },
  { to: "/leaveRequest", label: "Leave Requests", Icon: CalendarClock },
  { to: "/workflow", label: "Workflow Tracking", Icon: GitPullRequestArrow },
  { to: "/organizationUnit", label: "Organization Structure", Icon: Network },
  { to: "/position", label: "Positions", Icon: Briefcase },
  { to: "/branch", label: "Branches", Icon: Building },
] as const;

/** Static nav shortcuts — no data fetching at all, so memo() guarantees it never re-renders post-mount. */
function QuickAccessWidget() {
  const { t } = useTranslation();
  return (
    <Card title={t("Quick Access", "Quick Access")} icon={<ChevronRight className="h-4 w-4" />}>
      <nav className="p-2">
        {LINKS.map(({ to, label, Icon }) => (
          <Link
            key={to}
            to={to}
            className="flex items-center gap-2.5 rounded-lg px-2.5 py-2 text-[13px] text-foreground transition-colors hover:bg-secondary"
          >
            <Icon className="h-4 w-4 text-muted" />
            {t(label)}
            <ChevronRight className="ml-auto h-3.5 w-3.5 text-muted/60" />
          </Link>
        ))}
      </nav>
    </Card>
  );
}

export default memo(QuickAccessWidget);
