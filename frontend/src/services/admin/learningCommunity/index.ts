import { createPagedQuery } from "@/template/createPagedQuery";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { LearningCommunityModel, CommunityPostModel } from "@/models";
import type ParameterModel from "@/models/ParameterModel";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Active communities (HR also sees deactivated ones) with counts + caller flags. */
export const getAllCommunities = createPagedQuery<LearningCommunityModel>("LearningCommunity");

/** Topic feed (newest first) with inline replies — reading is open to every employee. */
export const getCommunityPosts = async (communityId: string, param: ParameterModel) => {
  const res = await fetch(
    `${API_BASE_URL}/LearningCommunity/${communityId}/posts?skip=${param.skip}&take=${param.take}`,
    { credentials: "include" },
  );
  if (!res.ok) throw new Error("Failed to load the discussion");
  return (await res.json()) as { total: number; data: CommunityPostModel[] };
};

async function jsonAction(method: string, path: string, body?: unknown): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/${path}`, {
    method,
    credentials: "include",
    headers: body ? { "Content-Type": "application/json" } : undefined,
    body: body ? JSON.stringify(body) : undefined,
  });
  const text = await res.text();
  let message = res.ok ? "Done" : "Request failed";
  try {
    const parsed = JSON.parse(text);
    message = errorMessageParser(parsed.errors || parsed) || parsed?.message || message;
    if (res.ok) message = parsed?.message ?? "Done";
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}

export const saveCommunity = async (model: LearningCommunityModel): Promise<{ ok: boolean; message: string }> => {
  const isUpdate = !!model.id;
  const res = await fetch(`${API_BASE_URL}/LearningCommunity`, {
    method: isUpdate ? "PUT" : "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(model),
  });
  const text = await res.text();
  const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
  if (!res.ok) return { ok: false, message: errorMessageParser(parsed.errors || parsed) };
  return { ok: true, message: parsed.message ?? "Saved" };
};

export const deleteCommunity = (id: string) => jsonAction("DELETE", `LearningCommunity/${id}`);
export const joinCommunity = (id: string) => jsonAction("POST", `LearningCommunity/${id}/join`);
export const leaveCommunity = (id: string) => jsonAction("POST", `LearningCommunity/${id}/leave`);

/** Posting needs membership; replies are one level deep. */
export const createCommunityPost = (communityId: string, content: string, parentPostId?: string) =>
  jsonAction("POST", `LearningCommunity/${communityId}/posts`, { content, parentPostId });

export const deleteCommunityPost = (postId: string) => jsonAction("DELETE", `LearningCommunity/posts/${postId}`);
