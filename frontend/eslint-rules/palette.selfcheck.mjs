/**
 * Does the palette rule still work?
 *
 * ⚠️ This exists because the rule's failure mode is a CLEAN RUN. Every bug it has had so far —
 * a selector walker that stopped at `::placeholder`, a rule-head regex that swallowed the comment
 * above a selector — made it report FEWER problems, not more. "0 errors" therefore proves nothing
 * on its own, and a false negative here means an unregistered utility ships and paints nothing.
 *
 * So this asserts both directions against the real theme.css: utilities the theme registers must
 * pass, and invented ones must fail. Run it with `npm run lint:palette`.
 */
import { pathToFileURL } from "node:url";
import path from "node:path";
import process from "node:process";

const root = path.resolve(import.meta.dirname, "..");
const { default: plugin } = await import(
  pathToFileURL(path.join(root, "eslint-rules", "palette.js")).href
);
const rule = plugin.rules["no-unregistered-utility"];

const reports = [];
const visitors = rule.create({ cwd: root, report: (r) => reports.push(r.data.token) });
if (!visitors.Literal) {
  console.error("FAIL: the rule disabled itself — no theme.css found at", root);
  process.exit(1);
}

let id = 0;
const flags = (token) => {
  reports.length = 0;
  visitors.Literal({ range: [id++, id], value: token });
  return reports.length > 0;
};

/** Registered in theme.css — every one of these was a false positive at some point. */
const mustPass = [
  "bg-primary", "text-primary", "accent-primary", "text-muted-foreground",
  "bg-primary/15", "bg-warning/10", "border-primary/25", "before:bg-primary",
  "focus:border-primary", "hover:bg-secondary", "hover:text-primary",
  "bg-secondary/40", "hover:bg-error/10", "group-hover:text-primary",
  "placeholder:text-muted-foreground", "divide-border",
  // Real Tailwind colours, which need no registration and must never be flagged.
  "bg-slate-100", "text-white", "bg-amber-50", "border-red-500",
];

/** Not registered — these emit nothing, which is the whole point of the rule. */
const mustFail = [
  "bg-primary/999", "hover:bg-primary/3", "text-error/7",
  "focus:bg-warning/55", "group-hover:border-info/99",
];

const failures = [];
for (const t of mustPass) if (flags(t)) failures.push(`  false positive: ${t} IS registered`);
for (const t of mustFail) if (!flags(t)) failures.push(`  false negative: ${t} is NOT registered`);

if (failures.length > 0) {
  console.error(`palette self-check FAILED (${failures.length}):\n${failures.join("\n")}`);
  process.exit(1);
}
console.log(`palette self-check ok — ${mustPass.length} registered pass, ${mustFail.length} unregistered flagged`);
