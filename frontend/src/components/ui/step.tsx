import { useTranslation } from "react-i18next";

const Step = ({ id, label, onClick, isActive }: any) => {
  const { t } = useTranslation();

  return (
    <div className="inline-flex items-center">
      <div className="mr-1 p-2 rounded-md" onClick={onClick}>
        {
          <div
            className={`border pt-3 text-center flex justify-center ${
              isActive ? "bg-primary text-on-accent" : "bg-secondary text-primary"
            } rounded-full w-8 h-8`}
          >
            {id}
          </div>
        }
      </div>
      <span
        className={`p-2 ${
          isActive
            ? "text-primary font-bold"
            : "text-secondary font-semibold"
        }`}
      >
        {label ? t(label) : ""}
      </span>
    </div>
  );
};

export default Step;
