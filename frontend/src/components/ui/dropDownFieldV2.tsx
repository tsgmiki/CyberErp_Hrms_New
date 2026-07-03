import type { FormComponentModel } from "@/models";
import { useEffect, useRef, useState, useMemo } from "react";
import { Search, SquarePlus } from "lucide-react";
import { ZodErrors } from "../common/statusMessage/status";
import SmallLoading from "../common/smallLoader/loader";
import { useTranslation } from "react-i18next";

const DropDownFieldV2 = ({
  error,
  required = false,
  name,
  label,
  disabled,
  labelWidth,
  value = "",
  data = [], // Default to empty array
  displayValue,
  onSelect,
  onSearch: _onSearch,
  param,
  setParam,
  isLoading,
  showAdd,
  colSpan,
  onAdd,
}: FormComponentModel) => {
  const [open, setOpen] = useState(false);
  const [localSearch, setLocalSearch] = useState(""); // Internal state for local search
  const [currentValue, setCurrentValue] = useState({
    id: value,
    name: displayValue || "",
  });
  const [dropdownStyle, setDropdownStyle] = useState<React.CSSProperties>({});

  const dropdownRef = useRef<HTMLDivElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const { t } = useTranslation();

  // Unified theme-aware classes
  const labelClass = "text-foreground";
  const inputBaseClass = "bg-background border text-foreground focus:border-primary focus:ring-0 focus:outline-none";
  const menuClass = "bg-card border border-border text-foreground";
  const searchInputClass = "bg-background border text-foreground placeholder:text-muted focus:border-primary";
  const addButtonClass = "bg-primary text-on-accent hover:bg-primary-hover";

  // 1. Sync internal state
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

  // 2. Handle Local Search Filtering
  const displayedData = useMemo(() => {
    // If param exists, we assume server-side search is happening; return data as-is
    if (param) return data;

    // If no param, filter the local data array based on internal search text
    if (!localSearch) return data;

    const searchLower = localSearch.toLowerCase();
    return data.filter(
      (item: any) =>
        item.name?.toLowerCase().includes(searchLower) ||
        item.remark?.toLowerCase().includes(searchLower),
    );
  }, [data, localSearch, param]);

  // 3. Outside Click Handler
  useEffect(() => {
    const clickHandler = (event: MouseEvent) => {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setOpen(false);
      }
    };
    if (open) document.addEventListener("mousedown", clickHandler);
    return () => document.removeEventListener("mousedown", clickHandler);
  }, [open]);

  const toggleDropdown = () => {
    if (disabled) return;

    if (!open && containerRef.current) {
      // Calculate position relative to the viewport
      const rect = containerRef.current.getBoundingClientRect();
      const viewportHeight = window.innerHeight;
      const spaceBelow = viewportHeight - rect.bottom;
      const spaceAbove = rect.top;

      // Dropdown has max-height of 250px (max-h-62.5 -> 15.625rem).
      // Show below if there's enough space, or if there's more space below than above.
      const dropdownMaxHeight = 250;
      const showBelow = spaceBelow > dropdownMaxHeight || spaceBelow > spaceAbove;

      // Calculate width - match the input field width (not including add button)
      const inputWidth = rect.width - (showAdd ? 42 : 0);

      // Always use 'fixed' positioning to ensure the dropdown appears above all other content,
      // including the confines of a modal with `overflow: auto` or `overflow: scroll`.
      // The position is calculated relative to the viewport.
      setDropdownStyle({
        position: "fixed",
        top: showBelow ? `${rect.bottom + 4}px` : "auto",
        bottom: showBelow ? "auto" : `${viewportHeight - rect.top + 4}px`,
        left: `${rect.left}px`,
        width: `${inputWidth}px`,
        maxWidth: "400px",
        zIndex: 99999, // A high z-index is crucial to appear over modals.
      });
    }

    setOpen(!open);
    setLocalSearch("");
    if (param) setParam?.({ ...param, searchText: "" } as any);
  };

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const text = e.target.value;
    if (param) {
      setParam?.({ ...param, searchText: text } as any);
    } else {
      setLocalSearch(text);
    }
  };

  return (
    <div
      key={name}
      className={`${colSpan != "full" && "md:inline-flex"} gap-1 w-full`}
    >
      <label
        className={`${labelClass} font-medium text-sm col-span-1 max-md:w-full text-end flex items-center justify-end gap-0.5 ${
          colSpan == "full"
            ? "w-full"
            : typeof labelWidth != "undefined"
              ? labelWidth
              : "w-[20%]"
        }`}
      >
        {label ? t(label) : ""}
        <span
          className={
          required
            ? "text-error text-lg leading-none"
            : "text-transparent text-lg leading-none"
          }
        >
          *
        </span>
      </label>
      <div className="w-full relative" ref={dropdownRef}>
        <div className="inline-flex w-full items-center" ref={containerRef}>
          <button
            type="button"
            className={`${inputBaseClass} flex items-center justify-center h-[33.5px] px-3 rounded-tl-[5px] rounded-bl-[5px] border-b transition-all duration-200`}
            onClick={toggleDropdown}
          >
            <Search size={18} />
          </button>

          {showAdd && (
            <button
              type="button"
              className={`ml-1 ${addButtonClass} flex items-center justify-center h-[33.5px] px-2 rounded-[5px] transition-colors`}
              onClick={() => onAdd?.()}
            >
              <SquarePlus size={18} />
            </button>
          )}
        </div>

        {/* DROPDOWN MENU */}
        {open && (
          <div
            className={`border shadow-lg rounded-lg flex flex-col max-h-62.5 ${menuClass}`}
            style={dropdownStyle}
          >
            {/* Search Input (Always visible now) */}
            <div className="p-2 border-b">
              <input
                className={`w-full p-2 text-sm rounded border ${searchInputClass}`}
                placeholder={t("Search...")}
                autoFocus
                value={param ? param.searchText : localSearch}
                onChange={handleSearchChange}
              />
            </div>

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
                    className={`border-b p-3 hover:cursor-pointer transition-colors ${item.disable ? "bg-secondary text-muted cursor-not-allowed" : "hover:bg-secondary text-foreground"}`}
                    onClick={() => {
                      if (item.disable) return;
                      setCurrentValue({ id: item.id, name: item.name });
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

        <ZodErrors error={error} />
      </div>
    </div>
  );
};

export default DropDownFieldV2;
