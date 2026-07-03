import { type ReactNode } from "react";
import OptionMenus from "./optionMenu/optionMenu";
import { Settings, ArrowLeft, Plus } from "lucide-react";
import { useTranslation } from "react-i18next";

interface InventoryLayoutProps {
  children: ReactNode;
  containerClass?: string;
  title: string;
  headerDescription?: string;
  headerIcon?: ReactNode;
  onList: () => void;
  onSetting?: () => void;
  onAdd?: () => void;
  showForm: boolean;
  showEnhancedHeader?: boolean;
  onAction?: (e: React.MouseEvent<HTMLButtonElement>) => void;
  showAction?: boolean;
  actionName?: string;
  showBack?: boolean;
  hideAdd?: boolean;
  hideBack?: boolean;
  hideSetting?: boolean;
  category?: string;
  tableTitle?: string;
  tableDescription?: string;
  tableIcon?: ReactNode;
  optionHander?: (value: string) => void;
  menuItems?: { name: string; value: string }[];
  hideTitle?: boolean;
  actionBar?: ReactNode;
}

function InventoryLayout({
  children,
  containerClass = "",
  title,
  onList,
  onAdd = () => {},
  onAction,
  onSetting,
  showAction,
  showForm,
  actionName,
  hideAdd,
  hideBack,
  hideSetting,
  category,
  optionHander,
  menuItems,
  actionBar,
  headerIcon,
  headerDescription,
  hideTitle,
  tableIcon,
}: InventoryLayoutProps) {
  const { t } = useTranslation();

  return (
    <div className="flex h-full w-full flex-col transition-colors duration-300">
      <div className="sticky top-0 z-10 shrink-0 transition-all duration-300">
        <div className="px-4 py-2">
          <div className="flex min-h-11 flex-wrap items-center justify-between gap-4">
            <div className="flex min-w-0 flex-1 items-center gap-2 text-left">
              {showForm && !hideBack && (
                <button
                  type="button"
                  onClick={onList}
                  className="shrink-0 rounded-md p-1.5 text-muted transition-colors hover:bg-secondary"
                  title="Discard changes and back to list"
                >
                  <ArrowLeft size={18} />
                </button>
              )}

              <div className="flex min-w-0 flex-col">
                <nav className="mb-0.5 flex items-center gap-1 overflow-hidden whitespace-nowrap text-[13px] text-muted">
                  {!hideTitle && (
                    <span className="flex items-center gap-2">
                      {headerIcon || tableIcon ? (
                        <span className="scale-75 opacity-70">
                          {headerIcon || tableIcon}
                        </span>
                      ) : null}
                      <h1 className="truncate text-[17px] font-bold tracking-tight text-foreground">
                        {t(title) || title}
                      </h1>
                      {!hideSetting && (
                        <button
                          type="button"
                          onClick={() => onSetting?.()}
                          className="rounded-md p-1 text-muted transition-colors hover:bg-secondary hover:text-primary"
                          title="Configuration"
                        >
                          <Settings size={15} />
                        </button>
                      )}
                    </span>
                  )}
                </nav>
                {!hideTitle && (headerDescription || category) && (
                  <p className="hidden truncate px-1 text-[11px] font-medium text-muted opacity-70 sm:block">
                    {headerDescription || category}
                  </p>
                )}
              </div>
            </div>

            <div className="ml-auto flex shrink-0 items-center gap-2">
              {!showForm && menuItems && (
                <OptionMenus
                  optionHander={optionHander as never}
                  menu={menuItems as never}
                />
              )}

              {showAction && actionName && (
                <button
                  type="button"
                  onClick={onAction}
                  className="h-9 rounded border border-border bg-secondary px-4 text-sm font-semibold text-primary shadow-sm transition-all duration-200 hover:bg-primary active:scale-95"
                >
                  {actionName}
                </button>
              )}

              {!showForm && !hideAdd && (
                <button
                  type="button"
                  onClick={onAdd}
                  className="flex h-9 w-9 shrink-0 items-center justify-center rounded border border-transparent bg-primary text-on-accent shadow-sm transition-all duration-200 hover:opacity-90 active:scale-95"
                  title={t("Add")}
                  aria-label={t("Add")}
                >
                  <Plus size={18} strokeWidth={3} />
                </button>
              )}
            </div>
          </div>
        </div>

        {actionBar && (
          <div className="animate-in fade-in slide-in-from-top-1 px-4 pb-2.5">
            <div className="border-t border-border pt-2.5">{actionBar}</div>
          </div>
        )}
      </div>

      <div className={`w-full flex-1 overflow-auto p-2 ${containerClass}`}>
        <div className="h-full w-full overflow-hidden rounded-lg transition-all duration-300">
          <div className="h-full w-full overflow-auto">{children}</div>
        </div>
      </div>
    </div>
  );
}

export default InventoryLayout;
