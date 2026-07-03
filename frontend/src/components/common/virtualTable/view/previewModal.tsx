"use client";
import { lazy, memo } from "react";
import Modal from "@/components/common/modal";
import type { ParameterModel } from "@/models";

const Detail = memo(lazy(() => import("./detail")));
function PreviewModal(props: {
  onClose: (visible: boolean) => void;
  dataSource: any[];
  param?: ParameterModel;
  reportInfo?: any;
}) {
  const { onClose, dataSource, param, reportInfo } = props;

  return (
    <div className="h-[80vh]">
      <Modal
        visible={true}
        title={"Data Preview"}
        onClose={onClose}
        showFullscreen={true}
      >
        <div className="h-[70vh]">
          <Detail sourceData={dataSource as never} param={param} reportInfo={reportInfo} />
        </div>
      </Modal>
    </div>
  );
}
export default PreviewModal;
