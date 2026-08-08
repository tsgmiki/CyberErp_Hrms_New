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
 * Mounted on the entity's shared parent route, so it also renders for the LIST (where there is no
 * `:id` at all) — see entityRoutes for why both children must sit at the same depth. Hence three
 * accepted cases: no id (list), the literal "new" (create), and a well-formed GUID (edit).
 */
function EntityRecordGuard() {
  const { id } = useParams();
  const invalid = id !== undefined && id !== NEW_SEGMENT && !isValidGUID(id);
  if (invalid) return <NotFoundPage />;
  return <Outlet />;
}

export default memo(EntityRecordGuard);
