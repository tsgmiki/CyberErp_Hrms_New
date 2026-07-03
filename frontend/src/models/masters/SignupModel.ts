import type AbstractModel from "../AbstractModel";

export default interface SignupModel extends AbstractModel {
  fullName?: string;
  phoneNo?: string;
  email?: string;
  userName?: string;
  password?: string;
  companyName?: string;
  type?: string;
  address?: string;
  name?: string;
}
