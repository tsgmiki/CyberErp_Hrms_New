import {
  cloneElement,
  createContext,
  isValidElement,
  useCallback,
  useContext,
  useEffect,
  useLayoutEffect,
  useRef,
  useState,
  type CSSProperties,
  type MouseEvent as ReactMouseEvent,
  type ReactElement,
  type ReactNode,
} from "react";
import { createPortal } from "react-dom";

type Align = "start" | "end";

/** Above inventory layout, tables, and in-page overlays (z-50) */
const HEADER_DROPDOWN_Z_INDEX = 200;

interface DropdownContextValue {
  open: boolean;
  setOpen: (open: boolean) => void;
  rootRef: React.RefObject<HTMLDivElement | null>;
  panelRef: React.RefObject<HTMLDivElement | null>;
}

const DropdownContext = createContext<DropdownContextValue | null>(null);

function useDropdownContext() {
  const context = useContext(DropdownContext);
  if (!context) {
    throw new Error("Header dropdown components must be used within HeaderDropdown");
  }
  return context;
}

function HeaderDropdown({ children }: { children: ReactNode }) {
  const [open, setOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);
  const panelRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;

    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") setOpen(false);
    };

    const onPointerDown = (event: Event) => {
      const target = event.target as Node;
      if (rootRef.current?.contains(target) || panelRef.current?.contains(target)) {
        return;
      }
      setOpen(false);
    };

    document.addEventListener("keydown", onKeyDown);
    document.addEventListener("mousedown", onPointerDown);

    return () => {
      document.removeEventListener("keydown", onKeyDown);
      document.removeEventListener("mousedown", onPointerDown);
    };
  }, [open]);

  return (
    <DropdownContext.Provider value={{ open, setOpen, rootRef, panelRef }}>
      <div ref={rootRef} className="relative inline-block text-left">
        {children}
      </div>
    </DropdownContext.Provider>
  );
}

function HeaderDropdownTrigger({ children }: { children: ReactElement }) {
  const { open, setOpen } = useDropdownContext();

  if (!isValidElement(children)) {
    return children;
  }

  type TriggerProps = {
    onClick?: (event: ReactMouseEvent<HTMLButtonElement>) => void;
    "aria-expanded"?: boolean;
    "aria-haspopup"?:
      | boolean
      | "menu"
      | "listbox"
      | "tree"
      | "grid"
      | "dialog";
  };
  const trigger = children as ReactElement<TriggerProps>;

  return cloneElement(trigger, {
    onClick: (event: ReactMouseEvent<HTMLButtonElement>) => {
      trigger.props.onClick?.(event);
      event.stopPropagation();
      setOpen(!open);
    },
    "aria-expanded": open,
    "aria-haspopup": "menu",
  });
}

interface HeaderDropdownContentProps {
  children: ReactNode;
  align?: Align;
  className?: string;
}

function HeaderDropdownContent({
  children,
  align = "end",
  className = "",
}: HeaderDropdownContentProps) {
  const { open, rootRef, panelRef } = useDropdownContext();
  const [panelStyle, setPanelStyle] = useState<CSSProperties>({});

  const updatePosition = useCallback(() => {
    const anchor = rootRef.current;
    if (!anchor) return;

    const rect = anchor.getBoundingClientRect();
    const next: CSSProperties = {
      position: "fixed",
      top: rect.bottom + 4,
      zIndex: HEADER_DROPDOWN_Z_INDEX,
    };

    if (align === "end") {
      next.right = Math.max(8, window.innerWidth - rect.right);
      next.left = "auto";
    } else {
      next.left = rect.left;
      next.right = "auto";
    }

    setPanelStyle(next);
  }, [align, rootRef]);

  useLayoutEffect(() => {
    if (!open) return;

    updatePosition();

    window.addEventListener("resize", updatePosition);
    window.addEventListener("scroll", updatePosition, true);

    return () => {
      window.removeEventListener("resize", updatePosition);
      window.removeEventListener("scroll", updatePosition, true);
    };
  }, [open, updatePosition]);

  if (!open) return null;

  return createPortal(
    <div
      ref={panelRef}
      role="menu"
      style={panelStyle}
      className={`min-w-32 overflow-hidden rounded-md border border-border bg-popover p-1 text-popover-foreground shadow-lg ${className}`}
    >
      {children}
    </div>,
    document.body,
  );
}

interface HeaderDropdownItemProps {
  children: ReactNode;
  onClick?: () => void;
  className?: string;
}

function HeaderDropdownItem({ children, onClick, className = "" }: HeaderDropdownItemProps) {
  const { setOpen } = useDropdownContext();

  return (
    <button
      type="button"
      role="menuitem"
      className={`flex w-full cursor-default select-none items-center rounded-sm px-2 py-1.5 text-sm outline-none transition-colors hover:bg-accent focus:bg-accent ${className}`}
      onClick={() => {
        onClick?.();
        setOpen(false);
      }}
    >
      {children}
    </button>
  );
}

function HeaderDropdownSeparator() {
  return <div className="-mx-1 my-1 h-px bg-muted" role="separator" />;
}

export {
  HeaderDropdown,
  HeaderDropdownTrigger,
  HeaderDropdownContent,
  HeaderDropdownItem,
  HeaderDropdownSeparator,
};
