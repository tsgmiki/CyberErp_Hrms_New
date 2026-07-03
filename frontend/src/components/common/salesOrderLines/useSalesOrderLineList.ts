import { useCallback, useEffect, useState } from "react";
import { v6 as uuid } from "uuid";
import { toast } from "@/components/common/toast";

interface UseSalesOrderLineListOptions<T extends { id?: string }> {
  items: T[] | undefined | null;
  onItemsChange: (items: T[]) => void;
  isDuplicate?: (items: T[], record: T, editingId?: string) => boolean;
  duplicateMessage?: string;
}

export function useSalesOrderLineList<T extends { id?: string }>({
  items,
  onItemsChange,
  isDuplicate,
  duplicateMessage = "Item already exists",
}: UseSalesOrderLineListOptions<T>) {
  const [rows, setRows] = useState<T[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<Partial<T>>({});

  useEffect(() => {
    if (items != null) {
      setRows(items);
    }
  }, [items]);

  const sync = useCallback(
    (next: T[]) => {
      setRows(next);
      onItemsChange(next);
    },
    [onItemsChange],
  );

  const add = useCallback(
    (record: T) => {
      if (isDuplicate?.(rows, record, record.id)) {
        toast.error(duplicateMessage);
        return;
      }
      const others = rows.filter((r) => r.id !== record.id);
      const next = [...others, { ...record, id: record.id || uuid() } as T];
      sync(next);
      setShowModal(false);
    },
    [rows, isDuplicate, duplicateMessage, sync],
  );

  const remove = useCallback(
    (id: string) => {
      sync(rows.filter((r) => r.id !== id));
    },
    [rows, sync],
  );

  const handleUpdate = useCallback(
    (id: string, field: string, value: unknown) => {
      sync(
        rows.map((row) =>
          row.id === id ? ({ ...row, [field]: value } as T) : row,
        ),
      );
    },
    [rows, sync],
  );

  const editHandler = useCallback(
    (id: string, canEdit?: boolean) => {
      const record = rows.find((r) => r.id === id);
      if (record) {
        setEditing(record);
        if (canEdit) setShowModal(true);
      }
    },
    [rows],
  );

  const addEmptyRow = useCallback(
    (createEmpty: () => T) => {
      sync([...rows, createEmpty()]);
    },
    [rows, sync],
  );

  const toggleMobileModal = useCallback(() => {
    setShowModal((prev) => {
      if (prev) setEditing({});
      return !prev;
    });
  }, []);

  const openMobileAdd = useCallback(() => {
    setEditing({});
    setShowModal(true);
  }, []);

  return {
    rows,
    showModal,
    setShowModal,
    editing,
    setEditing,
    add,
    remove,
    handleUpdate,
    editHandler,
    addEmptyRow,
    toggleMobileModal,
    openMobileAdd,
  };
}
