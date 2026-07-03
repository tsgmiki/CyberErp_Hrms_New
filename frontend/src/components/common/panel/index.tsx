import { ChevronDown } from "lucide-react";
import { useState, type ReactNode } from "react";

interface PanelProps {
  children: ReactNode;
  title?: string;
  subtitle?: string;
  hideBorder?: boolean;
  defaultExpanded?: boolean;
  showExpandButton?: boolean;
  className?: string;
  headerClassName?: string;
}

export default function Panel({
  children,
  title,
  subtitle,
  hideBorder = false,
  defaultExpanded = true,
  showExpandButton = true,
  className = "",
  headerClassName = "",
}: PanelProps) {
  const [isExpanded, setIsExpanded] = useState(defaultExpanded);

  return (
    <div
      className={`
        rounded-xl border border-border bg-card shadow-sm transition-all duration-300
        ${hideBorder ? "border-0 shadow-none" : ""}
        ${className}
      `}
    >
      {/* Header */}
      <div
        className={`
          flex items-center justify-between px-4 py-3
          ${isExpanded ? "rounded-t-xl" : "rounded-xl"}
          border-b border-border bg-muted/30
          ${headerClassName}
        `}
      >
        <div className="flex items-center gap-3">
          {title && (
            <div className="flex flex-col">
              <span
                className={`
                  font-semibold text-sm tracking-tight
                  text-foreground
                `}
              >
                {title}
              </span>
              {subtitle && (
                <span
                  className={`
                    text-xs mt-0.5
                    text-primary
                  `}
                >
                  {subtitle}
                </span>
              )}
            </div>
          )}
        </div>

        {showExpandButton && (
          <button
            onClick={() => setIsExpanded(!isExpanded)}
            className={`
              p-1.5 rounded-lg transition-all duration-200
              ${isExpanded ? "rotate-0" : "-rotate-90"}
              text-primary hover:text-foreground hover:bg-secondary/50
            `}
            aria-label={isExpanded ? "Collapse panel" : "Expand panel"}
          >
            <ChevronDown size={18} />
          </button>
        )}
      </div>

      {/* Content */}
      <div
        className={`
          transition-all duration-300 ease-in-out overflow-hidden
          ${isExpanded ? "max-h-1250 opacity-100" : "max-h-0 opacity-0"}
        `}
      >
        <div
          className={`
            p-4
            text-foreground
          `}
        >
          {children}
        </div>
      </div>
    </div>
  );
}
