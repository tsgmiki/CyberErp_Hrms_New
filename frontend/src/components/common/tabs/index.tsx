import Tab from "@/components/ui/tab";
import type { ReactNode } from "react";
import { useState } from "react";

export interface TabItem {
  id: number;
  label: ReactNode;
  content?: ReactNode;
  /** Optional count badge on the tab */
  count?: number;
  disabled?: boolean;
}

interface TabsProps {
  dir: "top" | "left";
  tabs: TabItem[];
  activeTab?: number;
  onTabClick?: (id: number) => void;
  className?: string;
}

function Tabs({
  tabs,
  dir,
  activeTab: controlledActiveTab,
  onTabClick,
  className = "",
}: TabsProps) {
  const [internalActiveTab, setInternalActiveTab] = useState(tabs[0]?.id ?? 1);

  const isControlled =
    controlledActiveTab !== undefined && onTabClick !== undefined;
  const activeTab = isControlled ? controlledActiveTab : internalActiveTab;

  const handleTabClick = (index: number, disabled?: boolean) => {
    if (disabled) return;
    if (isControlled) {
      onTabClick(index);
    } else {
      setInternalActiveTab(index);
    }
  };

  const tabList = (
    <div
      className={`flex shrink-0 gap-1 overflow-x-auto rounded-lg border border-border/80 bg-[color-mix(in_srgb,var(--secondary)_45%,var(--card))] p-1 ${
        dir === "left" ? "lg:flex-col lg:overflow-visible" : ""
      }`}
      role="tablist"
    >
      {tabs?.map((tab) => (
        <Tab
          key={tab.id}
          label={tab.label}
          count={tab.count}
          disabled={tab.disabled}
          onClick={() => handleTabClick(tab.id, tab.disabled)}
          isActive={tab.id === activeTab}
        />
      ))}
    </div>
  );

  const tabPanels = (
    <div className="min-h-[12rem] flex-1">
      {tabs?.map((tab) => (
        <div
          key={tab.id}
          role="tabpanel"
          hidden={tab.id !== activeTab}
          className={
            tab.id === activeTab
              ? "block animate-in fade-in duration-200"
              : "hidden"
          }
        >
          {tab.content}
        </div>
      ))}
    </div>
  );

  return (
    <div
      className={`flex flex-col ${dir === "left" ? "lg:flex-row lg:gap-4" : "gap-3"} ${className}`}
    >
      {dir === "left" ? (
        <>
          {tabList}
          <div className="min-w-0 flex-1 rounded-xl border border-border/80 bg-card p-0">
            {tabPanels}
          </div>
        </>
      ) : (
        <>
          {tabList}
          <div className="rounded-xl border border-border/80 bg-transparent p-0">
            {tabPanels}
          </div>
        </>
      )}
    </div>
  );
}

export default Tabs;
