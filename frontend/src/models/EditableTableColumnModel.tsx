export default interface EditableTableColumnModel {
  name: string;
  label: string;
  type?: 'text' | 'number' | 'select' | 'date' | 'checkbox'|'dropdown';
  editable?: boolean;
  required?: boolean;
  width?: string;
  responsive?: 'sm' | 'md' | 'lg';
  options?: { label: string; value: string | number }[];
  render?: (value: any, record: any, onChange: (value: any) => void) => React.ReactNode;
  validate?: (value: any, record: any) => string | null;
  defaultValue?: any;
  sort?: boolean;
  error?: string;
}
