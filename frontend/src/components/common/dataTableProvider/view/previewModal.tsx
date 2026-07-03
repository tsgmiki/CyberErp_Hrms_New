"use client";
import { lazy, memo } from "react";
import Modal from "@/components/common/modal";
import type { ParameterModel } from "@/models";

const Detail = memo(lazy(() => import("./detail")));
function PreviewModal(props: {
  onClose: (visible: boolean) => void;
  dataSource: any[];
  param?: ParameterModel;
}) {
  const { onClose, dataSource, param } = props;

  return (
    <div className={"m-2"}>
      <Modal
        visible={true}
        title={"Report Preview"}
        onClose={onClose}
        showFullscreen={true}
      >
        <Detail sourceData={dataSource as never} param={param} />
      </Modal>
    </div>
  );
}
export default PreviewModal;
