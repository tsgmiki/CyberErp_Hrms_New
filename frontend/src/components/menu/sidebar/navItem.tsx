import { useTranslation } from "react-i18next";
import MenuNavLink from "../navLink";
import type { SidebarNavItem } from "../utils/menuTypes";

interface NavItemProps {
  item: SidebarNavItem;
  collapsed?: boolean;
}

function NavItem({ item, collapsed }: NavItemProps) {
  const { t } = useTranslation();

  if (collapsed) {
    return (
      <MenuNavLink
        to={item.path}
        end
        title={t(item.title)}
        className="flex items-center justify-center p-2 rounded-lg text-sidebar-foreground hover:text-foreground hover:bg-sidebar-accent transition-colors"
        activeClassName="bg-sidebar-operation-active font-semibold text-primary"
      >
        <span className="[&>svg]:w-4 [&>svg]:h-4">{item.icon}</span>
      </MenuNavLink>
    );
  }

  return (
    <MenuNavLink
      to={item.path}
      end
      className="flex items-center gap-2.5 px-3 py-[7px] rounded-lg text-[13px] text-sidebar-foreground hover:text-foreground hover:bg-sidebar-accent transition-all duration-150"
      activeClassName="bg-sidebar-operation-active font-semibold text-primary"
    >
      <span className="shrink-0 [&>svg]:w-3.5 [&>svg]:h-3.5">{item.icon}</span>
      <span className="truncate">{t(item.title)}</span>
    </MenuNavLink>
  );
}

export default NavItem;
