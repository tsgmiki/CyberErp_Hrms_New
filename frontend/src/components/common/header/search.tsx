import { Command, Search } from "lucide-react";

interface HeaderSearchProps {
  onOpen?: () => void;
}

function HeaderSearch({ onOpen }: HeaderSearchProps) {
  return (
    <button
      type="button"
      onClick={onOpen}
      className="flex items-center gap-2 h-8 px-3 rounded-lg bg-muted/50 text-muted-foreground hover:bg-muted hover:text-foreground transition-all duration-150 text-xs group"
    >
      <Search className="w-3.5 h-3.5" />
      <span className="hidden sm:inline">Search...</span>
      <kbd className="hidden md:inline-flex items-center gap-0.5 ml-4 px-1.5 py-0.5 rounded bg-background border border-border text-[10px] font-mono text-muted-foreground/70">
        <Command className="w-2.5 h-2.5" />K
      </kbd>
    </button>
  );
}

export default HeaderSearch;
