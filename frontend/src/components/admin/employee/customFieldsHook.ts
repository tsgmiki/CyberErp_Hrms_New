import { useCallback, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import getAllEmployeeField from "@/services/admin/employeeField/getAll";
import { parameterInitialData } from "@/constants/initialization";
import { buildCustomFieldComponents } from "./customFieldConfigs";
import type { FormComponentModel } from "@/models";

/** Query param that fetches the active custom-field definitions (HC021) for one owner form. */
export const customFieldParam = (ownerType: string) => ({
  ...parameterInitialData,
  take: 200,
  status: "true",
  ownerType,
});

/** Maps a record's `{ name → value }` custom-field map to the `cf_`-prefixed keys the form binds to. */
export const hydrateCustomData = (customFields?: Record<string, string | null> | null) => {
  const cf: Record<string, string> = {};
  for (const [k, v] of Object.entries(customFields ?? {})) cf[`cf_${k}`] = v ?? "";
  return cf;
};

/**
 * Encapsulates the dynamic custom-field (HC021) plumbing for a child form: fetches the active
 * definitions for `ownerType`, holds their values in a dedicated state (so definition names can't
 * clobber the form's own fields), and yields the FormProvider components to spread into the form.
 * The rendered inputs are `cf_`-named, so they post via native FormData and are gathered back into a
 * `customFields` dict by `createSaveService({ customFields: true })`.
 *
 * Call `hydrate(record.customFields)` when opening an existing record for edit.
 */
export function useCustomFields(ownerType: string) {
  const [customData, setCustomData] = useState<Record<string, string>>({});
  const { data } = useQuery({
    queryKey: ["customFields", ownerType],
    queryFn: () => getAllEmployeeField(customFieldParam(ownerType)),
  });

  const onChange = useCallback((e: any) => {
    const { name, value } = e.target;
    setCustomData((p) => ({ ...p, [name]: value }));
  }, []);
  const onSelect = useCallback((name: string, r: any) => {
    setCustomData((p) => ({ ...p, [name]: r.id }));
  }, []);
  const hydrate = useCallback(
    (customFields?: Record<string, string | null> | null) => setCustomData(hydrateCustomData(customFields)),
    [],
  );

  const fields: FormComponentModel[] = buildCustomFieldComponents(data?.data, customData, onChange, onSelect);
  const hasFields = (data?.data?.length ?? 0) > 0;

  // The fields to spread into a FormProvider `components` array: a section divider + the fields.
  const components: FormComponentModel[] = hasFields
    ? [
        { name: "cf_divider", type: "break", label: "Additional Information", colSpan: "full" } as FormComponentModel,
        ...fields,
      ]
    : [];

  return { components, hasFields, hydrate };
}
