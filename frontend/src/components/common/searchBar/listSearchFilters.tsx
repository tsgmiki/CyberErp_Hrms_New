import { memo, useCallback, useEffect, useMemo, useState, type ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { SlidersHorizontal } from "lucide-react";
import type { ParameterModel } from "@/models";
import ButtonField from "@/components/ui/buttonField";
import Modal from "@/components/common/modal";
import { ListFilterFields } from "./listFilterFields";
import {
  applyListFilterParams,
  clearListFilterParams,
  countActiveListFilters,
  type ListFilterDefinition,
} from "./listFilterTypes";

interface ListSearchFiltersProps {
  filters: ListFilterDefinition[];
  param?: ParameterModel;
  setParam?: (updater: (prev: ParameterModel) => ParameterModel) => void;
  disabled?: boolean;
  modalTitle?: string;
  modalDescription?: string;
  /** Override default field rendering (e.g. dropdowns with lookup param per field). */
  renderFields?: (ctx: {
    draft: ParameterModel;
    patchDraft: (patch: Partial<ParameterModel>) => void;
    disabled: boolean;
  }) => ReactNode;
  onClearDraft?: (draft: ParameterModel) => ParameterModel;
}

function ListSearchFilters({
  filters,
  param,
  setParam,
  disabled = false,
  modalTitle,
  modalDescription,
  renderFields,
  onClearDraft,
}: ListSearchFiltersProps) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const [draft, setDraft] = useState<ParameterModel | undefined>(param);

  const activeCount = useMemo(
    () => countActiveListFilters(param, filters),
    [param, filters],
  );

  useEffect(() => {
    if (open && param) {
      setDraft(param);
    }
  }, [open, param]);

  const patchDraft = useCallback((patch: Partial<ParameterModel>) => {
    setDraft((prev) => (prev ? { ...prev, ...patch } : prev));
  }, []);

  const handleApply = useCallback(() => {
    if (!setParam || !draft) return;
    setParam((prev) => applyListFilterParams(prev, draft, filters));
    setOpen(false);
  }, [draft, filters, setParam]);

  const handleClearDraft = useCallback(() => {
    setDraft((prev) => {
      if (!prev) return prev;
      const cleared = clearListFilterParams(prev, filters);
      return onClearDraft ? onClearDraft(cleared) : cleared;
    });
  }, [filters, onClearDraft]);

  const handleClose = useCallback(() => {
    setOpen(false);
  }, []);

  if (!param || !setParam || filters.length === 0) return null;

  const title = modalTitle ?? t("More filters");
  const description =
    modalDescription ??
    t("Refine results with date, status, and related criteria.");

  return (
    <>
      <div className="relative shrink-0">
        <ButtonField
          value="More filters"
          variant="outline"
          htmlType="button"
          disabled={disabled}
          onClick={() => setOpen(true)}
          icon={<SlidersHorizontal className="h-4 w-4" />}
          className="!h-9 shrink-0 !px-3 !text-xs"
        />
        {activeCount > 0 ? (
          <span
            className="absolute -right-1 -top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-primary px-1 text-[10px] font-semibold text-on-accent"
            aria-label={t("{{count}} active filters", { count: activeCount })}
          >
            {activeCount}
          </span>
        ) : null}
      </div>

      <Modal
        visible={open}
        onClose={() => handleClose()}
        title={title}
        description={description}
        size="lg"
        footer={
          <>
            <ButtonField
              value="Clear all"
              variant="ghost"
              htmlType="button"
              disabled={disabled}
              onClick={handleClearDraft}
              className="mr-auto max-md:mr-0 max-md:w-full"
            />
            <ButtonField
              value="Cancel"
              variant="secondary"
              htmlType="button"
              disabled={disabled}
              onClick={handleClose}
            />
            <ButtonField
              value="Apply filters"
              variant="primary"
              htmlType="button"
              disabled={disabled}
              onClick={handleApply}
            />
          </>
        }
      >
        {draft ? (
          renderFields ? (
            renderFields({ draft, patchDraft, disabled })
          ) : (
            <ListFilterFields
              filters={filters}
              param={draft}
              onPatch={patchDraft}
              disabled={disabled}
            />
          )
        ) : null}
      </Modal>
    </>
  );
}

export default memo(ListSearchFilters);
