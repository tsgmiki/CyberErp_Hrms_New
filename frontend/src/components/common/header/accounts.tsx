import { ChevronDown, LogOut, Settings } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";
import {
  HeaderDropdown,
  HeaderDropdownContent,
  HeaderDropdownItem,
  HeaderDropdownSeparator,
  HeaderDropdownTrigger,
} from "./dropdown";

function Accounts() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const initials =
    user?.fullName
      ?.split(" ")
      .map((n) => n[0])
      .join("")
      .toUpperCase()
      .slice(0, 2) || "U";

  return (
    <HeaderDropdown>
      <HeaderDropdownTrigger>
        <button
          type="button"
          className="flex items-center gap-2 px-1.5 py-1 rounded-lg hover:bg-muted transition-colors"
        >
          <UserAvatar initials={initials} />
          <UserMeta name={user?.fullName} email={user?.email} />
          <ChevronDown className="w-3 h-3 text-muted-foreground hidden md:block" />
        </button>
      </HeaderDropdownTrigger>
      <HeaderDropdownContent align="end" className="w-56">
        <div className="px-3 py-2.5">
          <p className="text-sm font-semibold text-foreground">{user?.fullName || "User"}</p>
          {user?.email ? (
            <p className="text-xs text-muted-foreground mt-0.5">{user.email}</p>
          ) : null}
        </div>
        <HeaderDropdownSeparator />
        <HeaderDropdownItem
          className="gap-2.5 text-xs"
          onClick={() => navigate("/password")}
        >
          <Settings className="w-3.5 h-3.5 text-muted-foreground" /> Settings
        </HeaderDropdownItem>
        <HeaderDropdownSeparator />
        <HeaderDropdownItem
          onClick={() => {
            logout();
            navigate("/");
          }}
          className="gap-2.5 text-xs text-destructive focus:text-destructive"
        >
          <LogOut className="w-3.5 h-3.5" /> Sign Out
        </HeaderDropdownItem>
      </HeaderDropdownContent>
    </HeaderDropdown>
  );
}

function UserAvatar({ initials }: { initials: string }) {
  return (
    <div className="w-7 h-7 rounded-full bg-primary text-primary-foreground flex items-center justify-center text-[11px] font-semibold ring-2 ring-primary/20">
      {initials}
    </div>
  );
}

function UserMeta({ name, email }: { name?: string; email?: string }) {
  if (!name) return null;

  return (
    <div className="hidden md:block text-left">
      <p className="text-xs font-semibold text-foreground leading-tight">{name}</p>
      {email ? (
        <p className="text-[10px] text-muted-foreground leading-tight truncate max-w-[120px]">
          {email}
        </p>
      ) : null}
    </div>
  );
}

export default Accounts;
