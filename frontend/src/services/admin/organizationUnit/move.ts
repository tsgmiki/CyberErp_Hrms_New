import { api } from "@/utils/apiClient";

/** One drag-and-drop in the org structure tree. */
export interface MoveOrganizationUnitRequest {
  /** The unit that was dragged. */
  id: string;
  /** Its new parent; `null` makes it a root unit. */
  parentId: string | null;
  /**
   * The sibling it should sit immediately AFTER; `null` puts it first.
   *
   * ⚠️ An anchor, not an index — an index stops meaning anything as soon as the tree the user
   * dragged on differs from the tree on the server. If the anchor has itself moved since the page
   * loaded, the server refuses the move rather than guessing.
   */
  afterId: string | null;
}

/** PUT OrganizationUnit/move — reparent and reposition in one call. */
export default async function moveOrganizationUnit(body: MoveOrganizationUnitRequest) {
  return api.put<{ message: string }>("OrganizationUnit/move", body);
}
