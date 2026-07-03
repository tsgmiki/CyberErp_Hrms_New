import Sidebar from "./sidebar";
import HamburgerItem from "./hamburgerItem";
import { useMenuModules } from "./hooks/useMenuModules";

interface MenuProps {
  collapsed: boolean;
  onToggle: () => void;
}

function Menu({ collapsed, onToggle }: MenuProps) {
  const { navigation, isLoading } = useMenuModules();

  return (
    <>
      <Sidebar collapsed={collapsed} onToggle={onToggle} />
      {!isLoading && <HamburgerItem navigation={navigation} />}
    </>
  );
}

export default Menu;
