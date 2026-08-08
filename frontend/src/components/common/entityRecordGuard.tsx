import { memo } from "react";
import { Outlet, useParams } from "react-router-dom";
import isValidGUID from "@/components/util/validateGuid";
import NotFoundPage from "@/pages/home/notFound";
import { NEW_SEGMENT } from "@/template";

/**
 * Wraps the `:id` route of an entity so a malformed id never reaches the API.
 *
 * This matters because `createEntityGetById` swallows fetch errors and resolves to `undefined`:
 * without the guard, "/branch/garbage" would fire a doomed request and then render a form that
 * looks blank-but-editable rather than broken.
 *
 * "new" shares the `:id` slot (see entityRoutes for why) so it is admitted explicitly here.
 */
function EntityRecordGuard() {
  const { id } = useParams();
  if (id !== NEW_SEGMENT && !isValidGUID(id)) return <NotFoundPage />;
  return <Outlet />;
}

export default memo(EntityRecordGuard);
