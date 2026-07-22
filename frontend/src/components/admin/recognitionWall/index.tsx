"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { Trophy, ChevronLeft, ChevronRight, Coins, BadgeDollarSign, ScrollText, Medal } from "lucide-react";
import getRecognitionWall from "@/services/admin/recognitionWall/getAll";
import type { RecognitionWallItemModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";
import Loading from "../../common/loader/loader";

const PAGE_SIZE = 12;

const KIND_ICON: Record<string, typeof Medal> = {
  Badge: Medal,
  Certificate: ScrollText,
  GiftCard: Coins,
  MonetaryBonus: BadgeDollarSign,
};

function WallCard({ item }: { item: RecognitionWallItemModel }) {
  const { t } = useTranslation();
  const Kind = KIND_ICON[item.rewardKind ?? ""] ?? Medal;
  const color = item.badgeColor || "var(--color-primary, #f59e0b)";
  return (
    <div className="flex flex-col rounded-lg border border-border bg-card p-4">
      <div className="mb-2 flex items-center gap-2">
        <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full" style={{ backgroundColor: `${color}22`, color }}>
          <Kind className="h-4.5 w-4.5" size={18} />
        </span>
        <div className="min-w-0">
          <p className="truncate text-sm font-semibold text-foreground">{item.employeeName || t("Colleague")}</p>
          <p className="truncate text-xs font-medium" style={{ color }}>{item.badgeName}</p>
        </div>
      </div>
      {item.citation && <p className="mb-2 line-clamp-3 flex-1 text-xs text-muted">“{item.citation}”</p>}
      <p className="text-right text-[11px] text-muted">{(item.recognizedOn || "").slice(0, 10)}</p>
    </div>
  );
}

/** HC184 — the company-wide public recognition feed (visible to every employee). */
function RecognitionWall() {
  const { t } = useTranslation();
  const [page, setPage] = useState(0);

  const { data, isLoading } = useQuery({
    queryKey: ["recognitionWall", page],
    queryFn: () => getRecognitionWall({ ...parameterInitialData, skip: page * PAGE_SIZE, take: PAGE_SIZE }),
    placeholderData: keepPreviousData,
  });

  const items = data?.data ?? [];
  const total = data?.total ?? 0;
  const pages = Math.max(1, Math.ceil(total / PAGE_SIZE));

  return (
    <div className="flex h-full min-h-0 flex-col p-3">
      <div className="mb-3 flex items-center gap-2">
        <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><Trophy className="h-5 w-5" /></span>
        <div>
          <h1 className="text-base font-semibold text-foreground">{t("Recognition Wall")}</h1>
          <p className="text-xs text-muted">{t("Colleagues recognized across the company — congratulate them!")}</p>
        </div>
      </div>

      {isLoading ? (
        <Loading />
      ) : items.length === 0 ? (
        <p className="rounded-lg border border-dashed border-border bg-card/40 p-8 text-center text-sm text-muted">
          {t("No public recognitions yet — they will appear here as awards are granted.")}
        </p>
      ) : (
        <>
          <div className="grid min-h-0 flex-1 auto-rows-min grid-cols-1 gap-3 overflow-auto sm:grid-cols-2 lg:grid-cols-3">
            {items.map((i) => <WallCard key={i.id} item={i} />)}
          </div>
          {pages > 1 && (
            <div className="mt-3 flex items-center justify-end gap-2 text-xs text-muted">
              <span>{t("Page")} {page + 1} / {pages}</span>
              <button type="button" disabled={page === 0} onClick={() => setPage((p) => p - 1)} className="rounded border border-border p-1 disabled:opacity-40"><ChevronLeft size={14} /></button>
              <button type="button" disabled={page + 1 >= pages} onClick={() => setPage((p) => p + 1)} className="rounded border border-border p-1 disabled:opacity-40"><ChevronRight size={14} /></button>
            </div>
          )}
        </>
      )}
    </div>
  );
}

export default memo(RecognitionWall);
