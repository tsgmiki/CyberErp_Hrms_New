import ButtonField from "@/components/ui/buttonField";
import { ChevronLeft, ChevronRight, SearchCheck } from "lucide-react";
import { useEffect, useState, type ReactNode } from "react";
import Step from "../../ui/step";
import store from "@/store";
import { useTranslation } from "react-i18next";

interface Props {
  contents?: { id: number; label: any; content: ReactNode }[];
  finishAction: (e: any) => void;
  disbleNextStep?: boolean;
}
function Steps(props: Props) {
  const { t } = useTranslation();
  const { contents, disbleNextStep, finishAction } = props;
  const [currentStep, setStep] = useState(1);
  const stepCount = contents?.length;

  const [showNext, setShowNext] = useState(false);
  const onNext = () => {
    store.StepData.value.step = currentStep + 1;
    setStep(() => currentStep + 1);
  };
  const onBack = () => {
    store.StepData.value.step = currentStep - 1;
    setStep(currentStep - 1);
  };
  useEffect(() => {
    setShowNext(disbleNextStep as boolean);
  }, [disbleNextStep]);

  return (
    <div className="border border-text-primary rounded-md bg-text-primary-odo">
      <div className={`${"flex flex-col justify-between"}`}>
        <div
          className={` ${"flex flex-row justify-between pl-2 pr-2 ml-4 mr-4"}`}
        >
          {contents?.map((tab) => (
            <div key={tab.id}>
              <Step
                key={tab.id}
                label={tab.label}
                isActive={tab.id === currentStep}
              />
            </div>
          ))}
        </div>
        <div className=" w-full rounded-md p-2 text-sm">
          {contents?.filter((a) => a.id == currentStep)[0]?.content}
        </div>
      </div>
      <div className={"p-4"}>
        {typeof currentStep != "undefined" && currentStep > 1 && (
          <ButtonField
            className=" text-sm bg-text-primary-odo items-center align-middle p-1 pr-2 pl-2 m-1 border border-text-primary"
            value={t("Back")}
            icon={<ChevronLeft />}
            onClick={onBack}
            disabled={false}
          ></ButtonField>
        )}
        {stepCount != currentStep && (
          <ButtonField
            className={`${
              showNext && "bg-text-primary-accent/30"
            } text-sm bg-text-primary-odo items-center align-middle p-1 pr-2 pl-2  m-1 border border-text-primary`}
            value={t("Next")}
            icon={<ChevronRight />}
            onClick={onNext}
            disabled={stepCount == currentStep}
          ></ButtonField>
        )}
        {stepCount == currentStep && stepCount > 1 && (
          <ButtonField
            className=" text-sm bg-text-primary text-text-primary-odo items-center align-middle p-1 pr-2 pl-2  m-1 border border-text-primary"
            value={t("Finish")}
            icon={<SearchCheck />}
            onClick={finishAction}
            disabled={false}
          ></ButtonField>
        )}
      </div>
    </div>
  );
}

export default Steps;
