#!/usr/bin/env node
/**
 * Prints copy-paste scaffolding for a new CRUD entity.
 * Usage: node scripts/scaffold-entity.mjs Role admin Role
 */
const [,, entityPascal, domain, apiResource] = process.argv;
if (!entityPascal || !domain) {
  console.error("Usage: node scripts/scaffold-entity.mjs <EntityName> <domain> [ApiResourceName]");
  process.exit(1);
}
const resource = apiResource || entityPascal;
const entityKebab = entityPascal.replace(/([a-z])([A-Z])/g, "$1-$2").toLowerCase();
const listKey = entityKebab.replace(/-/g, "-");

console.log(`
# Scaffold: ${entityPascal}
# Domain: ${domain} | API: ${resource} | listKey: ${listKey}

## Files to create
- src/models/${entityPascal}Model.ts
- src/services/${domain}/${entityKebab}/getAll.ts  → createPagedQuery("${resource}")
- src/services/${domain}/${entityKebab}/getById.ts, create.ts, update.ts, delete.ts
- src/components/${domain}/${entityKebab}/index.tsx   (useEntityCrudModule + EntityModuleShell)
- src/components/${domain}/${entityKebab}/list.tsx     (useEntityList + EntityListShell)
- src/components/${domain}/${entityKebab}/form.tsx
- Route in src/routes/index.tsx
- i18n keys in src/locales/en.json

## getAll.ts
import type { ${entityPascal}Model } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";
export default createPagedQuery<${entityPascal}Model>("${resource}");

## index.tsx (skeleton)
import { lazy, memo } from "react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";
const ${entityPascal}Form = memo(lazy(() => import("./form")));
const ${entityPascal}List = memo(lazy(() => import("./list")));

export default function ${entityPascal}() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();
  return (
    <EntityModuleShell
      title="${entityPascal}s"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<${entityPascal}Form id={id} setId={setId} />}
      list={<${entityPascal}List editHandler={editHandler} />}
    />
  );
}
`);
