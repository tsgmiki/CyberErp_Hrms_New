import { lazy, memo } from "react";
import { MailPlus } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";

const NotificationTemplateForm = memo(lazy(() => import("./form")));
const NotificationTemplateList = memo(lazy(() => import("./list")));

/**
 * Administrator-defined e-mail templates and recipient routing.
 *
 * Replaces content and recipients that used to be compiled in: an administrator writes the subject
 * and body for an event, and defines WHO hears about it with rules that resolve at send time.
 */
function NotificationTemplate() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityRouteModule("/notificationTemplate");

  return (
    <EntityModuleShell
      title="Email Templates"
      headerDescription="Define what each automated message says, and who receives it, per event and workflow step"
      headerIcon={<MailPlus className="h-6 w-6 text-primary" />}
      tableTitle="Email Templates"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<NotificationTemplateForm id={id} setId={setId} />}
      list={<NotificationTemplateList editHandler={editHandler} />}
    />
  );
}

export default NotificationTemplate;
