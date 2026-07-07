import type AbstractModel from "../AbstractModel";

/** Admin-defined dynamic employee field (HC021). */
export default interface EmployeeFieldModel extends AbstractModel {
  name?: string;
  label?: string;
  dataType?: string; // Text | Number | Date | Boolean | Select
  options?: string; // comma-separated for Select
  isRequired?: boolean;
  isActive?: boolean;
  sortOrder?: number;
}
