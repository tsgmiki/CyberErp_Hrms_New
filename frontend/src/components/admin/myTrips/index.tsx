import { lazy, memo, useState } from "react";
import { Plane } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const MyTripRequestForm = memo(lazy(() => import("./form")));
const MyTripList = memo(lazy(() => import("./list")));
const MyTripDetailModal = memo(lazy(() => import("./detailModal")));

/** HC260/HC262/HC264 — the signed-in employee requests trips, records expenses and settles advances. */
function MyTrips() {
  const { showForm, backHandler, addHandler } = useEntityCrudModule();
  const [selId, setSelId] = useState<string | null>(null);

  return (
    <>
      <EntityModuleShell
        title="My Trips"
        headerDescription="Request business trips, record expenses and settle your advances"
        headerIcon={<Plane className="h-6 w-6 text-primary" />}
        tableTitle="My Trips"
        showForm={showForm}
        onList={backHandler}
        onAdd={addHandler}
        form={<MyTripRequestForm onDone={backHandler} />}
        list={<MyTripList onSelect={setSelId} />}
      />
      {selId && <MyTripDetailModal id={selId} onClose={() => setSelId(null)} />}
    </>
  );
}

export default MyTrips;
