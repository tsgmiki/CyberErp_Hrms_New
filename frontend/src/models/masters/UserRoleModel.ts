import type AbstractModel from '../AbstractModel'


export default interface UserRoleModel extends AbstractModel {
  user?:string
  userId:string
  role?:string
  roleId:string
}
