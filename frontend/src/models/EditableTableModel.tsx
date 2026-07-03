import type EditableTableColumnModel from "./EditableTableColumnModel";

export default interface EditableTableModel {
  columns: EditableTableColumnModel[];
  data: any[];
  onChange: (data: any[]) => void;
  onRowAdd?: () => void;
  onRowDelete?: (rowIndex: number) => void;
  onRowSave?: (row: any, rowIndex: number) => void;
  showAddButton?: boolean;
  showDeleteButton?: boolean;
  showSaveButton?: boolean;
  addButtonText?: string;
  emptyRowTemplate?: any;
  maxRows?: number;
  minRows?: number;
  isLoading?: boolean;
  readOnly?: boolean;
  showSummary?: boolean;
  sortHandler?: (columnName: string) => void;
  validateRow?: (row: any) => { isValid: boolean; errors: Record<string, string> };
}
