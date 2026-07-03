import InventoryLayout from "@/components/common/inventoryLayout";
import type { EntityModuleShellProps } from "./types";

/**
 * Standard CRUD page wrapper: header, list/add/back actions, list vs form body.
 */
export function EntityModuleShell({
  title,
  headerDescription,
  headerIcon,
  showForm,
  onList,
  onAdd,
  list,
  form,
  hideAdd,
  hideBack,
  tableTitle,
  tableDescription,
  tableIcon,
  onSetting,
  hideSetting,
  children,
}: EntityModuleShellProps) {
  const body = children ?? (showForm ? form : list);

  return (
    <InventoryLayout
      title={title}
      headerDescription={headerDescription}
      headerIcon={headerIcon}
      onList={onList}
      onAdd={onAdd}
      onSetting={onSetting}
      showForm={showForm}
      hideAdd={hideAdd}
      hideBack={hideBack}
      hideSetting={hideSetting}
      tableTitle={tableTitle}
      tableDescription={tableDescription}
      tableIcon={tableIcon}
    >
      {body}
    </InventoryLayout>
  );
}
