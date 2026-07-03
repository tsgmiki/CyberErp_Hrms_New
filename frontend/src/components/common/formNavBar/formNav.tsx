import ButtonField from "@/components/ui/buttonField";
import { ChevronLeft, CirclePlus } from "lucide-react";
import { useTranslation } from "react-i18next";

interface Props {
  onAdd?: (e: any) => void;
  onEdit?: (e: any) => void;
  onaddChild?: (e: any) => void;
  onList?: (e: any) => void;
  onAction?: (e: any) => void;
  title: string;
  category?: string;
  showEdit?: boolean;
  showAddChild?: boolean;
  showForm?: boolean;
  showAction?: boolean;
  actionName?: string;
  showBack?: boolean;
  hideAdd?: boolean;
}
function FormNavBar(props: Props) {
  const { t } = useTranslation();
  const {
    onAdd,
    onList,
    onAction,
    showBack,
    title,
    category,
    showAction,
    showForm,
    actionName,
    hideAdd,
  } = props;

  return (
    <div className=" pt-2  pl-2 mr-3">
      <div className="ml-2 flex justify-between">
        <div className="pb-0">
          {(showForm || showAction) && !showBack && (
            <ButtonField
              value=""
              className="b-0 font-bold text-md text-foreground"
              onClick={onList}
              icon={
                <ChevronLeft className={" text-primary pt-2"} size={24} />
              }
              disabled={false}
            />
          )}
          {title != "" && (
            <label
              className={` inline-flex font-bold  ml-4 max-md:ml-0 text-foreground text-2xl 
        `}
            >
              {title}
            </label>
          )}
        </div>

        <div className="justify-end">
          {" "}
          {showAction && !showForm && showBack && (
            <ButtonField
              className="b-white text-green border border-border"
              onClick={onAction}
              value={actionName as string}
              icon="CheckCircleFilled"
              disabled={false}
            />
          )}
          {(!showForm || (showAction && showBack)) && !hideAdd && (
            <ButtonField
              className=" text-foreground p-1 text-lg justify-center rounded-md drop-shadow-md  bg-primary  pl-2 pr-2 hover:bg-foreground border border-border shadow-sm shadow-primary/20"
              onClick={onAdd}
              value={t("New")}
              icon={
                <CirclePlus
                  className=" font-normal mt-0.5  text-foreground"
                  size={15}
                />
              }
              disabled={false}
            />
          )}
        </div>
      </div>
      <div
        className={` inline-flex font-semibold p-4  text-foreground   ml-4 rounded-md 
        `}
      >
        <span>{category && t("Category:") + "(" + category + ")"}</span>
      </div>
    </div>
  );
}

export default FormNavBar;
