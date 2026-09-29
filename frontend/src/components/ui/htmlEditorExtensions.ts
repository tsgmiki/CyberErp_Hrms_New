import { Node, Extension } from "@tiptap/core";

/**
 * Extensions that let the document-template editor hold a LAYOUT, not just prose.
 *
 * ⚠️ WHY THIS EXISTS. TipTap is schema-driven: anything its schema cannot represent is discarded
 * on parse, silently and without error. With StarterKit alone, saving a template did this:
 *
 *   <div style="display:flex">{{Logo}}…{{Branch}}</div>  →  <p>{{Logo}}</p><p>{{Branch}}</p>
 *   <table><td>LEFT</td><td>RIGHT</td></table>           →  <p>LEFTRIGHT</p>
 *
 * Block-level `text-align` on a paragraph was the ONLY layout primitive that survived, which is
 * why every attempt at a left/right header collapsed into a single alignment. It also meant that
 * opening any seeded template that contains a table — the Transfer Notice, the settlement letter,
 * the bilingual experience letter — and pressing Save destroyed its markup.
 */

/** Attribute name → CSS property, for the raw style passthrough. */
const STYLE_NODES = [
  "paragraph",
  "heading",
  "bulletList",
  "orderedList",
  "listItem",
  "blockquote",
  "image",
  "table",
  "tableRow",
  "tableCell",
  "tableHeader",
  "styledDiv",
];

/**
 * Keep the `style` and `class` an author wrote, on every node that can carry one.
 *
 * ⚠️ `text-align` is deliberately STRIPPED here and left to the TextAlign extension. Both would
 * otherwise render it: `mergeAttributes` dedupes by CSS property, so whichever ran last would win,
 * and a stale alignment saved in the raw string could silently override the toolbar button the
 * user just pressed.
 */
export const StylePassthrough = Extension.create({
  name: "stylePassthrough",

  addGlobalAttributes() {
    return [
      {
        types: STYLE_NODES,
        attributes: {
          style: {
            default: null,
            parseHTML: (element) => {
              const raw = element.getAttribute("style");
              if (!raw) return null;
              const kept = raw
                .split(";")
                .map((d) => d.trim())
                .filter((d) => d && !/^text-align\s*:/i.test(d))
                .join("; ");
              return kept || null;
            },
            renderHTML: (attributes) =>
              attributes.style ? { style: attributes.style } : {},
          },
          class: {
            default: null,
            parseHTML: (element) => element.getAttribute("class") || null,
            renderHTML: (attributes) =>
              attributes.class ? { class: attributes.class } : {},
          },
        },
      },
    ];
  },
});

/**
 * A generic `<div>` that survives a save, with whatever styling it carries.
 *
 * <p>This is what makes a flex letterhead — or any container an author pastes in from an existing
 * document — round-trip instead of being flattened into stacked paragraphs.</p>
 *
 * <p>⚠️ Low priority on purpose. A `div` rule matching at default priority would win against more
 * specific nodes for elements that are both (a table wrapper, for instance), so this only claims
 * divs nothing else wanted.</p>
 */
export const StyledDiv = Node.create({
  name: "styledDiv",
  group: "block",
  content: "block+",
  defining: true,
  priority: 50,

  parseHTML() {
    return [{ tag: "div" }];
  },

  renderHTML({ HTMLAttributes }) {
    return ["div", HTMLAttributes, 0];
  },
});
