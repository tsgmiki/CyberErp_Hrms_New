import { lazy, memo } from "react";
import { BookOpenCheck } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const TrainingCourseForm = memo(lazy(() => import("./form")));
const TrainingCourseList = memo(lazy(() => import("./list")));

function TrainingCourse() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Course Catalog"
      headerDescription="Training programs and courses — internal and external providers"
      headerIcon={<BookOpenCheck className="h-6 w-6 text-primary" />}
      tableTitle="Course Catalog"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<TrainingCourseForm id={id} setId={setId} />}
      list={<TrainingCourseList editHandler={editHandler} />}
    />
  );
}

export default TrainingCourse;
