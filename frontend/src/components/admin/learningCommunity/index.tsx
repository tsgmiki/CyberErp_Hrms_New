"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { keepPreviousData, useQuery, useQueryClient } from "@tanstack/react-query";
import { UsersRound, Plus, X, Save } from "lucide-react";
import { getAllCommunities, saveCommunity, joinCommunity, leaveCommunity, deleteCommunity } from "@/services/admin/learningCommunity";
import type { LearningCommunityModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";
import Loading from "../../common/loader/loader";
import DiscussionView from "./discussion";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

/** One community card: counts, membership state, join/leave/open. */
function CommunityCard({ item, onAction, onOpen }: {
  item: LearningCommunityModel;
  onAction: (fn: () => Promise<{ ok: boolean; message: string }>) => void;
  onOpen: () => void;
}) {
  const { t } = useTranslation();
  return (
    <div className={`flex flex-col rounded-lg border border-border bg-card p-4 ${item.isActive === false ? "opacity-60" : ""}`}>
      <div className="mb-1 flex items-start justify-between gap-2">
        <button type="button" onClick={onOpen} className="min-w-0 text-left">
          <p className="truncate text-sm font-semibold text-foreground hover:text-primary">{item.name}</p>
          {item.courseName && <p className="truncate text-xs text-primary">{item.courseName}</p>}
        </button>
        {item.isModerator && <span className="shrink-0 rounded-full bg-warning/15 px-2 py-0.5 text-[11px] font-semibold text-warning">{t("Moderator")}</span>}
        {item.isActive === false && <span className="shrink-0 rounded-full bg-muted/30 px-2 py-0.5 text-[11px] font-semibold text-muted">{t("Inactive")}</span>}
      </div>
      {item.description && <p className="mb-2 line-clamp-2 flex-1 text-xs text-muted">{item.description}</p>}
      <div className="flex items-center justify-between text-xs text-muted">
        <span>{item.memberCount ?? 0} {t("members")} · {item.postCount ?? 0} {t("posts")}</span>
        <span className="flex items-center gap-1.5">
          <button type="button" onClick={onOpen} className="rounded-md border border-border px-2 py-1 text-xs font-semibold text-foreground hover:bg-secondary/40">
            {t("Open")}
          </button>
          {item.isMember ? (
            !item.isModerator && (
              <button type="button" onClick={() => onAction(() => leaveCommunity(item.id!))}
                className="rounded-md border border-border px-2 py-1 text-xs font-semibold text-muted hover:text-error">
                {t("Leave")}
              </button>
            )
          ) : (
            item.isActive !== false && (
              <button type="button" onClick={() => onAction(() => joinCommunity(item.id!))}
                className="rounded-md bg-primary px-2 py-1 text-xs font-semibold text-on-accent hover:opacity-90">
                {t("Join")}
              </button>
            )
          )}
          {item.isModerator && (
            <button type="button" onClick={() => onAction(() => deleteCommunity(item.id!))}
              className="rounded-md border border-border px-2 py-1 text-xs font-semibold text-muted hover:text-error">
              {t("Delete")}
            </button>
          )}
        </span>
      </div>
    </div>
  );
}

/** HC198/HC199 — learning communities: browse, found, join, and discuss. */
function LearningCommunity() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [openCommunity, setOpenCommunity] = useState<LearningCommunityModel | null>(null);
  const [showCreate, setShowCreate] = useState(false);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [busy, setBusy] = useState(false);
  const [msg, setMsg] = useState("");

  const [param] = useState({ ...parameterInitialData, take: 60 });
  const { data, isLoading } = useQuery({
    queryKey: ["learningCommunities", param],
    queryFn: () => getAllCommunities(param),
    placeholderData: keepPreviousData,
  });
  const items = data?.data ?? [];

  const refresh = () => queryClient.invalidateQueries({ queryKey: ["learningCommunities"] });

  const runAction = async (fn: () => Promise<{ ok: boolean; message: string }>) => {
    const res = await fn();
    setMsg(res.message);
    if (res.ok) refresh();
  };

  const create = async () => {
    setBusy(true);
    const res = await saveCommunity({ name, description: description || undefined, isActive: true });
    setBusy(false);
    setMsg(res.message);
    if (res.ok) {
      setName("");
      setDescription("");
      setShowCreate(false);
      refresh();
    }
  };

  if (openCommunity) {
    return <DiscussionView community={openCommunity} onBack={() => { setOpenCommunity(null); refresh(); }} />;
  }

  return (
    <div className="flex h-full min-h-0 flex-col p-3">
      <div className="mb-3 flex items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><UsersRound className="h-5 w-5" /></span>
          <div>
            <h1 className="text-base font-semibold text-foreground">{t("Learning Communities")}</h1>
            <p className="text-xs text-muted">{t("Peer groups for knowledge sharing — join a community or start your own.")}</p>
          </div>
        </div>
        <button type="button" onClick={() => setShowCreate((v) => !v)}
          className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-sm font-semibold text-on-accent hover:opacity-90">
          {showCreate ? <X size={14} /> : <Plus size={14} />} {showCreate ? t("Cancel") : t("New Community")}
        </button>
      </div>

      {msg && <p className="mb-2 rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{msg}</p>}

      {showCreate && (
        <div className="mb-3 rounded-lg border border-border bg-card p-4">
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <div>
              <label className={LABEL}>{t("Name")} *</label>
              <input type="text" className={INPUT} placeholder={t("e.g. SQL Guild")} value={name} onChange={(e) => setName(e.target.value)} />
            </div>
            <div>
              <label className={LABEL}>{t("Description")}</label>
              <input type="text" className={INPUT} placeholder={t("What the group is about")} value={description} onChange={(e) => setDescription(e.target.value)} />
            </div>
          </div>
          <div className="mt-3 flex justify-end">
            <button type="button" disabled={busy || !name.trim()} onClick={create}
              className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
              <Save size={14} /> {busy ? t("Creating…") : t("Create Community")}
            </button>
          </div>
        </div>
      )}

      {isLoading ? (
        <Loading />
      ) : items.length === 0 ? (
        <p className="rounded-lg border border-dashed border-border bg-card/40 p-8 text-center text-sm text-muted">
          {t("No communities yet — start the first one!")}
        </p>
      ) : (
        <div className="grid min-h-0 flex-1 auto-rows-min grid-cols-1 gap-3 overflow-auto sm:grid-cols-2 lg:grid-cols-3">
          {items.map((c) => (
            <CommunityCard key={c.id} item={c} onAction={runAction} onOpen={() => setOpenCommunity(c)} />
          ))}
        </div>
      )}
    </div>
  );
}

export default memo(LearningCommunity);
