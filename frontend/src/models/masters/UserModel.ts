import type AbstractModel from "../AbstractModel";
import type Name from "../Name";

export default interface UserModel extends Name, AbstractModel {
  fullName?: string;
  PhoneNumber?: string;
  email?: string;
  password?: string;
  userName?: string;
  isAdmin: boolean;
  emailConfirmed: boolean;
  /** The employee this login belongs to (null = system/owner). Drives branch scope + evaluator permissions. */
  employeeId?: string;
  employeeName?: string;
}
