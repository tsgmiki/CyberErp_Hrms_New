import type { ReactNode } from "react";

interface SubsystemTitleProps {
  collapsed: boolean;
  title?: string;
  icon?: ReactNode;
}

function SubsystemTitle({ collapsed, title, icon }: SubsystemTitleProps) {
  if (!title || collapsed) return null;

  return (
    <div className="px-5 pt-3 pb-2">
      <div className="flex items-center gap-2">
        <span className="text-primary [&>svg]:w-4 [&>svg]:h-4">{icon}</span>
        <span className="font-display font-semibold text-sm text-foreground">{title}</span>
      </div>
    </div>
  );
}

export default SubsystemTitle;
