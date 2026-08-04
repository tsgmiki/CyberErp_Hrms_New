"use client";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  ArrowLeftRight,
  ArrowUpDown,
  Download,
  Maximize2,
  Minus,
  Plus,
  Scan,
  Search,
  X,
} from "lucide-react";
import type { OrgUnitTreeNode } from "@/models";
import getOrganizationTree from "@/services/admin/organizationUnit/getTree";
import Loading from "../loader/loader";
import OrgNodeCard from "./OrgNodeCard";
import { NODE_H, NODE_W, V_GAP, computeLayout, type LaidNode, type Orient } from "./layout";
import { hueFor, orgTheme } from "./palette";

/** Above this many units the chart opens collapsed to a readable depth. */
const AUTO_COLLAPSE_AT = 40;
const MAX_NODES_PER_LEVEL = 10;
const ZOOM_MIN = 0.3;
const ZOOM_MAX = 2;

function useIsDark() {
  const read = () => {
    if (typeof document === "undefined") return false;
    const attr = document.documentElement.getAttribute("data-theme");
    if (attr === "dark") return true;
    if (attr === "light") return false;
    return window.matchMedia?.("(prefers-color-scheme: dark)").matches ?? false;
  };
  const [dark, setDark] = useState(read);
  useEffect(() => {
    const onChange = () => setDark(read());
    const mq = window.matchMedia("(prefers-color-scheme: dark)");
    mq.addEventListener?.("change", onChange);
    const obs = new MutationObserver(onChange);
    obs.observe(document.documentElement, { attributes: true, attributeFilter: ["data-theme"] });
    return () => {
      mq.removeEventListener?.("change", onChange);
      obs.disconnect();
    };
  }, []);
  return dark;
}

/** Rounded orthogonal elbow connector between two cards, per orientation. */
function edgePath(from: LaidNode, to: LaidNode, orient: Orient): string {
  const R = 12;
  if (orient === "TB") {
    const px = from.x + NODE_W / 2;
    const pb = from.y + NODE_H;
    const cx = to.x + NODE_W / 2;
    const ct = to.y;
    const mid = pb + V_GAP / 2;
    if (Math.abs(cx - px) < 1) return `M${px},${pb} L${cx},${ct}`;
    const r = Math.min(R, Math.abs(cx - px) / 2, mid - pb, ct - mid);
    const dir = cx > px ? 1 : -1;
    return `M${px},${pb} L${px},${mid - r} Q${px},${mid} ${px + dir * r},${mid} L${cx - dir * r},${mid} Q${cx},${mid} ${cx},${mid + r} L${cx},${ct}`;
  }
  const pr = from.x + NODE_W;
  const pcy = from.y + NODE_H / 2;
  const cl = to.x;
  const ccy = to.y + NODE_H / 2;
  const mid = pr + V_GAP / 2;
  if (Math.abs(ccy - pcy) < 1) return `M${pr},${pcy} L${cl},${ccy}`;
  const r = Math.min(R, Math.abs(ccy - pcy) / 2, mid - pr, cl - mid);
  const dir = ccy > pcy ? 1 : -1;
  return `M${pr},${pcy} L${mid - r},${pcy} Q${mid},${pcy} ${mid},${pcy + dir * r} L${mid},${ccy - dir * r} Q${mid},${ccy} ${mid + r},${ccy} L${cl},${ccy}`;
}

function OrgChart() {
  const dark = useIsDark();
  const theme = useMemo(() => orgTheme(dark), [dark]);

  const [orient, setOrient] = useState<Orient>("TB");
  const [collapsed, setCollapsed] = useState<Set<string>>(new Set());
  const [query, setQuery] = useState("");
  const [hovered, setHovered] = useState<string | null>(null);
  const [zoom, setZoom] = useState(1);
  const [exporting, setExporting] = useState(false);

  const paneRef = useRef<HTMLDivElement>(null);
  const exportSvgRef = useRef<SVGSVGElement>(null);
  const didAutoRef = useRef(false);

  const { data, isLoading } = useQuery({
    queryKey: ["organizationTree"],
    queryFn: getOrganizationTree,
  });

  const roots = useMemo(() => data ?? [], [data]);
  const term = query.trim().toLowerCase();

  // Tree stats + a sensible default collapse (deepest level that still holds a readable row count).
  const { units, maxDepth, presentTypes, autoCollapse } = useMemo(() => {
    const types = new Set<string>();
    const levelCounts: number[] = [];
    let count = 0;
    let depth = 0;
    const walk = (n: OrgUnitTreeNode, d: number) => {
      count += 1;
      depth = Math.max(depth, d);
      levelCounts[d] = (levelCounts[d] ?? 0) + 1;
      types.add(n.unitType);
      n.children?.forEach((c) => walk(c, d + 1));
    };
    roots.forEach((r) => walk(r, 1));
    let readable = 1;
    for (let d = 1; d <= depth; d++) {
      if ((levelCounts[d] ?? 0) <= MAX_NODES_PER_LEVEL) readable = d;
      else break;
    }
    // Collapse every node at or beyond the readable depth so the initial view stays tidy.
    const set = new Set<string>();
    const mark = (n: OrgUnitTreeNode, d: number) => {
      if (d >= readable && n.children?.length) set.add(n.id);
      n.children?.forEach((c) => mark(c, d + 1));
    };
    if (count > AUTO_COLLAPSE_AT) roots.forEach((r) => mark(r, 1));
    return {
      units: count,
      maxDepth: depth,
      presentTypes: (["BusinessUnit", "Directorate", "Division", "Department", "Team", "Branch"] as string[]).filter((t) => types.has(t)),
      autoCollapse: set,
    };
  }, [roots]);

  // Apply the auto-collapse once when data first arrives.
  useEffect(() => {
    if (!didAutoRef.current && roots.length) {
      didAutoRef.current = true;
      setCollapsed(autoCollapse);
    }
  }, [roots, autoCollapse]);

  const matchIds = useMemo(() => {
    const set = new Set<string>();
    if (!term) return set;
    const walk = (n: OrgUnitTreeNode) => {
      if (n.name.toLowerCase().includes(term) || (n.code ?? "").toLowerCase().includes(term)) set.add(n.id);
      n.children?.forEach(walk);
    };
    roots.forEach(walk);
    return set;
  }, [roots, term]);

  // Searching forces a full expansion so matches deep in the tree are actually visible.
  const layout = useMemo(
    () => computeLayout(roots, term ? new Set<string>() : collapsed, orient),
    [roots, collapsed, orient, term],
  );

  const toggle = useCallback((id: string) => {
    setCollapsed((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }, []);

  const setLevel = useCallback(
    (level: number | -1) => {
      const set = new Set<string>();
      if (level !== -1) {
        const mark = (n: OrgUnitTreeNode, d: number) => {
          if (d >= level && n.children?.length) set.add(n.id);
          n.children?.forEach((c) => mark(c, d + 1));
        };
        roots.forEach((r) => mark(r, 1));
      }
      setCollapsed(set);
    },
    [roots],
  );

  const fit = useCallback(() => {
    const pane = paneRef.current;
    if (!pane || !layout.width) return;
    const z = Math.min(
      ZOOM_MAX,
      Math.max(ZOOM_MIN, Math.min((pane.clientWidth - 24) / layout.width, (pane.clientHeight - 24) / layout.height)),
    );
    setZoom(z);
    pane.scrollTo({ left: 0, top: 0 });
  }, [layout.width, layout.height]);

  // Fit to view once after the first layout is known.
  const didFitRef = useRef(false);
  useEffect(() => {
    if (!didFitRef.current && layout.width > 1 && paneRef.current) {
      didFitRef.current = true;
      fit();
    }
  }, [layout.width, fit]);

  // Ctrl/⌘ + wheel to zoom (native, non-passive so preventDefault works); plain wheel = scroll.
  useEffect(() => {
    const pane = paneRef.current;
    if (!pane) return;
    const onWheel = (e: WheelEvent) => {
      if (!(e.ctrlKey || e.metaKey)) return;
      e.preventDefault();
      setZoom((z) => Math.min(ZOOM_MAX, Math.max(ZOOM_MIN, z * (e.deltaY < 0 ? 1.1 : 0.9))));
    };
    pane.addEventListener("wheel", onWheel, { passive: false });
    return () => pane.removeEventListener("wheel", onWheel);
  }, []);

  // Drag-to-pan.
  const drag = useRef<{ x: number; y: number; l: number; t: number } | null>(null);
  const onPointerDown = (e: React.PointerEvent) => {
    if (e.button !== 0) return;
    const t = e.target as HTMLElement;
    if (t.closest("[data-node]")) return; // let node clicks toggle
    const pane = paneRef.current;
    if (!pane) return;
    drag.current = { x: e.clientX, y: e.clientY, l: pane.scrollLeft, t: pane.scrollTop };
    pane.setPointerCapture(e.pointerId);
  };
  const onPointerMove = (e: React.PointerEvent) => {
    const pane = paneRef.current;
    if (!drag.current || !pane) return;
    pane.scrollLeft = drag.current.l - (e.clientX - drag.current.x);
    pane.scrollTop = drag.current.t - (e.clientY - drag.current.y);
  };
  const endDrag = (e: React.PointerEvent) => {
    if (drag.current) paneRef.current?.releasePointerCapture(e.pointerId);
    drag.current = null;
  };

  const hasData = roots.length > 0;

  // ---- Export: serialize a full-expanded, light-theme offscreen SVG → rasterize → PDF ----
  const exportLayout = useMemo(
    () => (exporting ? computeLayout(roots, new Set<string>(), orient) : null),
    [exporting, roots, orient],
  );

  const handleExport = async () => {
    if (!hasData) return;
    setExporting(true);
    try {
      // Dimensions computed fresh (the closure's `exportLayout` is still null on this render).
      const lay = computeLayout(roots, new Set<string>(), orient);
      // Wait for the offscreen SVG (rendered from the memo) to mount + paint, then serialize it.
      let svgEl: SVGSVGElement | null = null;
      for (let i = 0; i < 10 && !svgEl; i++) {
        await new Promise((r) => requestAnimationFrame(() => r(null)));
        svgEl = exportSvgRef.current;
      }
      if (!svgEl) throw new Error("Org-chart export surface did not render.");
      const svgString = new XMLSerializer().serializeToString(svgEl);
      const { exportOrgChartPdf } = await import("./exportOrgChartPdf");
      await exportOrgChartPdf(svgString, lay.width, lay.height, presentTypes, "Organization Chart");
    } finally {
      setExporting(false);
    }
  };

  const shadow = (
    <defs>
      <filter id="orgShadow" x="-20%" y="-20%" width="140%" height="140%">
        <feGaussianBlur in="SourceGraphic" stdDeviation="3.5" />
      </filter>
    </defs>
  );

  const renderScene = (lay: typeof layout, t: ReturnType<typeof orgTheme>, interactive: boolean) => (
    <>
      {shadow}
      <g>
        {lay.edges.map((e) => (
          <path key={`${e.from.id}->${e.to.id}`} d={edgePath(e.from, e.to, orient)} fill="none" stroke={t.edge} strokeWidth={1.4} />
        ))}
      </g>
      <g>
        {lay.nodes.map((n) => (
          <g key={n.id} data-node>
            <OrgNodeCard
              node={n}
              theme={t}
              match={term ? matchIds.has(n.id) : undefined}
              hovered={interactive && hovered === n.id}
              onToggle={toggle}
              onHover={setHovered}
              interactive={interactive}
            />
          </g>
        ))}
      </g>
    </>
  );

  const depthChips: { label: string; value: number | -1 }[] = [
    { label: "1", value: 1 },
    { label: "2", value: 2 },
    { label: "3", value: 3 },
    { label: "All", value: -1 },
  ];
  // Which chip looks active: derive the shallowest currently-collapsed depth is complex; instead
  // reflect "All" when nothing is collapsed, else leave chips as quick actions.
  const allExpanded = collapsed.size === 0;

  return (
    <div className="m-2 flex h-full min-h-0 flex-col rounded-lg border border-border bg-card">
      {/* Toolbar row 1: legend + stats */}
      <div className="flex flex-wrap items-center justify-between gap-x-4 gap-y-1.5 border-b border-border px-4 py-2">
        <div className="flex flex-wrap items-center gap-x-4 gap-y-1.5">
          {presentTypes.map((t) => (
            <span key={t} className="flex items-center gap-1.5 text-xs text-muted">
              <span className="inline-block h-2.5 w-2.5 rounded-full" style={{ backgroundColor: hueFor(t, dark) }} />
              {t}
            </span>
          ))}
        </div>
        {hasData && (
          <span className="text-[11px] text-muted">
            {units} units · {maxDepth} levels
          </span>
        )}
      </div>

      {/* Toolbar row 2: search + view controls */}
      <div className="flex flex-wrap items-center gap-2 border-b border-border px-4 py-2">
        <div className="relative">
          <Search className="pointer-events-none absolute left-2 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-muted" />
          <input
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Find unit by name or code…"
            className="h-7 w-52 rounded border border-border bg-transparent pl-7 pr-7 text-xs text-foreground outline-none placeholder:text-muted focus:border-primary/60"
            aria-label="Find unit"
          />
          {query && (
            <button
              type="button"
              onClick={() => setQuery("")}
              className="absolute right-1.5 top-1/2 -translate-y-1/2 rounded p-0.5 text-muted hover:text-foreground"
              aria-label="Clear search"
            >
              <X className="h-3 w-3" />
            </button>
          )}
        </div>
        {term && (
          <span className={`text-[11px] ${matchIds.size ? "text-muted" : "text-error"}`}>
            {matchIds.size ? `${matchIds.size} match${matchIds.size === 1 ? "" : "es"}` : "No matches"}
          </span>
        )}

        <div className="mx-1 h-4 w-px bg-border" />

        <span className="text-[11px] text-muted">Levels</span>
        <div className="flex overflow-hidden rounded border border-border" role="group" aria-label="Expand to level">
          {depthChips.map((c) => {
            const active = !term && ((c.value === -1 && allExpanded) || false);
            return (
              <button
                key={c.label}
                type="button"
                onClick={() => setLevel(c.value)}
                disabled={!!term}
                className={`px-2 py-0.5 text-[11px] transition-colors disabled:opacity-40 ${
                  active ? "bg-primary text-on-accent" : "text-foreground hover:bg-secondary"
                }`}
              >
                {c.label}
              </button>
            );
          })}
        </div>

        <button
          type="button"
          onClick={() => setOrient((o) => (o === "TB" ? "LR" : "TB"))}
          className="flex items-center gap-1 rounded border border-border px-2.5 py-1 text-xs text-foreground hover:bg-secondary"
          title={orient === "TB" ? "Switch to horizontal layout" : "Switch to vertical layout"}
        >
          {orient === "TB" ? <ArrowLeftRight className="h-3.5 w-3.5" /> : <ArrowUpDown className="h-3.5 w-3.5" />}
          {orient === "TB" ? "Horizontal" : "Vertical"}
        </button>

        <div className="ml-auto flex items-center gap-2">
          {/* Zoom cluster */}
          <div className="flex items-center overflow-hidden rounded border border-border">
            <button type="button" onClick={() => setZoom((z) => Math.max(ZOOM_MIN, z * 0.9))} className="px-1.5 py-1 text-foreground hover:bg-secondary" title="Zoom out" aria-label="Zoom out">
              <Minus className="h-3.5 w-3.5" />
            </button>
            <span className="w-10 border-x border-border text-center text-[11px] text-muted">{Math.round(zoom * 100)}%</span>
            <button type="button" onClick={() => setZoom((z) => Math.min(ZOOM_MAX, z * 1.1))} className="px-1.5 py-1 text-foreground hover:bg-secondary" title="Zoom in" aria-label="Zoom in">
              <Plus className="h-3.5 w-3.5" />
            </button>
          </div>
          <button type="button" onClick={fit} className="flex items-center gap-1 rounded border border-border px-2.5 py-1 text-xs text-foreground hover:bg-secondary" title="Fit to view">
            <Scan className="h-3.5 w-3.5" /> Fit
          </button>
          <button
            type="button"
            onClick={() => {
              setZoom(1);
              paneRef.current?.scrollTo({ left: 0, top: 0 });
            }}
            className="flex items-center gap-1 rounded border border-border px-2.5 py-1 text-xs text-foreground hover:bg-secondary"
            title="Reset zoom"
          >
            <Maximize2 className="h-3.5 w-3.5" /> 100%
          </button>
          <button
            type="button"
            onClick={handleExport}
            disabled={!hasData || exporting}
            className="flex items-center gap-1 rounded bg-primary px-3 py-1 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-50"
          >
            <Download className="h-3.5 w-3.5" /> {exporting ? "Exporting…" : "Export PDF"}
          </button>
        </div>
      </div>

      {/* Chart pane */}
      <div
        ref={paneRef}
        className="relative min-h-0 flex-1 overflow-auto"
        style={{ backgroundColor: theme.canvas, cursor: drag.current ? "grabbing" : "grab", touchAction: "none" }}
        onPointerDown={onPointerDown}
        onPointerMove={onPointerMove}
        onPointerUp={endDrag}
        onPointerLeave={endDrag}
      >
        {isLoading && <Loading />}
        {!isLoading && !hasData && (
          <div className="flex h-full items-center justify-center p-8 text-center text-sm text-muted">
            No organization units yet. Add a root unit to see the chart.
          </div>
        )}
        {hasData && (
          <svg
            width={layout.width * zoom}
            height={layout.height * zoom}
            viewBox={`0 0 ${layout.width} ${layout.height}`}
            style={{ display: "block", minWidth: "100%", minHeight: "100%" }}
          >
            {renderScene(layout, theme, true)}
          </svg>
        )}
      </div>
      {hasData && (
        <p className="border-t border-border px-4 py-1.5 text-[11px] text-muted">
          Drag to pan · ⌘/Ctrl + wheel to zoom · click a node to collapse/expand · Fit resets the view · PDF exports
          the full structure
        </p>
      )}

      {/* Offscreen full-expanded SVG used only for the PDF snapshot (light theme, scale 1). */}
      {exporting && exportLayout && (
        <div style={{ position: "fixed", left: -100000, top: 0, pointerEvents: "none" }} aria-hidden>
          <svg
            ref={exportSvgRef}
            xmlns="http://www.w3.org/2000/svg"
            width={exportLayout.width}
            height={exportLayout.height}
            viewBox={`0 0 ${exportLayout.width} ${exportLayout.height}`}
          >
            <rect x={0} y={0} width={exportLayout.width} height={exportLayout.height} fill={orgTheme(false).canvas} />
            {renderScene(exportLayout, orgTheme(false), false)}
          </svg>
        </div>
      )}
    </div>
  );
}

export default OrgChart;
