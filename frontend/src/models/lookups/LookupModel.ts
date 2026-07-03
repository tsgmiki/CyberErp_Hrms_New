import type AbstractModel from "../AbstractModel";
export default interface LookupModel extends AbstractModel {
  name?: string;
  remark?: string;
  code?: string;
  tableName?: string;
}
