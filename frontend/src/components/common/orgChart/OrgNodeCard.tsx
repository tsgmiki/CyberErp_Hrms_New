import { NODE_H, NODE_W, initials, wrapLines, type LaidNode } from "./layout";
import { hexToRgba, hueFor, type OrgTheme } from "./palette";

/**
 * One org-unit rendered as an SVG card group — the single source of truth for a node's look,
 * used BOTH on-screen (interactive) and for the PDF (serialized offscreen, then rasterized).
 * Everything is a plain SVG element with inline presentation attributes (no CSS classes), so the
 * card survives XML serialization → PNG with byte-for-byte the same appearance as on screen.
 *
 * The design (SAP/Fiori object-card cues): a rounded surface with a soft shadow, a colored top
 * accent bar, a hue avatar with the unit's initials, the name word-wrapped to two lines, a tinted
 * type chip, and a headcount. Nodes with children carry a count pill that toggles collapse.
 */
export interface OrgNodeCardProps {
  node: LaidNode;
  theme: OrgTheme;
  /** Search state: undefined = no search; true = a match (emphasized); false = dimmed. */
  match?: boolean;
  hovered?: boolean;
  onToggle?: (id: string) => void;
  onHover?: (id: string | null) => void;
  /** static = export mode (no interactivity / pointer cursor). */
  interactive?: boolean;
}

const NAME_MAX_CHARS = 22;
const AV = 40; // avatar size

function OrgNodeCard({ node, theme, match, hovered, onToggle, onHover, interactive = true }: OrgNodeCardProps) {
  const hue = node.unitType ? hueFor(node.unitType, theme.dark) : theme.muted;
  const dimmed = match === false;
  const emphasized = match === true;
  const lines = wrapLines(node.name, NAME_MAX_CHARS, 2);
  const nameTop = 26 + (lines.length === 1 ? 8 : 0);

  const borderColor = emphasized ? hue : hovered ? hexToRgba(hue, 0.65) : theme.cardBorder;
  const borderW = emphasized ? 2 : 1;

  const textX = 16 + AV + 12; // right of the avatar
  const canToggle = interactive && node.hasChildren;

  return (
    <g
      transform={`translate(${node.x}, ${node.y})`}
      opacity={dimmed ? theme.dim : 1}
      style={interactive ? { cursor: canToggle ? "pointer" : "default" } : undefined}
      onMouseEnter={interactive ? () => onHover?.(node.id) : undefined}
      onMouseLeave={interactive ? () => onHover?.(null) : undefined}
      onClick={canToggle ? () => onToggle?.(node.id) : undefined}
    >
      {/* Soft drop shadow (a blurred offset clone via the shared filter) */}
      <rect
        x={0}
        y={3}
        width={NODE_W}
        height={NODE_H}
        rx={12}
        fill={theme.dark ? "#000000" : "#0f172a"}
        opacity={hovered ? 0.16 : 0.08}
        filter="url(#orgShadow)"
      />
      {/* Card surface */}
      <rect x={0} y={0} width={NODE_W} height={NODE_H} rx={12} fill={theme.card} stroke={borderColor} strokeWidth={borderW} />
      {/* Top accent bar (clipped to the card's rounded top) */}
      <path
        d={`M0,12 A12,12 0 0 1 12,0 L${NODE_W - 12},0 A12,12 0 0 1 ${NODE_W},12 L${NODE_W},15 L0,15 Z`}
        fill={hue}
      />

      {/* Avatar */}
      <rect x={16} y={18} width={AV} height={AV} rx={9} fill={node.unitType ? hue : theme.chipBg} />
      <text
        x={16 + AV / 2}
        y={18 + AV / 2}
        textAnchor="middle"
        dominantBaseline="central"
        fontFamily="Arial, Helvetica, sans-serif"
        fontSize={15}
        fontWeight={700}
        fill={node.unitType ? "#ffffff" : theme.muted}
      >
        {initials(node.name)}
      </text>

      {/* Name (1–2 wrapped lines) */}
      <text x={textX} y={nameTop} fontFamily="Arial, Helvetica, sans-serif" fontSize={13.5} fontWeight={600} fill={theme.ink}>
        {lines.map((ln, i) => (
          <tspan key={i} x={textX} dy={i === 0 ? 0 : 16}>
            {ln}
          </tspan>
        ))}
      </text>

      {/* Type chip + headcount row */}
      {node.unitType ? (
        <>
          <rect x={textX} y={NODE_H - 26} width={typeChipW(node.unitType)} height={17} rx={8.5} fill={hexToRgba(hue, theme.dark ? 0.22 : 0.13)} />
          <text
            x={textX + 9}
            y={NODE_H - 26 + 8.5}
            dominantBaseline="central"
            fontFamily="Arial, Helvetica, sans-serif"
            fontSize={9.5}
            fontWeight={600}
            fill={hue}
          >
            {node.unitType}
          </text>
          {node.headcount != null && (
            <text
              x={NODE_W - 14}
              y={NODE_H - 26 + 8.5}
              textAnchor="end"
              dominantBaseline="central"
              fontFamily="Arial, Helvetica, sans-serif"
              fontSize={9.5}
              fill={theme.muted}
            >
              {`HC ${node.headcount}`}
            </text>
          )}
        </>
      ) : null}

      {/* Collapse / expand count pill at the bottom edge */}
      {node.hasChildren && (
        <g>
          <rect
            x={NODE_W / 2 - 15}
            y={NODE_H - 9}
            width={30}
            height={18}
            rx={9}
            fill={node.collapsed ? hue : theme.card}
            stroke={node.collapsed ? hue : theme.cardBorder}
            strokeWidth={1}
          />
          <text
            x={NODE_W / 2}
            y={NODE_H}
            textAnchor="middle"
            dominantBaseline="central"
            fontFamily="Arial, Helvetica, sans-serif"
            fontSize={10}
            fontWeight={700}
            fill={node.collapsed ? "#ffffff" : theme.muted}
          >
            {node.collapsed ? `+${node.childCount}` : "–"}
          </text>
        </g>
      )}
    </g>
  );
}

/** Rough chip width from the type label length (SVG has no auto-size). */
function typeChipW(type: string) {
  return Math.round(type.length * 5.6) + 18;
}

export default OrgNodeCard;
