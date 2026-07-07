import type AbstractModel from "../AbstractModel";

export default interface JobCategoryModel extends AbstractModel {
  name?: string;
  code?: string;
  description?: string;
  isActive?: boolean;
}
