export default async function GetAllModuleWithOperation() {
  const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
  const queryString = new URLSearchParams({
    take: String(100),
    skip: String(0),
  });
  const response = await fetch(
    `${API_BASE_URL}/Module/WithOperations?${queryString}`,
    {
      method: "GET",
      credentials: "include",
      headers: {},
    },
  );
  if (response.ok) {
    const res = await response.json();
    console.log(res);

    return { data: res };
  }
}
