import { lazy, memo } from "react";
import { CalendarCog } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const FiscalYearForm = memo(lazy(() => import("./form")));
const FiscalYearList = memo(lazy(() => import("./list")));

function FiscalYear() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Fiscal Years"
      headerDescription="Fiscal periods that anchor leave balances, accrual and year-end rollover"
      headerIcon={<CalendarCog className="h-6 w-6 text-primary" />}
      tableTitle="Fiscal Years"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<FiscalYearForm id={id} setId={setId} />}
      list={<FiscalYearList editHandler={editHandler} />}
    />
  );
}

export default FiscalYear;
