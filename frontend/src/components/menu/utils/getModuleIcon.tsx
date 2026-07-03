import type { ReactNode } from "react";
import { Circle } from "lucide-react";
import { icons } from "../icons";

export function getModuleIcon(moduleName?: string): ReactNode {
  const match = icons.find((entry) => entry.name === moduleName);
  return match?.icon ?? <Circle className="w-4 h-4" />;
}

export function getOperationIcon(operationName?: string): ReactNode {
  for (const entry of icons) {
    const detail = entry.details?.find((item) => item.name === operationName);
    if (detail?.icon) return detail.icon;
  }

  return <Circle className="w-3.5 h-3.5" />;
}
