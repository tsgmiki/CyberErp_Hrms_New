
import "@react-pdf-viewer/core/lib/styles/index.css";
import "@react-pdf-viewer/default-layout/lib/styles/index.css";

import { Viewer, Worker } from "@react-pdf-viewer/core";
import { defaultLayoutPlugin } from "@react-pdf-viewer/default-layout";
import { Download } from "lucide-react";

function DocumentViewer(props: { documentUrl: any }) {
  const { documentUrl } = props;

  const plugin = defaultLayoutPlugin();
  const isImage = /\.(png|jpe?g|gif|bmp|webp)$/i.test(documentUrl);
  const isPdf = /\.pdf$/i.test(documentUrl);

  return (
    <div style={{ height: "700px" }} className="m-2">
      <a
        target="_blank"
        className=" underline inline-flex gap-2 bg-slate-50 p-2 m-2"
        href={documentUrl}
      >
        {<Download />}
        Download
      </a>
      {isPdf && (
        <Worker
          workerUrl={`https://unpkg.com/pdfjs-dist@3.11.174/build/pdf.worker.min.js`}
        >
          <Viewer fileUrl={documentUrl} plugins={[plugin]} />
        </Worker>
      )}
      {isImage && (
        <img
          src={documentUrl}
          alt="Document preview"
          style={{ maxWidth: "100%" }}
        />
      )}
    </div>
  );
}

export default DocumentViewer;
