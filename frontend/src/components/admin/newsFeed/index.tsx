"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { Newspaper, Pin } from "lucide-react";
import { getAnnouncementFeed } from "@/services/admin/engagement";
import { parameterInitialData } from "@/constants/initialization";
import Loading from "../../common/loader/loader";

/** HC206 — the employee-facing news feed: announcements targeted to me (all ∪ my branch ∪ my unit chain). */
function NewsFeed() {
  const { t } = useTranslation();
  const [param] = useState({ ...parameterInitialData, take: 50 });
  const { data, isLoading } = useQuery({
    queryKey: ["announcementFeed", param],
    queryFn: () => getAnnouncementFeed(param),
    placeholderData: keepPreviousData,
  });
  const items = data?.data ?? [];

  return (
    <div className="flex h-full min-h-0 flex-col p-3">
      <div className="mb-3 flex items-center gap-2">
        <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><Newspaper className="h-5 w-5" /></span>
        <div>
          <h1 className="text-base font-semibold text-foreground">{t("News & Announcements")}</h1>
          <p className="text-xs text-muted">{t("Company news addressed to you — pinned items stay on top.")}</p>
        </div>
      </div>

      {isLoading ? (
        <Loading />
      ) : items.length === 0 ? (
        <p className="rounded-lg border border-dashed border-border bg-card/40 p-8 text-center text-sm text-muted">{t("No announcements right now.")}</p>
      ) : (
        <div className="min-h-0 flex-1 space-y-3 overflow-auto">
          {items.map((a) => (
            <article key={a.id} className={`rounded-lg border bg-card p-4 ${a.isPinned ? "border-warning/50" : "border-border"}`}>
              <div className="mb-1 flex items-start justify-between gap-2">
                <h2 className="text-sm font-semibold text-foreground">
                  {a.isPinned && <Pin size={12} className="mr-1.5 inline text-warning" />}{a.title}
                </h2>
                <span className="shrink-0 text-xs text-muted">{a.publishFrom ? String(a.publishFrom).slice(0, 10) : ""}</span>
              </div>
              <p className="whitespace-pre-wrap text-sm text-foreground">{a.body}</p>
            </article>
          ))}
        </div>
      )}
    </div>
  );
}

export default memo(NewsFeed);
