import { useQuery } from "@tanstack/react-query";
import LandingPage from "@/components/home/landingPage";
import Spinner from "@/components/common/spinner/spinner";
import GetAllModuleWithOperation from "@/services/admin/module/getAllWithOperation";
import type { ModuleModel } from "@/models";
import { subSystems } from "@/constants/subSystem";

function buildFallbackModules(): ModuleModel[] {
  return subSystems.map((subsystem, index) => ({
    id: String(index),
    name: subsystem.name,
    subSystem: subsystem.name,
    operations: [],
  }));
}

export default function LandingPageWrapper() {
  const { data, isLoading, isError } = useQuery({
    queryKey: ["moduleWithOperations"],
    queryFn: () => GetAllModuleWithOperation(),
    staleTime: 5 * 60 * 1000,
  });

  const modules = data?.data?.length ? data.data : isError ? buildFallbackModules() : [];

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-background">
        <Spinner size="lg" showLabel />
      </div>
    );
  }

  return <LandingPage modules={modules as ModuleModel[]} />;
}
