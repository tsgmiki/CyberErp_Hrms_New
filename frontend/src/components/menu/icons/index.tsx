import {
  LayoutDashboard,
  ShieldCheck,
  Settings,
  ShoppingBag,
  Receipt,
  Landmark,
  Warehouse,
  BarChart3,
  ArrowRightLeft,
  ClipboardList,
  Box,
  TrendingUp,
  LucideStore,
} from "lucide-react";

const icons = [
  {
    name: "Dashboard",
    icon: <LayoutDashboard size={16} />,
  },
  {
    name: "Administration",
    icon: <ShieldCheck size={16} />,
  },
  {
    name: "Transaction",
    icon: <ArrowRightLeft size={16} />,
  },
  {
    name: "Container",
    icon: <Box size={16} />,
  },
  {
    name: "Inventory",
    icon: <Warehouse size={16} />,
  },
  {
    name: "Requests",
    icon: <ClipboardList size={16} />,
    details: [{ name: "Store Requisition", icon: <LucideStore size={14} /> }],
  },
  {
    name: "Finance",
    icon: <Landmark size={16} />,
    
  },
  {
    name: "Purchases",
    icon: <ShoppingBag size={16} />,
  },
  {
    name: "Sales",
    icon: <TrendingUp size={16} />,
  },
  {
    name: "Setting",
    icon: <Settings size={16} />,
  },
  {
    name: "Expense",
    icon: <Receipt size={16} />,
  },
  {
    name: "Report",
    icon: <BarChart3 size={16} />,
  },
];

export { icons };
