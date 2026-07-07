"use client";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import ReactECharts from "echarts-for-react";
import { useQuery } from "@tanstack/react-query";
import { Download, Maximize2 } from "lucide-react";
import type { OrgUnitTreeNode } from "@/models";
import getOrganizationTree from "@/services/admin/organizationUnit/getTree";
import Loading from "../loader/loader";
import { exportOrgChartPdf } from "./exportOrgChartPdf";

/** Validated categorical palette (dataviz reference slots 1–5), one hue per unit type. */
const UNIT_ORDER = ["BusinessUnit", "Directorate", "Division", "Department", "Team","Branch"] as const;
const HUE: Record<string, { light: string; dark: string }> = {
  BusinessUnit: { light: "#2a78d6", dark: "#3987e5" },
  Directorate: { light: "#1baf7a", dark: "#199e70" },
  Division: { light: "#eda100", dark: "#c98500" },
  Department: { light: "#008300", dark: "#008300" },
  Team: { light: "#4a3aa7", dark: "#9085e9" },
  Branch: { light: "#8a8a8a", dark: "#5a5a5a" },
};
const FALLBACK = { light: "#52514e", dark: "#c3c2b7" };

function hueFor(type: string, dark: boolean) {
  return (HUE[type] ?? FALLBACK)[dark ? "dark" : "light"];
}

function hexToRgba(hex: string, alpha: number) {
  const h = hex.replace("#", "");
  const r = parseInt(h.slice(0, 2), 16);
  const g = parseInt(h.slice(2, 4), 16);
  const b = parseInt(h.slice(4, 6), 16);
  return `rgba(${r}, ${g}, ${b}, ${alpha})`;
}

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

interface EChartNode {
  name: string;
  rawType: string;
  headcount?: number | null;
  itemStyle: Record<string, unknown>;
  children?: EChartNode[];
}

function OrgChart() {
  const chartRef = useRef<any>(null);
  const dark = useIsDark();
  const [exporting, setExporting] = useState(false);

  const { data, isLoading } = useQuery({
    queryKey: ["organizationTree"],
    queryFn: getOrganizationTree,
  });

  const surface = dark ? "#1a1a19" : "#ffffff";
  const ink = dark ? "#f4f4f2" : "#0b0b0b";
  const muted = dark ? "#c3c2b7" : "#52514e";
  const edge = dark ? "#3a3a37" : "#d4d3cc";

  const toNode = useCallback(
    (n: OrgUnitTreeNode): EChartNode => {
      const hue = hueFor(n.unitType, dark);
      return {
        name: n.name,
        rawType: n.unitType,
        headcount: n.allocatedHeadcount,
        itemStyle: {
          color: hexToRgba(hue, dark ? 0.24 : 0.12),
          borderColor: hue,
          borderWidth: 1.6,
        },
        children: n.children?.map(toNode),
      };
    },
    [dark],
  );

  const { treeData, presentTypes } = useMemo(() => {
    const roots = (data ?? []).map(toNode);
    const types = new Set<string>();
    const walk = (n: OrgUnitTreeNode) => {
      types.add(n.unitType);
      n.children?.forEach(walk);
    };
    (data ?? []).forEach(walk);
    const ordered = UNIT_ORDER.filter((t) => types.has(t));

    const rootData: EChartNode[] =
      roots.length === 1
        ? roots
        : [
            {
              name: "Organization",
              rawType: "",
              itemStyle: { color: surface, borderColor: muted, borderWidth: 1.4 },
              children: roots,
            },
          ];
    return { treeData: rootData, presentTypes: ordered };
  }, [data, toNode, surface, muted]);

  const option = useMemo(
    () => ({
      backgroundColor: "transparent",
      tooltip: {
        trigger: "item",
        borderWidth: 0,
        backgroundColor: dark ? "#26262340" : "#ffffff",
        extraCssText: "box-shadow:0 4px 16px rgba(0,0,0,0.18);border-radius:8px;",
        textStyle: { color: ink, fontSize: 12 },
        formatter: (p: any) => {
          const d = p.data as EChartNode;
          if (!d.rawType) return `<b>${d.name}</b>`;
          const hc = d.headcount != null ? `<br/>Headcount: <b>${d.headcount}</b>` : "";
          return `<b>${d.name}</b><br/><span style="color:${hueFor(d.rawType, dark)}">${d.rawType}</span>${hc}`;
        },
      },
      series: [
        {
          type: "tree",
          data: treeData,
          layout: "orthogonal",
          orient: "TB",
          edgeShape: "polyline",
          edgeForkPosition: "63%",
          roam: true,
          initialTreeDepth: -1,
          symbol: "roundRect",
          symbolSize: [168, 50],
          top: 24,
          bottom: 24,
          left: 24,
          right: 24,
          itemStyle: { borderRadius: 8 },
          lineStyle: { color: edge, width: 1.4 },
          label: {
            position: "inside",
            color: ink,
            formatter: (p: any) => {
              const d = p.data as EChartNode;
              return d.rawType ? `{n|${d.name}}\n{t|${d.rawType}}` : `{h|${d.name}}`;
            },
            rich: {
              n: { fontSize: 12.5, fontWeight: 600, color: ink, align: "center", lineHeight: 17 },
              t: { fontSize: 9.5, color: muted, align: "center", padding: [2, 0, 0, 0] },
              h: { fontSize: 13, fontWeight: 700, color: ink, align: "center" },
            },
          },
          leaves: { label: { position: "inside" } },
          emphasis: { focus: "relative", itemStyle: { borderWidth: 2.4 } },
          expandAndCollapse: true,
          animationDuration: 450,
          animationDurationUpdate: 500,
        },
      ],
    }),
    [treeData, dark, ink, muted, edge],
  );

  const resetView = () => chartRef.current?.getEchartsInstance()?.dispatchAction({ type: "restore" });

  const handleExport = async () => {
    const inst = chartRef.current?.getEchartsInstance();
    if (!inst) return;
    setExporting(true);
    try {
      const url = inst.getDataURL({ type: "png", pixelRatio: 2, backgroundColor: surface });
      await exportOrgChartPdf(url, { width: inst.getWidth(), height: inst.getHeight(), title: "Organization Chart" });
    } finally {
      setExporting(false);
    }
  };

  const hasData = !!data && data.length > 0;

  return (
    <div className="m-2 flex h-full min-h-0 flex-col rounded-lg border border-border bg-card">
      {/* Toolbar */}
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-border px-4 py-2.5">
        <div className="flex flex-wrap items-center gap-x-4 gap-y-1.5">
          {presentTypes.map((t) => (
            <span key={t} className="flex items-center gap-1.5 text-xs text-muted">
              <span
                className="inline-block h-2.5 w-2.5 rounded-sm"
                style={{ backgroundColor: hexToRgba(hueFor(t, dark), dark ? 0.35 : 0.18), border: `1.5px solid ${hueFor(t, dark)}` }}
              />
              {t}
            </span>
          ))}
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={resetView}
            className="flex items-center gap-1 rounded border border-border px-2.5 py-1 text-xs text-foreground hover:bg-secondary"
            title="Reset zoom / pan"
          >
            <Maximize2 className="h-3.5 w-3.5" /> Reset
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

      {/* Chart */}
      <div className="relative min-h-0 flex-1">
        {isLoading && <Loading />}
        {!isLoading && !hasData && (
          <div className="flex h-full items-center justify-center p-8 text-center text-sm text-muted">
            No organization units yet. Add a root unit to see the chart.
          </div>
        )}
        {hasData && (
          <ReactECharts
            ref={chartRef}
            option={option}
            notMerge
            style={{ height: "100%", width: "100%", minHeight: "60vh" }}
          />
        )}
      </div>
      {hasData && (
        <p className="border-t border-border px-4 py-1.5 text-[11px] text-muted">
          Scroll to zoom · drag to pan · click a node to collapse/expand
        </p>
      )}
    </div>
  );
}

export default OrgChart;
