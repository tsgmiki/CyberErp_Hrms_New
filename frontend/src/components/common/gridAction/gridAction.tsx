"use client";
import Tooltip from "../toolTip/tooltip";
import { FileX, Pencil, Plus, TableCellsSplit, Trash } from "lucide-react";
import DialogModal from "../dialog";
import { type ReactNode, useState } from "react";
import store from "@/store";
import { useSignals } from "@preact/signals-react/runtime";
import { useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";

interface Props {
  editHandler?: (id: string) => void;
  deleteHandler?: (id: string) => void;
  addHandler?: (record?: unknown) => void;
  actionHandler?: (id: string) => void;
  id: string;
  record?: any;
  showAdd?: boolean;
  showEdit?: boolean;
  showDelete?: boolean;
  showAction?: boolean;
  actionName?: string;
}

type ActionTone = "default" | "primary" | "danger";

interface GridActionButtonProps {
  label: string;
  icon: ReactNode;
  disabled?: boolean;
  tone?: ActionTone;
  onClick?: () => void;
}

function gridActionButtonClass(tone: ActionTone, disabled?: boolean): string {
  const base =
    "inline-flex h-8 w-8 shrink-0 items-center justify-center rounded-md border-0 bg-transparent p-0 shadow-none transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/25";

  if (disabled) {
    return `${base} cursor-not-allowed opacity-40`;
  }

  if (tone === "danger") {
    return `${base} text-error/80 hover:bg-error/10 hover:text-error`;
  }

  if (tone === "primary") {
    return `${base} text-primary hover:bg-primary/10 hover:text-primary`;
  }

  return `${base} text-muted-foreground hover:bg-muted/50 hover:text-foreground`;
}

function GridActionButton({
  label,
  icon,
  disabled,
  tone = "default",
  onClick,
}: GridActionButtonProps) {
  return (
    <button
      type="button"
      aria-label={label}
      title={label}
      disabled={disabled}
      onClick={(e) => {
        e.stopPropagation();
        onClick?.();
      }}
      className={gridActionButtonClass(tone, disabled)}
    >
      {icon}
    </button>
  );
}

function GridAction(props: Props) {
  const { t } = useTranslation();
  const {
    editHandler,
    deleteHandler,
    actionHandler,
    id,
    showAction,
    actionName,
    showEdit,
    showDelete,
    showAdd,
    addHandler,
    record,
  } = props;
  useSignals();
  const dialogMessage = t("Are you sure you want to {{action}}?", {
    action: actionName ? String(actionName).toLowerCase() : t("delete"),
  });
  const [showDialog, setshowDialog] = useState(false);
  const permissions = store.PermissionData.value;
  const location = useLocation();
  const pathName = location.pathname;

  // When no permission data is loaded (permission system not configured), don't gate
  // actions — otherwise every Edit/Delete would be disabled. Real gating still applies
  // once PermissionData is populated.
  const hasPermissions = Array.isArray(permissions) && permissions.length > 0;
  const canAdd = permissions?.find(
    (b) => pathName.includes(b.link as string) && b.canAdd == true,
  );
  const canEdit = permissions?.find(
    (b) => pathName.includes(b.link as string) && b.canEdit == true,
  );
  const canDelete = permissions?.find(
    (b) => pathName.includes(b.link as string) && b.canDelete == true,
  );

  const disableAdd = hasPermissions && !canAdd;
  const disableEdit = hasPermissions && !canEdit;
  const disableDelete = hasPermissions && !canDelete;
  const isRejectAction = actionName === "Reject";

  return (
    <>
      <DialogModal
        visible={showDialog}
        title={t("Confirm")}
        onClose={setshowDialog}
        variant={!actionName || isRejectAction ? "destructive" : "default"}
        okLabel={actionName ? String(actionName) : t("Delete")}
        onOk={() => {
          if (actionName) actionHandler?.(id);
          else deleteHandler?.(id);
        }}
      >
        <span>{dialogMessage}</span>
      </DialogModal>

      <div
        className="flex items-center justify-end gap-0.5"
        role="group"
        aria-label={t("Row actions")}
        onClick={(e) => e.stopPropagation()}
      >
        {showAdd ? (
          <Tooltip message={t("Add Child")}>
            <GridActionButton
              label={t("Add Child")}
              tone="primary"
              disabled={disableAdd}
              icon={<Plus className="h-4 w-4" strokeWidth={2} />}
              onClick={() => addHandler?.(record)}
            />
          </Tooltip>
        ) : null}

        {showAction ? (
          <Tooltip message={actionName as string}>
            <GridActionButton
              label={String(actionName)}
              tone={isRejectAction ? "danger" : "primary"}
              icon={
                isRejectAction ? (
                  <FileX className="h-4 w-4" strokeWidth={2} />
                ) : (
                  <TableCellsSplit className="h-4 w-4" strokeWidth={2} />
                )
              }
              onClick={() => setshowDialog(true)}
            />
          </Tooltip>
        ) : null}

        {showEdit ? (
          <Tooltip message={t("Edit")}>
            <GridActionButton
              label={t("Edit")}
              tone="primary"
              disabled={disableEdit}
              icon={<Pencil className="h-4 w-4" strokeWidth={2} />}
              onClick={() => editHandler?.(id)}
            />
          </Tooltip>
        ) : null}

        {showDelete ? (
          <Tooltip message={t("Delete")}>
            <GridActionButton
              label={t("Delete")}
              tone="danger"
              disabled={disableDelete}
              icon={<Trash className="h-4 w-4 text-red-500" strokeWidth={2} />}
              onClick={() => setshowDialog(true)}
            />
          </Tooltip>
        ) : null}
      </div>
    </>
  );
}

GridAction.defaultProps = {
  showAdd: false,
  showEdit: true,
  showDelete: true,
  showPost: false,
  showUnPost: false,
};

export default GridAction;
