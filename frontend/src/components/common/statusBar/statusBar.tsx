interface Props {
  status: string;
  step?: number;
}
function StatusBar(props: Props) {
  const { status, step } = props;
  function FolderIcons({ step }: { step: number }) {
   
    return (
      <>
        {Array.from({ length: step }, (_, i) => (
          <div key={i} className=" inline-flex gap-1">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="1em"
              height="1em"
              viewBox="0 0 32 32"
              className={` ${status === "Done" && i === 2 ? "text-green-500" : "text-gray-500"} `}
            >
              <path
                fill="currentColor"
                d="M26 28H6a2 2 0 0 1-2-2V11a2 2 0 0 1 2-2h5.667a2 2 0 0 1 1.2.4l3.467 2.6H26a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2M11.667 11H5.998L6 26h20V14H15.667ZM28 9H17.667l-4-3H6V4h7.667a2 2 0 0 1 1.2.4L18.334 7H28Z"
              />
            </svg>
            {i === 0 && (
              <span
                className={`text-[12px] ${status === "draft" ? "text-green-500" : "text-gray-500"}`}
              >
                Draft
              </span>
            )}
            {i === 1 && (
              <span
                className={`text-[12px] ${status !== "Done" && status !== "draft" ? "text-green-500" : "text-gray-500"}`}
              >
                Pending
              </span>
            )}
            {i === 2 && (
              <span
                className={`text-[12px] ${status === "Done" ? "text-green-500" : "text-gray-500"}`}
              >
                Done
              </span>
            )}
          </div>
        ))}
      </>
    );
  }

  return (
    <div className=" grid grid-flow-col gap-0 ">
      <FolderIcons step={step as number} />
    </div>
  );
}

export default StatusBar;
