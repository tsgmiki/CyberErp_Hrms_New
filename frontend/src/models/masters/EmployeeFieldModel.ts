import type AbstractModel from "../AbstractModel";

/** Admin-defined dynamic custom field (HC021). */
export default interface EmployeeFieldModel extends AbstractModel {
  /** Which form this field applies to: Employee | Education | Experience | Dependent | Movement | Discipline | Termination */
  ownerType?: string;
  name?: string;
  label?: string;
  dataType?: string; // Text | Number | Date | Boolean | Select
  options?: string; // comma-separated for Select
  isRequired?: boolean;
  isActive?: boolean;
  sortOrder?: number;
}
