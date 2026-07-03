import { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import type { SidebarNavigationModel } from "../utils/menuTypes";

export function useExpandedCategories(navigation: SidebarNavigationModel) {
  const location = useLocation();
  const [expandedCategories, setExpandedCategories] = useState<string[]>([]);

  useEffect(() => {
    const keys: string[] = [];

    navigation.entries.forEach((entry) => {
      entry.categories.forEach((category) => {
        if (category.items.some((item) => location.pathname === item.path)) {
          keys.push(category.key);
        }
      });
    });

    setExpandedCategories((prev) => Array.from(new Set([...prev, ...keys])));
  }, [location.pathname, navigation]);

  const toggleCategory = (key: string) => {
    setExpandedCategories((prev) =>
      prev.includes(key) ? prev.filter((item) => item !== key) : [...prev, key],
    );
  };

  const isCategoryExpanded = (key: string) => expandedCategories.includes(key);

  return {
    toggleCategory,
    isCategoryExpanded,
  };
}
