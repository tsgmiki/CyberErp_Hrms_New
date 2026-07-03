
import { PDFViewer } from "@react-pdf/renderer";
import type { ReactElement } from "react";

interface props {
  children: ReactElement<any>;
}

function PdfViewer(props: props) {
  const { children } = props;

  return <PDFViewer className=" min-h-screen w-full" >{children}</PDFViewer>;
}
export default PdfViewer;
