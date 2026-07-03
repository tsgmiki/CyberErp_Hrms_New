import { lazy, memo, Suspense } from "react";

const ThemeSwitcher = memo(lazy(() => import("@/components/common/header/themeSwitcher")));
const Notification = memo(lazy(() => import("@/components/common/header/notification")));
const Accounts = memo(lazy(() => import("@/components/common/header/accounts")));

function Navigation() {
  return (
    <div className="flex items-center gap-0.5">
      <Suspense fallback={null}>
        <ThemeSwitcher />
        <Notification />
        <Accounts />
      </Suspense>
    </div>
  );
}

export default Navigation;
