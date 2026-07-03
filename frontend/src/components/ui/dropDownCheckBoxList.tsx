"use client";
import type { DataTableColumnModel } from "@/models";
import { type ReactNode, useEffect, useRef, useState } from "react";
import { v6 as uuid } from "uuid";
import ButtonField from "./buttonField";
import { useTranslation } from "react-i18next";

interface Props {
  value?: string;
  icon: ReactNode;
  className?: string;
  iconClassName?: string;
  htmlType?: "submit" | "button" | "reset" | undefined;
  type?: "primary";
  disabled: boolean;
  onSelect?: Function;
  menu: any[];
  selectedMenu: any[];
}

const CheckBoxList = (props: Props) => {
  const {
    value,
    icon,
    className = defaultProps.className,
    onSelect,
    selectedMenu,
    disabled = defaultProps.disabled,
    menu,
  } = props;
  const [open, setOpen] = useState(false);
  const dropdown = useRef(null);
  const [, setCurrentValues] = useState([] as DataTableColumnModel[]);
  const { t } = useTranslation();
  const handleOpen = () => {
    setOpen(!open);
  };
  useEffect(() => {
    const clickHandler = (pros: { target: any }) => {
      if (
        !open ||
        (typeof dropdown != "undefined" &&
          dropdown != null &&
          typeof dropdown?.current != "undefined" &&
          dropdown?.current != null &&
          (dropdown?.current as any).contains(pros.target))
      )
        return;

      setOpen(false);
    };
    document.addEventListener("click", clickHandler);
    return () => document.removeEventListener("click", clickHandler);
  });
  useEffect(() => {
    if (typeof value == "undefined" || value == "" || value == null)
      setCurrentValues([]);
    else {
      const idList = (value as string)?.split(",");
      const valueList = menu
        ?.filter((a: any) => idList.filter((b) => b == a.name).length > 0)
        .map((item: any) => {
          return item;
        });
      setCurrentValues(valueList as never);
    }
  }, [value, menu]);
  return (
    <div className="dropdown relative">
      <ButtonField
        className={className}
        onClick={handleOpen}
        disabled={disabled}
        icon={icon}
        value={t(value as string)}
      ></ButtonField>
          {open ? (
        <ul
          ref={dropdown}
          className="absolute top-full left-0 mt-1 font-normal z-50 min-w-40 rounded-md border border-border bg-card max-h-60 overflow-auto shadow-lg"
        >
          {menu?.map((item) => (
            <li key={uuid()} className="flex gap-2 p-2 hover:bg-secondary/20 cursor-pointer">
              <input
                type={"checkbox"}
                className={"cursor-pointer accent-primary"}
                checked={
                  selectedMenu?.filter((a) => a.name == item.name).length > 0
                }
                onChange={(e) => {
                  if (e.target.checked == true) {
                    onSelect?.(item, true);
                  } else if (e.target.checked == false) {
                    onSelect?.(item, false);
                  }
                }}
              />
              <span className={"text-sm font-normal text-foreground"}>
                {item.label}
              </span>
            </li>
          ))}
        </ul>
      ) : null}
    </div>
  );
};
const defaultProps = {
  disabled: false,
  className: "max-md:w-full border border-primary bg-primary text-on-accent hover:bg-primary-hover",
};

export default CheckBoxList;
