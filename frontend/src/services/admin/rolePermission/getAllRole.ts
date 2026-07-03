import type { ParameterModel, RolePermissionModel } from "@/models";


export default async function GetAllRolePermissionRole(param: ParameterModel) {

  const queryString = new URLSearchParams({
        sortCol: param.sortCol,
    dir: param.dir,
    take: String(param.take),
    limit: String(param.skip),
  });

  const response = await fetch(
    process.env.NEXT_PUBLIC_REACT_APP_API_URL +
      "RolePermission/RoleList?" +
      queryString,
    {
      method: "GET",credentials: "include",

      headers: {
       
      },
    }
  );
  if (response.ok) {
    const res = await response.json();
    const data = (await res.data) as RolePermissionModel[];
    const count = await res.count;

    return { data: data, count: count };
  }
}
