import type { OperationModel, ParameterModel } from "@/models";


const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export default async function getAllOperationByRole(param: ParameterModel) {

  const queryString = new URLSearchParams({
        sortCol: param.sortCol,
    dir: param.dir,
    skip: String(param.skip),
    take: String(param.take),
  });

  const response = await fetch(
    API_BASE_URL + "/Operation/ByRole?" + queryString,
    {
      method: "GET",
      credentials: "include",
      headers: {
       
      },
    }
  );

  if (response.ok) {
    const res = await response.json();
    const data = res.data as OperationModel[];
    const count = res.count;

    return { data: data, count: count };
  }
}
