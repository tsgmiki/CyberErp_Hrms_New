"use client";
import { v6 as uuid } from "uuid";
import { useTranslation } from "react-i18next";

function Summary(props: { totalSummary: string }) {
  const { t } = useTranslation();
  return (
    <div
      className={`p-2  mr-4 ml-4 border rounded-md bg-card border-border`}
    >
      <div className=" items-end">
        {props.totalSummary
          .split(";")
          .filter((a) => a != "")
          .map((a) => {
            return (
              <div
                key={uuid()}
                className="p-1 text-end font-semibold text-foreground"
              >
                <div className=" inline-flex gap-1">
                  {a.split(":").map((b) => {
                    return <div key={uuid()}>{t(b)}</div>;
                  })}
                </div>
              </div>
            );
          })}
      </div>
    </div>
  );
}

export default Summary;
