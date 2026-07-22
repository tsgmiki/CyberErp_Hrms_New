# Cross-module end-to-end test

`e2e_full.mjs` drives a single realistic scenario across **Recruitment → Employee → Performance →
Career Development** against the running API, exercising every hand-off between the modules and their
integrations. It follows one person — *Meron Bekele* — from job applicant to ranked successor.

`cross-module-workflows.html` is the human-readable walkthrough of the same flow (open it in a browser):
each workflow, the cross-module link it creates, where to see it in the UI, and the database evidence.

## What it verifies

- **Recruitment → Employee** — hiring an accepted candidate creates an `Employee` on the candidate's shared `Person` (zero re-entry).
- **Performance → Career (HC153)** — finalizing an appraisal auto-refreshes the employee's succession readiness.
- **HC163** — career-path suggestions blend competency match with the latest appraisal (`fit` score).
- **HC167** — development goals generated from a gap are aligned to an organizational objective.
- **HC130 / HC155** — a competency gap becomes structured Individual Development Plans (career + succession).
- **HC158** — the Employee 360 "Development" profile bridges performance + career data.

## Run it

Prerequisites: the API running on `http://localhost:5014` (e.g. `dotnet run --project CyberErp.Hrms.Api`,
or Visual Studio / IIS Express), pointed at your database.

```bash
node docs/e2e/e2e_full.mjs <tenantIdentifier> <adminUserName> [phone]
# example — writes a reviewable dataset to the "demo" tenant:
node docs/e2e/e2e_full.mjs demo demo
```

It registers the tenant on first run, then seeds the whole scenario and prints a pass/fail line per step.
Sign in to the app as `demo` / `Passw0rd!` to explore the generated data in the UI (employee **Meron Bekele**,
`#EMP-1001`). Re-running against an existing tenant will add duplicate data — use a fresh identifier for a clean run.
