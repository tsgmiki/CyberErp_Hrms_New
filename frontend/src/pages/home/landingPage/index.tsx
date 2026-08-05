import { useQuery } from "@tanstack/react-query";
import LandingPage from "@/components/home/landingPage";
import Spinner from "@/components/common/spinner/spinner";
import GetAllModuleWithOperation from "@/services/admin/module/getAllWithOperation";
import getAllSubsystems from "@/services/admin/subsystem/getAll";
import { parameterInitialData } from "@/constants/initialization";
import type { ModuleModel, SubsystemModel } from "@/models";

/**
 * Subsystem picker — everything is read LIVE from the shared navigation tables:
 * dbo.coreSubsystem (the subsystem master, incl. each application's URL) and
 * dbo.coreModule / coreOperation (the role-visible menu feed). Nothing is hardcoded.
 */
export default function LandingPageWrapper() {
  const { data, isLoading } = useQuery({
    queryKey: ["moduleWithOperations"],
    queryFn: () => GetAllModuleWithOperation(),
    staleTime: 5 * 60 * 1000,
  });

  const { data: subsystems, isLoading: subsystemsLoading } = useQuery({
    queryKey: ["subsystems", "landing"],
    queryFn: () => getAllSubsystems({ ...parameterInitialData, take: 200 }),
    staleTime: 5 * 60 * 1000,
  });

  // Wait for BOTH feeds: the auto-forward after login needs the subsystem URLs to deep-link.
  if (isLoading || subsystemsLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-background">
        <Spinner size="lg" showLabel />
      </div>
    );
  }

  return (
    <LandingPage
      modules={(data?.data ?? []) as ModuleModel[]}
      subsystems={(subsystems?.data ?? []) as SubsystemModel[]}
    />
  );
}
