import { useNavigate } from "react-router-dom";
import { useTheme } from "@/context/ThemeContext";
import { X } from "lucide-react";

interface QuickAddProps {
  isOpen: boolean;
  onClose: () => void;
}

export function QuickAdd({ isOpen, onClose }: QuickAddProps) {
  const navigate = useNavigate();
  const { theme } = useTheme();
  const isDark = theme === "dark";

  const handleNavigation = (path: string) => {
    navigate(path);
    onClose();
  };

  if (!isOpen) return null;

  const quickAddItems = [
    { label: "Sales Order", path: "/salesOrder", category: "Sales" },
    { label: "Delivery", path: "/delivery", category: "Sales" },
    { label: "Collection", path: "/collection", category: "Sales" },
    { label: "Customer", path: "/customer", category: "Sales" },
    { label: "Invoice", path: "/invoice", category: "Sales" },
    { label: "POS Sales", path: "/posSales", category: "Sales" },
    { label: "Quotation", path: "/quotation", category: "Sales" },
    { label: "Purchase Order", path: "/purchaseOrder", category: "Purchase" },
    { label: "Receive", path: "/receive", category: "Purchase" },
    { label: "Supplier", path: "/supplier", category: "Purchase" },
    { label: "Payment", path: "/payment", category: "Finance" },
    { label: "Bank Transfer", path: "/bankTransfer", category: "Finance" },
    { label: "Expense", path: "/expense", category: "Finance" },
    { label: "Adjustment", path: "/adjustment", category: "Inventory" },
    { label: "Transfer", path: "/transfer", category: "Inventory" },
    { label: "Inventory Opening", path: "/inventoryOpening", category: "Inventory" },
    { label: "Item", path: "/item", category: "Inventory" },
    { label: "Store", path: "/store", category: "Inventory" },
  ];

  return (
    <div className="absolute left-0 mt-1  w-56 rounded-lg border-text-primary-accent shadow-lg border overflow-hidden z-50">
      <div
        className={`flex items-center justify-between px-3 py-2 border-b ${
          isDark
            ? "border-text-primary-accent bg-secondary text-primary"
            : "border-text-primary-accent bg-secondary text-primary"
        }`}
      >
        <span
          className={`text-sm font-semibold ${
            isDark ? "text-text-primary-bg-primary" : "text-text-primary-bg-primary"
          }`}
        >
          Quick Add
        </span>
        <button
          onClick={onClose}
          className={`p-1 rounded-md ${
            isDark
              ? "hover:bg-text-primary-accent/80 text-text-primary-bg-primary"
              : "hover:bg-text-primary-accent/80 text-text-primary-bg-primary"
          }`}
        >
          <X className="w-4 h-4" />
        </button>
      </div>
      <div
        className={`py-1 max-h-96 overflow-y-auto ${
          isDark ? "bg-secondary text-primary" : "bg-secondary text-primary"
        }`}
      >
        {Object.entries(
          quickAddItems.reduce((acc: Record<string, typeof quickAddItems>, item) => {
            const cat = item.category || "Other";
            if (!acc[cat]) acc[cat] = [];
            acc[cat].push(item);
            return acc;
          }, {})
        ).map(([category, items]: [string, any[]]) => (
          <div key={category}>
            <div className="px-4 py-1 text-xs font-semibold text-text-primary-muted uppercase tracking-wider">
              {category}
            </div>
            {items.map((item, index: number) => (
              <button
                key={index}
                onClick={() => handleNavigation(item.path)}
                className={`w-full px-4 py-2 text-left text-sm transition-colors ${
                  isDark
                    ? "text-text-primary-bg-primary hover:bg-text-primary-accent/80"
                    : "text-text-primary-bg-primary hover:bg-text-primary-accent/80"
                }`}
              >
                {item.label}
              </button>
            ))}
          </div>
        ))}
      </div>
    </div>
  );
}