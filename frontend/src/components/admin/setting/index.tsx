import { lazy, memo } from "react";
import { Settings2 } from "lucide-react";
import { EntityModuleShell } from "@/template";

const SettingForm = memo(lazy(() => import("./form")));

/**
 * Deployment operations settings: the outbound mail relay and the backup schedule.
 *
 * A SINGLETON, like Increment Rules — one `Core.Setting` row, so there is no list to page through,
 * nothing to add and nothing to go back to. The shell is used anyway to keep the standard header and
 * chrome, with `showForm` pinned true and the list/add/back actions suppressed rather than left on
 * screen doing nothing.
 */
function Setting() {
  return (
    <EntityModuleShell
      title="Settings"
      headerDescription="Outbound e-mail relay and database backup schedule for this deployment"
      headerIcon={<Settings2 className="h-6 w-6 text-primary" />}
      showForm
      hideAdd
      hideBack
      form={<SettingForm />}
    />
  );
}

export default Setting;
