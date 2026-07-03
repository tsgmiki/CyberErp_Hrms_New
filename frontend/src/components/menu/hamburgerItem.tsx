import Hamburger from "./hamburger";
import SidebarNav from "./sidebar/sidebarNav";
import { useExpandedCategories } from "./hooks/useExpandedCategories";
import type { SidebarNavigationModel } from "./utils/menuTypes";

interface HamburgerItemProps {
  navigation: SidebarNavigationModel;
}

function HamburgerItem({ navigation }: HamburgerItemProps) {
  const { isCategoryExpanded, toggleCategory } = useExpandedCategories(navigation);

  return (
    <div className="z-50 md:hidden absolute top-3 left-3">
      <Hamburger>
        <div className="w-64 max-h-[80vh] overflow-y-auto sidebar-scroll rounded-lg border border-border bg-sidebar p-2 shadow-lg">
          <SidebarNav
            collapsed={false}
            navigation={navigation}
            isCategoryExpanded={isCategoryExpanded}
            toggleCategory={toggleCategory}
          />
        </div>
      </Hamburger>
    </div>
  );
}

export default HamburgerItem;
