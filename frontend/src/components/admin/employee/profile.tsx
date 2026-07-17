"use client";
import { lazy, memo, useState } from "react";
import { useTranslation } from "react-i18next";
import {
  UserRound,
  GraduationCap,
  BriefcaseBusiness,
  HeartHandshake,
  ArrowLeftRight,
  Gavel,
  UserX,
  CalendarCheck,
  LayoutGrid,
  Rocket,
} from "lucide-react";
import { useDynamicForms } from "@/components/common/dynamicForm/useDynamicForms";
import DynamicFormSection from "@/components/common/dynamicForm/DynamicFormSection";

const MasterForm = memo(lazy(() => import("./masterForm")));
const EducationSection = memo(lazy(() => import("./educationSection")));
const ExperienceSection = memo(lazy(() => import("./experienceSection")));
const FamilySection = memo(lazy(() => import("./familySection")));
const MovementSection = memo(lazy(() => import("./movementSection")));
const DisciplineSection = memo(lazy(() => import("./disciplineSection")));
const TerminationSection = memo(lazy(() => import("./terminationSection")));
const LeaveRequestSection = memo(lazy(() => import("./leaveRequestSection")));
const DevelopmentSection = memo(lazy(() => import("./developmentSection")));

type TabKey =
  | "personal"
  | "education"
  | "experience"
  | "family"
  | "movements"
  | "leave"
  | "discipline"
  | "termination"
  | "development";

const TABS: { key: TabKey; label: string; Icon: typeof UserRound; needsId: boolean }[] = [
  { key: "personal", label: "Personal & Identification", Icon: UserRound, needsId: false },
  { key: "education", label: "Education", Icon: GraduationCap, needsId: true },
  { key: "experience", label: "Experience", Icon: BriefcaseBusiness, needsId: true },
  { key: "family", label: "Family", Icon: HeartHandshake, needsId: true },
  { key: "movements", label: "Movements", Icon: ArrowLeftRight, needsId: true },
  { key: "leave", label: "Annual Leave", Icon: CalendarCheck, needsId: true },
  { key: "development", label: "Development", Icon: Rocket, needsId: true },
  { key: "discipline", label: "Discipline", Icon: Gavel, needsId: true },
  { key: "termination", label: "Termination", Icon: UserX, needsId: true },
];

interface Props {
  id: string;
  setId: (id: string) => void;
  onBack: () => void;
  /** Org unit selected in the tree — scopes the Position dropdown on the master form. */
  orgUnitId?: string;
  orgUnitName?: string;
}

/** Tabbed employee profile (ERP-standard): master data + child collections per tab. */
function EmployeeProfile({ id, setId, onBack, orgUnitId, orgUnitName }: Props) {
  const { t } = useTranslation();
  // `tab` is a string so it can also hold a dynamic form id (custom tabs live alongside the fixed ones).
  const [tab, setTab] = useState<string>("personal");
  const hasId = !!id;
  const { data: dynamicForms } = useDynamicForms("Employee");

  return (
    <div className="flex h-full min-h-0 flex-col">
      <div className="mx-1 flex flex-wrap gap-1 border-b border-border pb-0">
        {TABS.map(({ key, label, Icon, needsId }) => {
          const disabled = needsId && !hasId;
          const active = tab === key;
          return (
            <button
              key={key}
              type="button"
              disabled={disabled}
              title={disabled ? t("Save the employee first") : undefined}
              onClick={() => setTab(key)}
              className={`-mb-px flex items-center gap-1.5 rounded-t-lg border-x border-t px-3.5 py-2 text-[13px] font-medium transition-colors ${
                active
                  ? "border-border bg-card text-primary"
                  : "border-transparent text-muted hover:text-foreground"
              } ${disabled ? "cursor-not-allowed opacity-40" : ""}`}
            >
              <Icon className="h-4 w-4" />
              {t(label)}
            </button>
          );
        })}
        {/* User-defined custom tabs (Form Builder) — always need a saved employee. */}
        {(dynamicForms ?? []).map((f) => {
          const disabled = !hasId;
          const active = tab === f.id;
          return (
            <button
              key={f.id}
              type="button"
              disabled={disabled}
              title={disabled ? t("Save the employee first") : undefined}
              onClick={() => setTab(f.id!)}
              className={`-mb-px flex items-center gap-1.5 rounded-t-lg border-x border-t px-3.5 py-2 text-[13px] font-medium transition-colors ${
                active
                  ? "border-border bg-card text-primary"
                  : "border-transparent text-muted hover:text-foreground"
              } ${disabled ? "cursor-not-allowed opacity-40" : ""}`}
            >
              <LayoutGrid className="h-4 w-4" />
              {t(f.label ?? f.name ?? "")}
            </button>
          );
        })}
      </div>

      <div className="min-h-0 flex-1 overflow-auto pt-2">
        {tab === "personal" && (
          <MasterForm
            id={id}
            orgUnitId={orgUnitId}
            orgUnitName={orgUnitName}
            onSaved={(savedId, isNew) => {
              setId(savedId);
              if (!isNew) onBack();
            }}
          />
        )}
        {tab === "education" && hasId && <EducationSection employeeId={id} />}
        {tab === "experience" && hasId && <ExperienceSection employeeId={id} />}
        {tab === "family" && hasId && <FamilySection employeeId={id} />}
        {tab === "movements" && hasId && <MovementSection employeeId={id} />}
        {tab === "leave" && hasId && <LeaveRequestSection employeeId={id} />}
        {tab === "development" && hasId && <DevelopmentSection employeeId={id} />}
        {tab === "discipline" && hasId && <DisciplineSection employeeId={id} />}
        {tab === "termination" && hasId && <TerminationSection employeeId={id} />}
        {/* Active custom tab content */}
        {hasId &&
          (dynamicForms ?? [])
            .filter((f) => f.id === tab)
            .map((f) => (
              <DynamicFormSection key={f.id} form={f} ownerType="Employee" ownerId={id} />
            ))}
      </div>
    </div>
  );
}

export default EmployeeProfile;
