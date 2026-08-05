import type { OrgUnitTreeNode } from "@/models";

/** Card + spacing geometry (shared by screen and PDF). */
export const NODE_W = 236;
export const NODE_H = 92;
export const H_GAP = 26; // gap between sibling cards along the breadth axis
export const V_GAP = 56; // gap between levels along the depth axis

export type Orient = "TB" | "LR";

export interface LaidNode {
  id: string;
  name: string;
  code: string;
  unitType: string;
  headcount?: number | null;
  /** top-left of the card in layout space */
  x: number;
  y: number;
  depth: number;
  hasChildren: boolean;
  collapsed: boolean;
  childCount: number;
}

export interface Edge {
  from: LaidNode;
  to: LaidNode;
}

export interface Layout {
  nodes: LaidNode[];
  edges: Edge[];
  width: number;
  height: number;
}

interface Work {
  src?: OrgUnitTreeNode;
  id: string;
  name: string;
  code: string;
  unitType: string;
  headcount?: number | null;
  children: Work[];
  depth: number;
  breadth: number; // center position along the breadth axis (in "slots")
  hasChildren: boolean;
  collapsed: boolean;
}

const SYN_ROOT = "__org_root__";

/**
 * Tidy layout for fixed-size cards. Leaves (or collapsed nodes) are packed into sequential slots
 * along the breadth axis; every ancestor is centered over the span of its visible children. Because
 * sibling subtrees occupy disjoint, contiguous slot ranges, cards at a given level are always ≥ one
 * slot (NODE_W + gap) apart — so nodes can never overlap, for any structure. O(n).
 */
export function computeLayout(
  roots: OrgUnitTreeNode[],
  collapsed: Set<string>,
  orient: Orient,
): Layout {
  // Wrap multiple roots under a single synthetic "Organization" node so the chart has one apex.
  const multiRoot = roots.length > 1;

  const build = (n: OrgUnitTreeNode, depth: number): Work => {
    const isCollapsed = collapsed.has(n.id);
    const kids = n.children ?? [];
    return {
      src: n,
      id: n.id,
      name: n.name,
      code: n.code,
      unitType: n.unitType,
      headcount: n.allocatedHeadcount,
      children: isCollapsed ? [] : kids.map((c) => build(c, depth + 1)),
      depth,
      breadth: 0,
      hasChildren: kids.length > 0,
      collapsed: isCollapsed && kids.length > 0,
    };
  };

  let forest: Work[];
  if (multiRoot) {
    forest = [
      {
        id: SYN_ROOT,
        name: "Organization",
        code: "",
        unitType: "",
        children: roots.map((r) => build(r, 1)),
        depth: 0,
        breadth: 0,
        hasChildren: true,
        collapsed: false,
      },
    ];
  } else {
    forest = roots.map((r) => build(r, 0));
  }

  // Assign breadth centers: post-order, leaves take the next slot, parents center over children.
  let slot = 0;
  const assign = (w: Work) => {
    if (w.children.length === 0) {
      w.breadth = slot;
      slot += 1;
      return;
    }
    w.children.forEach(assign);
    w.breadth = (w.children[0].breadth + w.children[w.children.length - 1].breadth) / 2;
  };
  forest.forEach(assign);

  const nodes: LaidNode[] = [];
  const edges: Edge[] = [];
  const byId = new Map<string, LaidNode>();

  const slotStep = NODE_W + H_GAP; // breadth pitch
  const levelStep = NODE_H + V_GAP; // depth pitch

  const place = (w: Work, parent?: LaidNode) => {
    // Map (depth, breadth) → (x, y) per orientation.
    const bPix = w.breadth * (orient === "TB" ? slotStep : NODE_H + H_GAP);
    const dPix = w.depth * (orient === "TB" ? levelStep : NODE_W + V_GAP);
    const laid: LaidNode = {
      id: w.id,
      name: w.name,
      code: w.code,
      unitType: w.unitType,
      headcount: w.headcount,
      x: orient === "TB" ? bPix : dPix,
      y: orient === "TB" ? dPix : bPix,
      depth: w.depth,
      hasChildren: w.hasChildren,
      collapsed: w.collapsed,
      childCount: w.src?.children?.length ?? w.children.length,
    };
    nodes.push(laid);
    byId.set(laid.id, laid);
    if (parent) edges.push({ from: parent, to: laid });
    w.children.forEach((c) => place(c, laid));
  };
  forest.forEach((w) => place(w));

  // Normalize to a positive origin and measure the canvas.
  let minX = Infinity;
  let minY = Infinity;
  let maxX = -Infinity;
  let maxY = -Infinity;
  for (const n of nodes) {
    minX = Math.min(minX, n.x);
    minY = Math.min(minY, n.y);
    maxX = Math.max(maxX, n.x + NODE_W);
    maxY = Math.max(maxY, n.y + NODE_H);
  }
  const pad = 40;
  for (const n of nodes) {
    n.x += pad - minX;
    n.y += pad - minY;
  }

  return {
    nodes,
    edges,
    width: maxX - minX + pad * 2,
    height: maxY - minY + pad * 2,
  };
}

/** Uppercased 1–2 letter initials from a unit name (for the card avatar). */
export function initials(name: string): string {
  const words = name.trim().split(/\s+/).filter(Boolean);
  if (words.length === 0) return "•";
  if (words.length === 1) return words[0].slice(0, 2).toUpperCase();
  return (words[0][0] + words[words.length - 1][0]).toUpperCase();
}

/** Greedy word-wrap into at most `maxLines` lines of ~`maxChars`, last line ellipsised if clipped. */
export function wrapLines(name: string, maxChars: number, maxLines: number): string[] {
  const words = name.trim().split(/\s+/).filter(Boolean);
  const lines: string[] = [];
  let cur = "";
  for (const word of words) {
    const cand = cur ? `${cur} ${word}` : word;
    if (cand.length <= maxChars) {
      cur = cand;
    } else if (!cur) {
      // A single token longer than a line — hard-split it.
      lines.push(word.slice(0, maxChars));
      cur = word.slice(maxChars);
    } else {
      lines.push(cur);
      cur = word;
    }
    if (lines.length === maxLines) break;
  }
  if (lines.length < maxLines && cur) lines.push(cur);
  // If content remains beyond maxLines, ellipsise the last emitted line.
  const consumed = lines.join(" ").length;
  if (consumed < name.trim().length && lines.length > 0) {
    const last = lines[maxLines - 1] ?? lines[lines.length - 1];
    const idx = lines.lastIndexOf(last);
    lines[idx] = `${last.slice(0, Math.max(0, maxChars - 1)).trimEnd()}…`;
  }
  return lines.length ? lines : [name];
}
