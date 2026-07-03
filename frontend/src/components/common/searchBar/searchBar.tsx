import { memo } from "react";
import type { ChangeEvent, KeyboardEvent } from "react";
import { Search, X } from "lucide-react";

interface SearchBarProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  disabled?: boolean;
  onEnter?: () => void;
  onClear?: () => void;
  className?: string;
}

const SearchBar = memo(function SearchBar({
  value,
  onChange,
  placeholder = "Search",
  disabled = false,
  onEnter,
  onClear,
  className = "",
}: SearchBarProps) {
  return (
    <div className={`relative min-w-0 flex-1 max-w-md ${className}`}>
      <Search
        size={16}
        className="absolute left-3 top-1/2 -translate-y-1/2 text-accent"
      />
      <input
        type="text"
        placeholder={placeholder}
        value={value}
        onChange={(e: ChangeEvent<HTMLInputElement>) =>
          onChange(e.target.value)
        }
        onKeyDown={(e: KeyboardEvent<HTMLInputElement>) => {
          if (e.key === "Enter" && onEnter) {
            onEnter();
          }
        }}
        disabled={disabled}
        className="w-full rounded-xl border bg-secondary py-2 pl-10 pr-9 text-sm text-foreground outline-none transition-all duration-200 placeholder:text-muted focus:border-primary focus:bg-background"
      />
      {value && onClear && !disabled ? (
        <button
          type="button"
          onClick={onClear}
          className="absolute right-2 top-1/2 -translate-y-1/2 rounded p-1 text-muted transition-colors hover:bg-muted/50 hover:text-foreground"
          aria-label="Clear search"
        >
          <X size={14} />
        </button>
      ) : null}
    </div>
  );
});

SearchBar.displayName = "SearchBar";
export default SearchBar;
