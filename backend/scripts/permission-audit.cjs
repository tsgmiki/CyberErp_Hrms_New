const fs = require("fs"), path = require("path");
const sp = process.argv[2];
const classes = JSON.parse(fs.readFileSync(sp + "/classes.json", "utf8"));

const grants = {};
for (const l of fs.readFileSync(sp + "/userrole.txt", "utf8").split(/\n/)) {
  const [k, b] = l.trim().split("|");
  if (k && b) grants[k] = { View:b[0]==="1", Add:b[1]==="1", Edit:b[2]==="1",
                            Delete:b[3]==="1", Approve:b[4]==="1", Export:b[5]==="1" };
}
const APPROVE = new Set(["approve","approved","reject","rejected","decide","decision","endorse","authorize","authorise","sign","signoff","acknowledge","calibrate","verify"]);
const ADDT    = new Set(["generate","build","create","clone","duplicate","seed","enroll","enrol"]);
const EXPORTT = new Set(["export","download","print","pdf","excel","csv"]);
function derive(verb, suffix) {
  const toks = (suffix || "").split("-").filter(Boolean);
  if (toks.some(t => EXPORTT.has(t))) return "Export";
  if (verb === "Get") return "View";
  if (verb === "Delete") return "Delete";
  if (verb === "Put" || verb === "Patch") return "Edit";
  if (verb === "Post") {
    if (!suffix) return "Add";
    if (toks.some(t => APPROVE.has(t))) return "Approve";
    if (toks.some(t => ADDT.has(t))) return "Add";
    return "Edit";
  }
  return "View";
}
const STRONG = /CanAccessEmployeeAsync|scope\.IsAdmin|GetScopeAsync|EnsureAdminAsync|EvaluateClearanceApproverAsync|LoadGatedAsync|ResolveApproverUserIdsAsync|approverAuth|HasAnyAsync|CurrentEmployeeId|scope\.EmployeeId|visibility\.|EnsureCanActAsync|CanActOnStageAsync|GetCurrentUserId|EnsureCanActOnUnitAsync|UnitScopeGuard|CanAdministerAsync|CanManageEmployeeAsync/;
const WEAK = /EnsureEmployeeVisibleAsync|EnsurePersonVisibleAsync/;

const di = fs.readFileSync("CyberErp.Hrms.App/DependencyInjection.cs", "utf8").replace(/\r\n/g, "\n");
const diMap = {};
for (const ln of di.split("\n")) {
  const mm = ln.match(/AddScoped<\s*([\w.]+)\s*,\s*([\w.]+)\s*>/);
  if (mm) diMap[mm[1].split(".").pop()] = mm[2].split(".").pop();
}

const dir = "CyberErp.Hrms.Api/Controllers";
const files = [];
(function walk(d){ for (const e of fs.readdirSync(d, {withFileTypes:true})) {
  const p = path.join(d, e.name);
  if (e.isDirectory()) walk(p); else if (e.name.endsWith(".cs")) files.push(p);
}})(dir);

const REQ = /\[RequirePermission\(([^\]]*)\)\]/;
const out = [];
for (const f of files) {
  const lines = fs.readFileSync(f, "utf8").replace(/\r\n/g, "\n").split("\n");
  let clsName = null, clsLinks = null, params = {};
  for (let i = 0; i < lines.length; i++) {
    const ln = lines[i];

    const cm = ln.match(/public class ([A-Za-z0-9_]+)/);
    if (cm) {
      clsName = cm[1]; params = {};
      // class-level attribute: nearest [RequirePermission] directly above the declaration
      clsLinks = null;
      for (let j = i - 1; j >= 0 && j >= i - 6; j--) {
        const rm = lines[j].match(REQ);
        if (rm) { clsLinks = rm[1]; break; }
        if (lines[j].trim() && !lines[j].trim().startsWith("[") && !lines[j].trim().startsWith("///")) break;
      }
      // primary-constructor params follow the declaration
      for (let j = i; j < Math.min(i + 25, lines.length); j++) {
        const pm = lines[j].match(/^\s*(I[A-Za-z0-9_]+)\s+([A-Za-z0-9_]+)\s*[,)]/);
        if (pm) params[pm[2]] = pm[1];
        if (/\)\s*:\s*\w+Controller|\)\s*:\s*BaseController/.test(lines[j])) break;
      }
      continue;
    }

    const hm = ln.match(/\[Http(Get|Post|Put|Patch|Delete)(?:\("([^"]*)"\))?\]/);
    if (!hm || !clsName) continue;
    const verb = hm[1], route = hm[2] || "";

    // the action's own attributes: contiguous [..] lines after the Http attribute, plus the method body
    let k = i + 1, attrs = [], selfScoped = false, actLinks = null, ovr = null;
    while (k < lines.length && /^\s*\[/.test(lines[k])) { attrs.push(lines[k]); k++; }
    for (const a of attrs) {
      if (/\[SelfScoped\]/.test(a)) selfScoped = true;
      const rm = a.match(REQ);
      if (rm) actLinks = rm[1];
      const om = a.match(/Access\s*=\s*PermissionAccess\.([A-Za-z]+)/);
      if (om) ovr = om[1];
    }
    if (selfScoped) continue;

    const raw = actLinks !== null ? actLinks : clsLinks;
    if (raw === null) continue;                       // not permission-gated at all
    const links = [...raw.matchAll(/"([^"]+)"/g)].map(x => x[1].toLowerCase().replace(/^\/?(hrms\/)?/, ""));
    if (links.length === 0) continue;

    const lit = route.split("/").filter(s => s && !s.includes("{")).pop() || null;
    const acc = ovr || derive(verb, lit);
    if (acc === "View" || acc === "Export") continue;
    const reach = links.filter(l => grants[l] && grants[l][acc]);
    if (reach.length === 0) continue;

    const body = lines.slice(k, Math.min(k + 12, lines.length)).join("\n");
    const call = (body.match(/([A-Za-z0-9_]+)\.[A-Za-z0-9_]+\(/) || [])[1];
    const iface = call ? params[call] : null;
    const cls = iface ? diMap[iface] : null;
    const hbody = cls && classes[cls] ? classes[cls].body : null;

    // A guard can live in the CONTROLLER ACTION rather than the handler (EmployeeMovement's
    // execute endpoints check IsAdmin inline), so the action body counts too — otherwise those
    // endpoints report as unresolved for ever and a later pass "rediscovers" them.
    const actionGuarded = STRONG.test(body);

    let verdict;
    if (actionGuarded) verdict = "guarded";
    else if (!hbody) verdict = "UNRESOLVED";
    else if (STRONG.test(hbody)) verdict = "guarded";
    else if (WEAK.test(hbody)) verdict = "WEAK";
    else verdict = "NO GUARD";

    out.push({ file: path.basename(f), ctrl: clsName, verb, route, acc,
               via: reach.join(","), cls: cls || "?", verdict });
  }
}
fs.writeFileSync(sp + "/audit.json", JSON.stringify(out, null, 1));
const by = v => out.filter(o => o.verdict === v).length;
console.log("write endpoints UserRole can invoke: " + out.length);
console.log("  guarded          : " + by("guarded"));
console.log("  NO GUARD         : " + by("NO GUARD"));
console.log("  WEAK (existence) : " + by("WEAK"));
console.log("  UNRESOLVED       : " + by("UNRESOLVED"));
