import type { FormComponentModel } from "@/models";
import {
  useEffect,
  useRef,
  useState,
  useMemo,
  useCallback,
} from "react";
import { SquarePlus } from "lucide-react";
import SmallLoading from "../common/smallLoader/loader";
import { useTranslation } from "react-i18next";
import { FieldShell } from "./fieldShell";
import { FORM_COMPACT_INPUT_CLASS, FORM_INPUT_CLASS, LIST_FILTER_CONTROL_CLASS } from "./fieldStyles";

const DropDownField = ({
  error,
  required = false,
  name,
  label,
  disabled,
  labelWidth,
  value = "",
  data = [],
  displayValue,
  onSelect,
  onFocus,
  param,
  setParam,
  isLoading,
  showAdd,
  colSpan,
  onAdd,
  compact,
  placeholder,
  layout,
}: FormComponentModel & { compact?: boolean }) => {
  const [open, setOpen] = useState(false);
  const [localSearch, setLocalSearch] = useState("");
  const [currentValue, setCurrentValue] = useState({
    id: value,
    name: displayValue || "",
  });
  const [dropdownStyle, setDropdownStyle] = useState<React.CSSProperties>({
    position: "fixed",
    zIndex: 99999,
  });

  const inputRef = useRef<HTMLInputElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const { t } = useTranslation();

  const menuClass = "bg-card border border-border text-foreground shadow-lg";

  // Sync internal state
  useEffect(() => {
    if (typeof displayValue != "undefined")
      setCurrentValue({ id: currentValue.id, name: displayValue });
  }, [displayValue]);
  useEffect(() => {
    if (typeof value == "undefined" || value == "")
      setCurrentValue({ id: "", name: displayValue || "" });
  }, [value]);
  useEffect(() => {
    if (typeof value != "undefined")
      setCurrentValue((prevState) => ({
        ...prevState,
        id: value || "",
      }));
  }, [value]);

  // Handle Local Search Filtering
  const displayedData = useMemo(() => {
    if (param) return data;
    if (!localSearch) return data;
    const searchLower = localSearch.toLowerCase();
    return data.filter(
      (item: any) =>
        item.name?.toLowerCase().includes(searchLower) ||
        item.remark?.toLowerCase().includes(searchLower),
    );
  }, [data, localSearch, param]);

  // Recalculate dropdown position when scrolling
  const updateDropdownPosition = useCallback(() => {
    if (!containerRef.current) return;

    const rect = containerRef.current.getBoundingClientRect();
    const viewportHeight = window.innerHeight;
    const spaceBelow = viewportHeight - rect.bottom;
    const spaceAbove = rect.top;

    const dropdownMaxHeight = 250;
    const showBelow = spaceBelow > dropdownMaxHeight || spaceBelow > spaceAbove;

    const inputWidth = rect.width - (showAdd ? 42 : 0);

    setDropdownStyle({
      position: "fixed",
      top: showBelow 
        ? `${rect.bottom + 4}px` 
        : "auto",
      bottom: showBelow 
        ? "auto" 
        : `${viewportHeight - rect.top + 4}px`,
      left: `${rect.left}px`,
      width: `${inputWidth}px`,
      maxWidth: "400px",
      zIndex: 99999,
    });
  }, [showAdd]);

  // Outside Click Handler & Scroll/Resize listeners
  useEffect(() => {
    const clickHandler = (event: MouseEvent) => {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setOpen(false);
      }
    };
    if (open) {
      updateDropdownPosition();
      document.addEventListener("mousedown", clickHandler);
      // Update position on scroll/resize when dropdown is open
      window.addEventListener("scroll", updateDropdownPosition, true);
      window.addEventListener("resize", updateDropdownPosition);
    }
    return () => {
      document.removeEventListener("mousedown", clickHandler);
      window.removeEventListener("scroll", updateDropdownPosition, true);
      window.removeEventListener("resize", updateDropdownPosition);
    };
  }, [open, updateDropdownPosition]);

  const toggleDropdown = () => {
    if (disabled) return;
    setOpen(!open);
    setLocalSearch("");
  };

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const text = e.target.value;
    setLocalSearch(text);
    if (param) {
      setParam?.({ ...param, searchText: text } as any);
    }
  };

  return (
    <FieldShell
      name={name}
      label={label}
      required={required}
      labelWidth={labelWidth}
      colSpan={colSpan}
      error={error}
      controlClassName={
        compact ? `relative ${LIST_FILTER_CONTROL_CLASS}` : "relative w-full min-w-0"
      }
      layout={compact ? "toolbar" : layout ?? "horizontal"}
      hideLabel={compact ? true : !label}
    >
      <div className="w-full" ref={dropdownRef}>
        <div className="inline-flex w-full items-center" ref={containerRef}>
          <input
            ref={inputRef}
            className="hidden"
            name={name}
            id={name}
            value={currentValue.id}
            readOnly
          />

          <input
            className={`${compact ? FORM_COMPACT_INPUT_CLASS : FORM_INPUT_CLASS} cursor-pointer`}
            value={localSearch || currentValue.name}
            onChange={(e) => {
              handleSearchChange(e);
              setOpen(true);
            }}
            onFocus={(e) => {
              e.target.select();
              onFocus?.();
              toggleDropdown();
            }}
            placeholder={placeholder ? t(placeholder) : t("Select...")}
          />

          {showAdd && (
            <button
              type="button"
              className="ml-1 flex h-10 shrink-0 items-center justify-center rounded-lg bg-primary px-2 text-on-accent transition-colors hover:bg-primary-hover"
              onClick={() => onAdd?.()}
            >
              <SquarePlus size={18} />
            </button>
          )}
        </div>

        {/* DROPDOWN MENU - Fixed position near input */}
        {open && (
          <div
            className={`border shadow-lg rounded-lg flex flex-col max-h-62.5 overflow-visible ${menuClass}`}
            style={dropdownStyle}
          >
            <div className="p-1"></div>

            <ul className="overflow-y-auto flex-1">
              {isLoading ? (
                <div className="p-4 flex justify-center">
                  <SmallLoading />
                </div>
              ) : displayedData.length === 0 ? (
                <li className="p-2 text-sm opacity-60 text-center">
                  {t("No data found")}
                </li>
               ) : (
                displayedData.map((item: any) => (
                  <li
                    key={item.id}
                    className={`border-b border-border p-3 hover:cursor-pointer transition-colors ${item.disable ? "bg-secondary text-muted cursor-not-allowed" : "hover:bg-secondary text-foreground"}`}
                    onClick={() => {
                      if (item.disable) return;
                      setCurrentValue({ id: item.id, name: item.name });
                      setLocalSearch("");
                      onSelect?.(name as string, item);
                      setOpen(false);
                    }}
                  >
                    <div className="flex flex-col text-start">
                      <span className="text-sm font-medium">{item.name}</span>
                      {item.remark && (
                        <span className="text-xs italic opacity-70">
                          {item.remark}
                        </span>
                      )}
                    </div>
                  </li>
                ))
              )}
            </ul>
          </div>
        )}

      </div>
    </FieldShell>
  );
};

export default DropDownField;
