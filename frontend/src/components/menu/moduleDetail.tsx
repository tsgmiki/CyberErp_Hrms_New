import type { OperationModel } from "@/models";

import Tooltip from "../common/toolTip/tooltip";
import { icons } from "./icons";
import { v6 as uuid } from "uuid";
import { useTranslation } from "react-i18next";

export default function ModuleDetail(props: { operations: OperationModel[] }) {
  const { t } = useTranslation();
  const { operations } = props;

  return (
    <>
      {operations?.map((operation: any) => {
        return (
          <a
            key={uuid()}
            className="block text-theme-bg-primary text-[14px] p-2 pl-0  hover:bg-theme-accent/20 border-b-0 border-theme-accent"
            href={"/" + operation.link}
          >
            <span className=" inline-flex text-[14px]  text-theme-accent ml-4">
              <span className="pr-2 text-theme-accent pt-1">
                {
                  icons
                    .map((a) => ({
                      ...a,
                      items: a.details
                        ?.filter((b) => b.name == operation.name)
                        .map((c) => c.icon),
                    }))
                    .filter((a) => (a.items?.length as number) > 0)?.[0]
                    ?.items?.[0]
                }
              </span>

              <span className=" text-theme-accent">
                <Tooltip message={t(operation?.name || '')}> {t(operation?.name || '')}</Tooltip>
              </span>
            </span>
          </a>
        );
      })}
    </>
  );
}
