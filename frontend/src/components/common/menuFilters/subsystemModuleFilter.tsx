"use client";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import DropDownField from "@/components/ui/dropDownField";
import getAllSubsystems from "@/services/admin/subsystem/getAll";
import getAllModule from "@/services/admin/module/getAll";
import { parameterInitialData } from "@/constants/initialization";

export interface MenuScope {
  subsystemId?: string;
  subsystemName?: string;
  moduleId?: string;
  moduleName?: string;
}

interface SubsystemModuleFilterProps {
  value: MenuScope;
  onChange: (scope: MenuScope) => void;
  /** Hide the module half (e.g. the Menu Modules screen filters by subsystem only). */
  showModule?: boolean;
}

const ALL = { id: "", name: "All" };

/**
 * Cascading central-administration filter: Subsystem → Module, both read LIVE from
 * dbo.coreSubsystem / dbo.coreModule. Shared by the System screens (Role Permissions,
 * Menu Modules, Menu Operations) so every subsystem — HRMS, Home, Finance, … — is
 * configurable from one place. Choosing a subsystem resets the module; "All" clears.
 */
export default function SubsystemModuleFilter({
  value,
  onChange,
  showModule = true,
}: SubsystemModuleFilterProps) {
  const { t } = useTranslation();
  const [subsystemParam, setSubsystemParam] = useState({ ...parameterInitialData, take: 200 });
  const [moduleParam, setModuleParam] = useState({ ...parameterInitialData, take: 500 });

  const { data: subsystems, isLoading: subsystemsLoading } = useQuery({
    queryKey: ["subsystems", "menuFilter", subsystemParam],
    queryFn: () => getAllSubsystems(subsystemParam),
    staleTime: 60_000,
  });

  const { data: modules, isLoading: modulesLoading } = useQuery({
    queryKey: ["modules", "menuFilter", moduleParam, value.subsystemId ?? ""],
    queryFn: () => getAllModule({ ...moduleParam, subsystemId: value.subsystemId || undefined }),
    staleTime: 60_000,
    enabled: showModule,
  });

  const subsystemOptions = useMemo(
    () => [ALL, ...(subsystems?.data ?? []).map((s) => ({ id: s.id ?? "", name: s.name ?? "" }))],
    [subsystems],
  );
  const moduleOptions = useMemo(
    () => [ALL, ...(modules?.data ?? []).map((m) => ({ id: m.id ?? "", name: m.name ?? "" }))],
    [modules],
  );

  return (
    <>
      <DropDownField
        type="dropDown"
        name="subsystemId"
        compact
        placeholder={t("All Subsystems")}
        value={value.subsystemId ?? ""}
        displayValue={value.subsystemName ?? ""}
        param={subsystemParam}
        setParam={setSubsystemParam as never}
        isLoading={subsystemsLoading}
        data={subsystemOptions as never}
        onSelect={(_n: string, item: { id: string; name: string }) =>
          // Cascading: a subsystem change always resets the module scope.
          onChange({
            subsystemId: item.id || undefined,
            subsystemName: item.id ? item.name : undefined,
          })
        }
      />
      {showModule && (
        <DropDownField
          type="dropDown"
          name="moduleId"
          compact
          placeholder={t("All Modules")}
          value={value.moduleId ?? ""}
          displayValue={value.moduleName ?? ""}
          param={moduleParam}
          setParam={setModuleParam as never}
          isLoading={modulesLoading}
          data={moduleOptions as never}
          onSelect={(_n: string, item: { id: string; name: string }) =>
            onChange({
              ...value,
              moduleId: item.id || undefined,
              moduleName: item.id ? item.name : undefined,
            })
          }
        />
      )}
    </>
  );
}
