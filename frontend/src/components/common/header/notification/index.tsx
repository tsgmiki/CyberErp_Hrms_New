import { Bell } from "lucide-react";
import { useState } from "react";

function Notification() {
  const [hasUnread] = useState(true);

  return (
    <a
      href="/notification"
      className="relative w-8 h-8 flex items-center justify-center rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted transition-colors"
      aria-label="Notifications"
    >
      <Bell className="w-4 h-4" />
      {hasUnread && (
        <span className="absolute top-1.5 right-1.5 w-1.5 h-1.5 bg-error rounded-full" />
      )}
    </a>
  );
}

export default Notification;
