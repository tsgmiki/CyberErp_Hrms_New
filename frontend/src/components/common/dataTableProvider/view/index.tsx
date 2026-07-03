import { useState } from "react";

import { Printer } from "lucide-react";
import ButtonField from "@/components/ui/buttonField";
import PreviewModal from "./previewModal";
import type { ParameterModel } from "@/models";
function ReportPreview(props: { sourceData: []; param?: ParameterModel }) {
  const { sourceData, param } = props;
  const [preview, setPreview] = useState(false);
  return (
    <div className="  ">
      {preview && (
        <PreviewModal
          onClose={() => setPreview(false)}
          dataSource={sourceData}
          param={param}
        />
      )}
      <div className="relative inline-flex ml-3 text-start   ">
        <ButtonField
          className="bg-slate-50 text-slate-500 p-1"
          onClick={() => setPreview(true)}
          icon={<Printer className="mt-1" size={14} />}
          value={"Print Preview"}
          disabled={false}
        ></ButtonField>
      </div>
    </div>
  );
}

export default ReportPreview;
