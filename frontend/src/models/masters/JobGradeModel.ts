import type AbstractModel from "../AbstractModel";

export default interface JobGradeModel extends AbstractModel {
  name?: string;
  nameA?: string;
  code?: string;
}
