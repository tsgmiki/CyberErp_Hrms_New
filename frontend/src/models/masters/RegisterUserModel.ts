import type AbstractModel from "../AbstractModel";

export default interface RegisterUserModel extends AbstractModel {
  // User Information
  fullName?: string;
  email?: string;
  phoneNumber?: string;
  userName?: string;
  password?: string;
  confirmPassword?: string;
  
  // Tenant Information
  tenantName?: string;
  tenantIdentifier?: string;
  tenantAddress?: string;
  tenantPhoneNumber?: string;
}
