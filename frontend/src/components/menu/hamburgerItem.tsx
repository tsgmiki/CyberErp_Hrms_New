import Hamburger from "./hamburger";
import SidebarNav from "./sidebar/sidebarNav";

function HamburgerItem() {
  return (
    <div className="z-50 md:hidden absolute top-3 left-3">
      <Hamburger>
        <div className="w-64 max-h-[80vh] overflow-y-auto sidebar-scroll rounded-lg border border-border bg-sidebar p-2 shadow-lg">
          <SidebarNav collapsed={false} />
        </div>
      </Hamburger>
    </div>
  );
}

export default HamburgerItem;
