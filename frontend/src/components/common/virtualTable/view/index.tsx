import { useState } from "react";

import { Printer } from "lucide-react";
import ButtonField from "@/components/ui/buttonField";
import PreviewModal from "./previewModal";
import type { ParameterModel } from "@/models";

function ReportPreview(props: { sourceData: []; param?: ParameterModel; reportInfo?: any }) {
  const { sourceData, param, reportInfo } = props;
  const [preview, setPreview] = useState(false);

  return (
    <div className="  ">
      {preview && (
        <PreviewModal
          onClose={() => setPreview(false)}
          dataSource={sourceData}
          param={param}
          reportInfo={reportInfo}
        />
      )}
      <div className="relative inline-flex ml-3 text-start   ">
        <ButtonField
          className={`p-1 transition-colors duration-200 bg-secondary/30 text-foreground hover:bg-primary/20`}
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
