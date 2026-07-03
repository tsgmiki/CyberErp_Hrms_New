import type { ReactNode } from "react";

export interface SidebarNavItem {
  id: string;
  title: string;
  path: string;
  icon: ReactNode;
}

export interface SidebarCategory {
  key: string;
  category: string;
  items: SidebarNavItem[];
}

export interface SidebarModuleEntry {
  id: string;
  title: string;
  icon: ReactNode;
  categories: SidebarCategory[];
  flatItems: SidebarNavItem[];
}

export interface SidebarNavigationModel {
  selectedSubsystem?: string;
  selectedSubsystemIcon?: ReactNode;
  isSubsystemSelected: boolean;
  entries: SidebarModuleEntry[];
}
