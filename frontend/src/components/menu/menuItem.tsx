
import type { ModuleModel, RolePermissionModel } from "@/models";

import { useEffect, useState } from "react";
import Modules from "./modules";
import { useSignals } from "@preact/signals-react/runtime";
import store from "@/store";

export default function MenuItem(props: {
  modules: ModuleModel[];
  rolePermissions?: RolePermissionModel[];
}) {
  useSignals();
  const { modules, rolePermissions } = props;
  const [activeIndex, setActiveIndex] = useState(-1);

  const moduleSelectHandler = (index: number) => {
    setActiveIndex(index === activeIndex ? -1 : index);
  };
  useEffect(() => {
    if (rolePermissions) {
      store.PermissionData.value = rolePermissions;
    }
  }, [rolePermissions]);
  return (
    <div className={`h-full flex bg-card flex-col gap-0 overflow-y-auto`}>
      <Modules
        modules={modules}
        activeIndex={activeIndex}
        setActiveIndex={moduleSelectHandler}
      ></Modules>
    </div>
  );
}
