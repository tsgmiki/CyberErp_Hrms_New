
import type{  UserPermissionModel } from "@/models";
import { signal } from "@preact/signals-react";

export const PermissionData = signal([] as UserPermissionModel[]);