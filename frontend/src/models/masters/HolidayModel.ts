import type AbstractModel from "../AbstractModel";

export default interface HolidayModel extends AbstractModel {
  date?: string;
  name?: string;
  nameA?: string;
  holidayType?: string;
  isRecurring?: boolean;
  description?: string;
  isActive?: boolean;
}
