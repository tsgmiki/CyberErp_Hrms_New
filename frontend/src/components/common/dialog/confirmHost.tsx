"use client";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import DialogModal from "./index";
import { getConfirm, resolveConfirm, subscribe } from "./confirmStore";

/**
 * Single mount point (in App) that renders the active imperative {@link confirm} dialog
 * through our standard {@link DialogModal}. Mirrors the Toaster's store-subscription pattern.
 */
export function ConfirmHost() {
  const { t } = useTranslation();
  const [state, setState] = useState(getConfirm);

  useEffect(() => subscribe(() => setState(getConfirm())), []);

  if (!state) return null;

  return (
    <DialogModal
      visible
      title={state.title ?? t("Confirm")}
      variant={state.variant ?? "destructive"}
      okLabel={state.confirmLabel}
      cancelLabel={state.cancelLabel}
      onOk={() => resolveConfirm(true)}
      // Fires on cancel, backdrop, Escape, or the ✕. onOk resolves first, so this no-ops after confirm.
      onClose={() => resolveConfirm(false)}
    >
      {state.message}
    </DialogModal>
  );
}

export default ConfirmHost;
