import fs from "node:fs";
import path from "node:path";

/**
 * Palette utilities in this app are HAND-WRITTEN CSS, not Tailwind colours.
 *
 * Tailwind does not know `primary`, `warning`, `secondary` and the rest, so every utility that uses
 * one — including every `/nn` opacity variant and every `hover:` form — exists only because
 * `src/config/theme.css` spells it out. Anything not spelled out there emits NOTHING: no build
 * error, no console warning, just an element with no background, no border, or no hover.
 *
 * That failure is invisible in review and invisible in the browser unless you happen to know what
 * the colour should have looked like. This rule makes it a lint error instead.
 *
 * ⚠️ The set of "palette colours" is DERIVED FROM theme.css, never hardcoded here. A colour counts
 * as palette exactly when the theme file hand-writes at least one utility for it, which keeps the
 * rule correct as the palette grows and stops it flagging real Tailwind colours (`slate`, `amber`,
 * `white`) that need no registration.
 */

/** Variant prefixes we understand; a token may carry several (`group-hover:focus:`). */
const VARIANT = "(?:[a-z][a-z0-9-]*:)*";
/** Utility families that take a colour. */
const FAMILY = "bg|text|border|ring|divide|accent|fill|stroke|outline|shadow|caret|decoration";

const TOKEN = new RegExp(`^(${VARIANT})(${FAMILY})-([a-z][a-z0-9-]*?)(?:\\/(\\d{1,3}))?$`);

/**
 * Read the class name off the FRONT of a selector, stopping at whatever follows it.
 *
 * ⚠️ A naive "strip a known pseudo-class tail" version misses most of this file. The class name
 * itself contains escaped colons and slashes, and what follows it can be a pseudo-class
 * (:hover), a pseudo-element (::placeholder), a functional selector
 * (:is(:where(.group):hover *)) or a combinator (.divide-border > :not([hidden])). Walking the
 * characters and treating a backslash as "take the next character literally" handles all of
 * them — and getting it wrong makes the rule report REGISTERED utilities as missing, which is
 * how 28 false positives appeared the first time.
 */
function classNameFromSelector(selector) {
  if (selector[0] !== ".") return null;
  let name = "";
  for (let i = 1; i < selector.length; i++) {
    const c = selector[i];
    if (c === "\\") { name += selector[++i] ?? ""; continue; }
    if (/[A-Za-z0-9_-]/.test(c)) { name += c; continue; }
    break;   // a pseudo, a combinator, whitespace — the class name has ended
  }
  return name || null;
}

function readTheme(themePath) {
  // ⚠️ Comments FIRST. A rule head is read as "everything since the last brace", so a comment
  // sitting directly above a rule lands in front of its selector and the whole rule is skipped —
  // silently, and only for the commented ones. That is how `.text-primary` and `.accent-primary`
  // read as unregistered while `.text-secondary`, three lines below, read as fine.
  const css = fs.readFileSync(themePath, "utf8").replace(/\/\*[\s\S]*?\*\//g, " ");
  const registered = new Set();

  // Every rule head, split on commas — each part may carry pseudos and combinators after the class.
  for (const match of css.matchAll(/([^{}]+)\{/g)) {
    for (const part of match[1].split(",")) {
      const name = classNameFromSelector(part.trim());
      if (name) registered.add(name);
    }
  }

  // A colour is "palette" when the theme hand-writes at least one utility for it.
  const palette = new Set();
  for (const cls of registered) {
    const m = TOKEN.exec(cls);
    if (m) palette.add(m[3]);
  }
  // ⚠️ A parse failure here would not look like a failure. The rule would find no palette, decide
  // nothing is checkable, and report a clean zero — the exact silent-nothing behaviour it exists to
  // catch, now applied to itself. So: if the theme file is present and readable, it MUST yield a
  // palette. Blowing up is the only outcome anybody notices.
  if (palette.size === 0)
    throw new Error(
      `palette/no-unregistered-utility: parsed ${registered.size} class names from ${themePath} ` +
        `but recognised no palette colours. The rule would silently pass everything — fix the ` +
        `parser rather than letting it run.`,
    );

  return { registered, palette };
}

let cache = null;
function theme(cwd) {
  if (cache) return cache;
  const themePath = path.join(cwd, "src", "config", "theme.css");
  if (!fs.existsSync(themePath)) return (cache = { registered: new Set(), palette: new Set() });
  return (cache = readTheme(themePath));
}

/** Suggest the closest registered alternative, so the message is actionable rather than a refusal. */
function nearest(token, registered) {
  const m = TOKEN.exec(token);
  if (!m) return [];
  const [, variant, family, colour] = m;
  const prefix = `${variant}${family}-${colour}`;
  return [...registered]
    .filter((r) => r === prefix || r.startsWith(`${prefix}/`))
    .sort();
}

const noUnregisteredUtility = {
  meta: {
    type: "problem",
    docs: {
      description:
        "Palette colour utilities must be hand-written in theme.css; unregistered ones silently emit nothing.",
    },
    schema: [],
    messages: {
      unregistered:
        "`{{token}}` is not registered in theme.css and will emit NOTHING — no colour at all, with no build error. {{hint}}",
    },
  },

  create(context) {
    const { registered, palette } = theme(context.cwd ?? process.cwd());
    // Only reachable when there is no theme.css at all — a bad parse throws in readTheme instead.
    if (palette.size === 0) return {};

    const seen = new Set();

    function check(node, text) {
      for (const token of text.split(/\s+/)) {
        if (!token || seen.has(`${node.range[0]}:${token}`)) continue;
        const m = TOKEN.exec(token);
        if (!m) continue;

        const colour = m[3];
        // Not one of ours — a real Tailwind colour needs no registration.
        if (!palette.has(colour)) continue;
        if (registered.has(token)) continue;

        seen.add(`${node.range[0]}:${token}`);
        const options = nearest(token, registered);
        const hint = options.length > 0
          ? `Registered for this colour: ${options.join(", ")}.`
          : "Use a registered variant, or add the rule to theme.css and verify it in the BUILT css.";
        context.report({ node, messageId: "unregistered", data: { token, hint } });
      }
    }

    return {
      Literal(node) {
        if (typeof node.value === "string") check(node, node.value);
      },
      // Template literals carry the conditional className case: `... ${on ? "bg-x" : "bg-y"}`.
      TemplateElement(node) {
        if (node.value?.cooked) check(node, node.value.cooked);
      },
    };
  },
};

export default {
  rules: { "no-unregistered-utility": noUnregisteredUtility },
};
