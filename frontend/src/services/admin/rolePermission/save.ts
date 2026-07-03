import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import { RolePermissionSchema } from "@/components/util/validation";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export default async function saveRolePermissionService(formData: FormData) {
  const formDataObj = Object.fromEntries(formData);

  const result = RolePermissionSchema.safeParse({
    ...formDataObj,
    isRelatedToApplication:
      formDataObj.isRelatedToApplication == "true" ? true : false,
  });

  if (!result.success) {
    const zodErrors = result.error.flatten().fieldErrors;
    return {
      status: "error",
      message: "Validation failed",
      zodErrors,
    };
  }

  try {
    const details = JSON.parse(formDataObj.details as string);

    const response = await fetch(
      `${API_BASE_URL}/RolePermission`,
      {
        method: "POST",
        credentials: "include",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(
          { 
           roleId: formDataObj.roleId,
           items:(details as any[]).map((a) => {
             return {
               ...a,
               roleId: formDataObj.roleId,
               canView: a.canView == "true" ? true : false,
               canAdd: a.canAdd == "true" ? true : false,
               canEdit: a.canEdit == "true" ? true : false,
               canApprove: a.canApprove == "true" ? true : false,
               canDelete: a.canDelete == "true" ? true : false,
             };
           })}
        ),
      }
    );

    if (!response.ok) {
      const text = await response.text();
      const result = isValidJson(text) ? JSON.parse(text) : { message: text };
      const message = errorMessageParser(result.errors || result);
      return {
        status: "error",
        message,
        zodErrors: {},
      };
    }

    return {
      status: "success",
      message: "Successfully saved",
      zodErrors: {},
    };
  } catch (error) {
    return {
      status: "error",
      message: "Network error",
      zodErrors: {},
    };
  }
}
