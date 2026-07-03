import { Fragment, memo, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import {
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
} from "lucide-react";

interface PaginationProps {
  take?: number;
  skip?: number;
  recordCount: number;
  paginationHandler: (val: { take: number; skip: number }) => void;
  pageSizeOptions?: number[];
}

const DEFAULT_PAGE_SIZES = [10, 15, 25, 50, 100];

const Pagination = memo(function Pagination({
  take = 10,
  skip = 0,
  recordCount,
  paginationHandler,
  pageSizeOptions = DEFAULT_PAGE_SIZES,
}: PaginationProps) {
  const { t } = useTranslation();

  const [currentPage, setCurrentPage] = useState(1);
  const [nPages, setnPages] = useState(1);
  const [recordsPerPage, setRecordPerPage] = useState(take);

  useEffect(() => {
    setRecordPerPage(take);
  }, [take]);

  useEffect(() => {
    const nPage = Math.max(1, Math.ceil(recordCount / recordsPerPage) || 1);
    setnPages(nPage);
  }, [recordCount, recordsPerPage]);

  useEffect(() => {
    const page = Math.floor(skip / recordsPerPage) + 1;
    setCurrentPage(Math.min(Math.max(1, page), nPages));
  }, [skip, recordsPerPage, nPages]);

  const goToFirstPage = () => {
    if (currentPage !== 1) {
      paginationHandler({ skip: 0, take: recordsPerPage });
      setCurrentPage(1);
    }
  };

  const goToPrevPage = () => {
    if (currentPage !== 1) {
      paginationHandler({
        skip: (currentPage - 2) * recordsPerPage,
        take: recordsPerPage,
      });
      setCurrentPage(currentPage - 1);
    }
  };

  const goToNextPage = () => {
    if (currentPage !== nPages) {
      paginationHandler({
        skip: currentPage * recordsPerPage,
        take: recordsPerPage,
      });
      setCurrentPage(currentPage + 1);
    }
  };

  const goToLastPage = () => {
    if (currentPage !== nPages) {
      paginationHandler({
        skip: (nPages - 1) * recordsPerPage,
        take: recordsPerPage,
      });
      setCurrentPage(nPages);
    }
  };

  const goToPage = (page: number) => {
    if (page === currentPage || page < 1 || page > nPages) return;
    paginationHandler({
      skip: (page - 1) * recordsPerPage,
      take: recordsPerPage,
    });
    setCurrentPage(page);
  };

  const visiblePages = (() => {
    if (nPages <= 7) {
      return Array.from({ length: nPages }, (_, i) => i + 1);
    }
    const pages = new Set<number>([1, nPages, currentPage]);
    if (currentPage > 1) pages.add(currentPage - 1);
    if (currentPage < nPages) pages.add(currentPage + 1);
    return Array.from(pages).sort((a, b) => a - b);
  })();

  const handlePageSizeChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const newSize = parseInt(e.target.value, 10);
    setRecordPerPage(newSize);
    setCurrentPage(1);
    paginationHandler({ skip: 0, take: newSize });
  };

  const startItem =
    recordCount > 0 ? (currentPage - 1) * recordsPerPage + 1 : 0;
  const endItem = Math.min(currentPage * recordsPerPage, recordCount);

  const buttonClasses =
    "rounded-lg p-2 text-muted transition-colors hover:bg-secondary disabled:cursor-not-allowed disabled:opacity-50";

  return (
    <div className="mx-1 flex flex-wrap items-center justify-between gap-3 rounded-lg border border-border bg-card px-3 py-2">
      <div className="flex flex-wrap items-center gap-3">
        <p className="text-sm text-muted">
          {t("Showing")}{" "}
          <span className="font-semibold text-foreground">
            {startItem}-{endItem}
          </span>
          <span className="mx-1">{t("of")}</span>
          <span className="font-semibold text-foreground">{recordCount}</span>
        </p>

        <label className="flex items-center gap-2 text-xs text-muted">
          <span className="max-sm:sr-only">{t("Rows per page")}</span>
          <select
            value={recordsPerPage}
            onChange={handlePageSizeChange}
            className="rounded-md border border-border bg-secondary px-2 py-1 text-sm text-foreground outline-none focus:border-primary"
            aria-label={t("Rows per page")}
          >
            {pageSizeOptions.map((size) => (
              <option key={size} value={size}>
                {size}
              </option>
            ))}
          </select>
        </label>
      </div>

      <div className="flex items-center gap-1">
        <button
          type="button"
          onClick={goToFirstPage}
          disabled={currentPage === 1}
          className={buttonClasses}
          aria-label={t("First page")}
        >
          <ChevronsLeft className="h-4 w-4" />
        </button>

        <button
          type="button"
          onClick={goToPrevPage}
          disabled={currentPage === 1}
          className={buttonClasses}
          aria-label={t("Previous page")}
        >
          <ChevronLeft className="h-4 w-4" />
        </button>

        {nPages > 1 &&
          visiblePages.map((page, index) => {
            const prev = visiblePages[index - 1];
            const showEllipsis = index > 0 && page - prev > 1;
            return (
              <Fragment key={page}>
                {showEllipsis && (
                  <span className="px-1 text-xs text-muted">…</span>
                )}
                <button
                  type="button"
                  onClick={() => goToPage(page)}
                  className={`h-8 min-w-8 rounded-md px-2 text-xs font-medium transition-colors ${
                    currentPage === page
                      ? "bg-primary text-on-accent"
                      : "text-muted hover:bg-secondary"
                  }`}
                >
                  {page}
                </button>
              </Fragment>
            );
          })}

        <button
          type="button"
          onClick={goToNextPage}
          disabled={currentPage === nPages || recordCount === 0}
          className={buttonClasses}
          aria-label={t("Next page")}
        >
          <ChevronRight className="h-4 w-4" />
        </button>

        <button
          type="button"
          onClick={goToLastPage}
          disabled={currentPage === nPages || recordCount === 0}
          className={buttonClasses}
          aria-label={t("Last page")}
        >
          <ChevronsRight className="h-4 w-4" />
        </button>
      </div>
    </div>
  );
});

Pagination.displayName = "Pagination";
export default Pagination;
