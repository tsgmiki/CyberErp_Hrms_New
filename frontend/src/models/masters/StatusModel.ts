import type AbstractModel from "../AbstractModel";

export default interface StatusModel extends AbstractModel {
  name: string;
  code: string;
  description?: string;       // nullable in C#
  dataTypeId: string;         // Guid → string
  plannedDuration: number;    // decimal → number
  remark?: string;            // nullable in C#
  openToClient?: boolean;     // nullable in C#
  dataType?: string;     // nullable in C#
  statusType?: string;        // nullable in C#
}
