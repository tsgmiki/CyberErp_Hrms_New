import { useSignals } from "@preact/signals-react/runtime";
import { type ReactNode, useEffect, useRef, useState } from "react";

interface Props {
  value?: string;
  icon: ReactNode;
  className?: string;
  iconClassName?: string;
  htmlType?: "submit" | "button" | "reset" | undefined;
  type?: "primary";
  disabled?: boolean;
  onClick?: (item: any) => void;
  menu: any[];
}

const DropDownButton = (props: Props) => {
  const {
    value,
    icon,
    className = "border border-primary bg-primary text-on-accent w-44 h-10",
    onClick,
    htmlType = "button",
    disabled = false,
    menu,
  } = props;
  
  useSignals();

  const [open, setOpen] = useState(false);
  const dropdownRef = useRef<HTMLUListElement>(null);
  const buttonRef = useRef<HTMLButtonElement>(null);

  const handleToggle = (e: React.MouseEvent) => {
    e.stopPropagation();
    setOpen(!open);
  };

  // Close on Escape key for better accessibility
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (open && event.key === "Escape") {
        setOpen(false);
      }
    };
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [open]);

  useEffect(() => {
    const clickHandler = (event: MouseEvent) => {
      if (
        open &&
        dropdownRef.current && 
        !dropdownRef.current.contains(event.target as Node) &&
        buttonRef.current && 
        !buttonRef.current.contains(event.target as Node)
      ) {
        setOpen(false);
      }
    };

    document.addEventListener("click", clickHandler);
    return () => document.removeEventListener("click", clickHandler);
  }, [open]);

  return (
    <div className="relative inline-block text-left"> 
      <button
        ref={buttonRef}
        className={`${className} rounded-lg transition-all duration-200 ease-in-out active:scale-95 focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 disabled:opacity-70 disabled:cursor-not-allowed`}
        onClick={handleToggle}
        disabled={disabled}
        type={htmlType}
        aria-haspopup="true"
        aria-expanded={open}
      >
        <div className={`flex items-center justify-center gap-2 ${value ? "px-2" : ""}`}>
          <span className="flex items-center">{icon}</span>
          {value && <span className="truncate">{value}</span>}
        </div>
      </button>

      {open && (
        <ul
          ref={dropdownRef}
          role="menu"
          className="absolute bg-card border border-border right-0 mt-2 z-50 min-w-44 w-max shadow-2xl rounded-lg overflow-hidden"
        >
          {menu?.map((item, index) => (
            <li
              key={index}
              role="none"
              className={item.disable ? "bg-secondary cursor-not-allowed" : "cursor-pointer"}
            >
              <button
                role="menuitem"
                className={`flex items-center gap-3 p-3 w-full text-left transition-colors duration-150 whitespace-nowrap ${
                  item.disable 
                    ? "text-muted" 
                    : "hover:bg-secondary text-foreground hover:text-primary"
                }`}
                disabled={item.disable}
                onClick={(e) => {
                  e.stopPropagation();
                  onClick?.(item);
                  setOpen(false);
                }}
              >
                <span className={`${item.disable ? "text-muted" : "text-primary"} text-base`}>
                  {item.icon}
                </span>
                <span className={`text-sm font-medium ${item.disable ? "text-muted" : "text-foreground"}`}>
                  {item.label}
                </span>
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};

export default DropDownButton;
