import {useState } from 'react';
interface SyncStatus {
  stores: number;
  customers: number;
  items: number;
  banks: number;
  taxes: number;
  lastSync: string | null;
  isOnline: boolean;
  error?: string;
}

export function useBackgroundSync() {
  const [syncStatus] = useState<SyncStatus>({
    stores: 0,
    customers: 0,
    items: 0,
    banks: 0,
    taxes: 0,
    lastSync: null,
    isOnline: false,
  });
  const [isSyncing] = useState(false);

  return {
    syncStatus,
    isSyncing,
  };
}
