import { useCallback, useState } from "react";

/**
 * Standard list ↔ form state for InventoryLayout modules.
 */
export function useEntityCrudModule(initialId = "") {
  const [id, setId] = useState(initialId);
  const [showForm, setShowForm] = useState(false);

  const backHandler = useCallback(() => setShowForm(false), []);
  const addHandler = useCallback(() => {
    setId("");
    setShowForm(true);
  }, []);
  const editHandler = useCallback((recordId: string) => {
    setId(recordId);
    setShowForm(true);
  }, []);

  return {
    id,
    setId,
    showForm,
    backHandler,
    addHandler,
    editHandler,
  };
}
