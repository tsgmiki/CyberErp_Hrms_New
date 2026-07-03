import type AbstractModel from '../AbstractModel'
import type Name from '../Name'

export default interface ChangePasswordModel extends Name,AbstractModel {
 password?:string,
 oldPassword?:string
 confirmPassword?:string
}
