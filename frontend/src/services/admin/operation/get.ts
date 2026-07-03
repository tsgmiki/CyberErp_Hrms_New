import type { OperationModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<OperationModel>("Operation");
