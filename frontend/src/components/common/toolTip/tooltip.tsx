import type { ReactNode } from "react";

function Tooltip(props: { message: string; children: ReactNode }) {
  const { message, children } = props;
  return (
    <div className="group relative flex ">
      {children}
      <span className=" z-50 absolute top-10 scale-0 transition-all rounded bg-gray-800 p-2 text-xs text-white group-hover:scale-100">
        {message}
      </span>
    </div>
  );
}
export default Tooltip;
