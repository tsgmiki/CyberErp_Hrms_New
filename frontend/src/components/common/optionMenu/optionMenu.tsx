import { ExternalLink } from "lucide-react";
import DropDownButton from "@/components/ui/dropDownButton";
import DialogModal from "../dialog";
import { useState } from "react";
import store from "@/store";
import { useSignals } from "@preact/signals-react/runtime";
import { useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";

function OptionMenus(props: { optionHander: Function; menu?: [] }) {
  useSignals();
  const { t } = useTranslation();
  const { optionHander, menu } = props;
  const [dialogMessage, setDialogMessage] = useState("");
  const [showDialog, setshowDialog] = useState(false);
  const [key, setKey] = useState("");
  const permissions = store.PermissionData.value;
  const location = useLocation();
  const pathName = location.pathname;
  const SelectHandler = (item: any) => {
    if (
      item.key == "delete" ||
      item.key == "approve" ||
      item.key == "reject" ||
      item.key == "revise"
    ) {
      setDialogMessage(t("Are you sure you want to {{action}}?", { action: item.key }));
      setKey(item.key);
      setshowDialog(true);
    } else optionHander(item.key);
  };
  const confirmHandler = () => {
    optionHander(key);
    setshowDialog(false);
  };
  return (
    <>
      <DialogModal
        visible={showDialog}
        title={t("Confirm")}
        onClose={setshowDialog}
        onOk={confirmHandler}
      >
        <span>{dialogMessage}</span>
      </DialogModal>
      <div className=" relative ml-3 flex justify-end ">
        <DropDownButton
          value=""
          className=" bg-secondary hover:bg-primary/20 text-primary rounded-full  mr-2 p-2 "
          menu={
            menu?.map((a: any) => {
              return {
                ...a,
                disable:
                  permissions?.filter(
                    (b) =>
                      pathName.includes(b.operation as string) &&
                      ((b.canApprove == false && a.label == "Approve") ||
                        (b.canDelete == false && a.label == "Delete"))
                  )?.length > 0
                    ? true
                    : false,
              };
            }) as never
          }
          icon={<ExternalLink size={17} className="text-primary" />}
          onClick={SelectHandler}
          disabled={false}
        />
      </div>
    </>
  );
}

export default OptionMenus;
