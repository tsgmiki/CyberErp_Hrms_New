import Sidebar from "./sidebar";
import HamburgerItem from "./hamburgerItem";

interface MenuProps {
  collapsed: boolean;
  onToggle: () => void;
}

function Menu({ collapsed, onToggle }: MenuProps) {
  return (
    <>
      <Sidebar collapsed={collapsed} onToggle={onToggle} />
      <HamburgerItem />
    </>
  );
}

export default Menu;
