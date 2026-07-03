import type { ModuleModel } from "@/models";
import Tooltip from "../common/toolTip/tooltip";
import { icons } from "./icons";
import { LayoutDashboardIcon, ChevronDown, Circle } from "lucide-react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";

export default function Modules(props: {
  modules: ModuleModel[];
  activeIndex: number;
  setActiveIndex: Function;
}) {
  const { t } = useTranslation();
  const { modules, activeIndex, setActiveIndex } = props;

  const toggleModule = (index: number) => {
    setActiveIndex(index === activeIndex ? -1 : index);
  };

  return (
    <div className="flex flex-col px-2 py-1">
      {/* dashboard */}
      <Link
        to="/"
        onClick={() => toggleModule(-1)}
        className={`flex items-center gap-3 px-4 py-2.5 mx-1 rounded-lg transition-all duration-200 ${
          activeIndex === -1
            ? "bg-primary text-on-accent shadow-md"
            : "text-foreground hover:bg-secondary"
        }`}
      >
        <LayoutDashboardIcon size={16} />
        <span className="text-[13px] font-semibold tracking-wide">{t("Dashboard")}</span>
      </Link>

      <div className="my-2 mx-3 border-t border" />

      {/* Modules */}
      {modules?.map((item, index) => {
        const isActive = activeIndex === index;
        const itemIcon = icons.find((a) => a.name === item.name)?.icon;

        return (
          <div key={item.id || index}>
            <button
              onClick={() => toggleModule(index)}
              className={`flex items-center justify-between w-full px-4 py-2.5 mx-1 rounded-lg transition-all duration-200 ${
                isActive
                  ? "bg-primary text-on-accent shadow-md"
                  : "text-foreground hover:bg-secondary"
              }`}
            >
              <div className="flex items-center gap-3">
                <ChevronDown
                  size={14}
                  className={`transition-transform duration-200 ${
                    isActive ? "rotate-180" : ""
                  } ${isActive ? "text-on-accent" : "text-foreground"}`}
                />
                <div
                  className={`transition-colors duration-200 ${
                    isActive ? "text-on-accent" : "text-foreground"
                  }`}
                >
                  <Tooltip message={t(item.name || '')}>{itemIcon}</Tooltip>
                </div>
                <span className="text-[13px] font-semibold tracking-wide">{t(item.name || '')}</span>
              </div>
            </button>

            {/* Dropdown Menu */}
            <div
              className={`overflow-hidden transition-all duration-300 ease-in-out ${
                isActive ? "max-h-screen opacity-100" : "max-h-0 opacity-0"
              }`}
            >
              <div className="flex flex-col py-1">
                {item.operations?.map((operation: any, opIndex: number) => {
                  return (
                    <Link
                      key={operation.id || opIndex}
                      to={"/" + operation.link}
                      className="flex items-center gap-3 pl-12 pr-4 py-1.5 mx-1 rounded-md text-[12.5px] transition-all duration-200 font-medium tracking-wide text-foreground hover:text-primary"
                    >
                      <Circle size={6} className="shrink-0 fill-current" />
                      <span className="truncate">{t(operation?.name || '')}</span>
                    </Link>
                  );
                })}
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
}
