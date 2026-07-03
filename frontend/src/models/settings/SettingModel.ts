import type AbstractModel from "../AbstractModel";

export default interface SettingModel extends AbstractModel {
  type?: string;
  settingKey?: string;
  settingValue?: string;
  description?: string;
}