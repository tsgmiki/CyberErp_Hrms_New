
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";


const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export default async function deleteRole(id: string) {

  try {
    const response = await fetch(API_BASE_URL + "/Role/" + id, {
      method: "DELETE",
      credentials: "include",
      headers: {
       
      },
    });

    if (response.ok) {
      const result = await response.json();
      return result;
    } else {
      const text = await response.text();
      const result = isValidJson(text) ? JSON.parse(text) : { message: text };
      const message =
        typeof result.reason != "undefined"
          ? result.reason
          : errorMessageParser(result.errors);
      return {
        status: "error",
        message: message,
      };
    }
  } catch (error) {
    const message = errorMessageParser(error);
    return {
      status: "error",
      message: message,
    };
  }
}
