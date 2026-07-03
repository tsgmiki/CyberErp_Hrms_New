import { Mail, Phone, User, Briefcase } from "lucide-react";

export type AvatarDisplayType = "name" | "phone" | "email" | "position";

export interface AvatarDisplayProps {
  name?: string;
  phone?: string;
  email?: string;
  position?: string;
  displayType?: AvatarDisplayType;
  size?: "sm" | "md" | "lg";
}

const sizeClasses = {
  sm: "w-6 h-6 text-xs",
  md: "w-8 h-8 text-sm",
  lg: "w-10 h-10 text-base",
};

const colors = [
  "bg-blue-500",
  "bg-green-500",
  "bg-purple-500",
  "bg-pink-500",
  "bg-indigo-500",
  "bg-yellow-500",
  "bg-red-500",
  "bg-teal-500",
];

function getInitials(text?: string): string {
  if (!text) return "?";

  const parts = text.trim().split(" ");
  if (parts.length === 1) {
    return parts[0].charAt(0).toUpperCase();
  }

  const first = parts[0].charAt(0);
  const last = parts[parts.length - 1].charAt(0);
  return (first + last).toUpperCase();
}

function getColorIndex(text?: string): number {
  if (!text) return 0;
  return text.length % colors.length;
}

function getDisplayValue(
  type: AvatarDisplayType,
  name?: string | undefined,
  phone?: string | undefined,
  email?: string | undefined,
  position?: string | undefined
): string {
  switch (type) {
    case "name":
      return name || "";
    case "phone":
      return phone || "";
    case "email":
      return email || "";
    case "position":
      return position || "";
    default:
      return name || "";
  }
}

function getIcon(type: AvatarDisplayType, size: "sm" | "md" | "lg") {
  const iconSize = size === "sm" ? 12 : size === "md" ? 16 : 20;

  switch (type) {
    case "phone":
      return <Phone size={iconSize} />;
    case "email":
      return <Mail size={iconSize} />;
    case "position":
      return <Briefcase size={iconSize} />;
    default:
      return <User size={iconSize} />;
  }
}

export default function AvatarDisplay(props: AvatarDisplayProps) {
  const {
    name,
    phone,
    email,
    position,
    displayType = "name",
    size = "md",
  } = props;

  const displayValue = getDisplayValue(displayType, name, phone, email, position);
  const showIcon = displayType !== "name" && !!displayValue;
  const initials = getInitials(displayValue);
  const colorIndex = getColorIndex(displayValue);
  const bgColor = colors[colorIndex];
  const sizeClass = sizeClasses[size];
  const icon = getIcon(displayType, size);

  return (
    <div className="flex items-center gap-2">
      <div
        className={`${sizeClass} rounded-full ${bgColor} text-white flex items-center justify-center`}
      >
        {showIcon ? icon : <span className="font-medium">{initials}</span>}
      </div>
      <span className="text-sm  truncate max-w-[150px]">
        {displayValue || "-"}
      </span>
    </div>
  );
}
