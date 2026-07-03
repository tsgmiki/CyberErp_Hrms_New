
import { useBackgroundSync } from '../../services/background/useBackgroundSync';
import { Wifi, WifiOff, Database, Users, Package, Store, Building, Receipt } from 'lucide-react';

export function BackgroundSyncStatus() {
  const { syncStatus } = useBackgroundSync();

  const formatLastSync = (dateString: string | null) => {
    if (!dateString) return 'Never';
    const date = new Date(dateString);
    return date.toLocaleString();
  };

  return (
    <div className="bg-white border border-gray-200 rounded-lg shadow-sm p-4">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-semibold text-gray-800">Background Sync Status</h3>
        <div className="flex items-center space-x-2">
          {syncStatus.isOnline ? (
            <div className="flex items-center space-x-1 text-green-600">
              <Wifi className="w-4 h-4" />
              <span className="text-sm font-medium">Online</span>
            </div>
          ) : (
            <div className="flex items-center space-x-1 text-red-600">
              <WifiOff className="w-4 h-4" />
              <span className="text-sm font-medium">Offline</span>
            </div>
          )}

        </div>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4 mb-4">
        <div className="flex items-center space-x-3 p-3 bg-blue-50 rounded-lg">
          <Store className="w-8 h-8 text-blue-600" />
          <div>
            <div className="text-2xl font-bold text-blue-800">{syncStatus.stores}</div>
            <div className="text-sm text-blue-600">Stores</div>
          </div>
        </div>

        <div className="flex items-center space-x-3 p-3 bg-green-50 rounded-lg">
          <Users className="w-8 h-8 text-green-600" />
          <div>
            <div className="text-2xl font-bold text-green-800">{syncStatus.customers}</div>
            <div className="text-sm text-green-600">Customers</div>
          </div>
        </div>

        <div className="flex items-center space-x-3 p-3 bg-purple-50 rounded-lg">
          <Package className="w-8 h-8 text-purple-600" />
          <div>
            <div className="text-2xl font-bold text-purple-800">{syncStatus.items}</div>
            <div className="text-sm text-purple-600">Items</div>
          </div>
        </div>

        <div className="flex items-center space-x-3 p-3 bg-orange-50 rounded-lg">
          <Building className="w-8 h-8 text-orange-600" />
          <div>
            <div className="text-2xl font-bold text-orange-800">{syncStatus.banks}</div>
            <div className="text-sm text-orange-600">Banks</div>
          </div>
        </div>

        <div className="flex items-center space-x-3 p-3 bg-red-50 rounded-lg">
          <Receipt className="w-8 h-8 text-red-600" />
          <div>
            <div className="text-2xl font-bold text-red-800">{syncStatus.taxes}</div>
            <div className="text-sm text-red-600">Taxes</div>
          </div>
        </div>
      </div>

      <div className="border-t border-gray-200 pt-3">
        <div className="flex items-center justify-between text-sm text-gray-600">
          <div className="flex items-center space-x-2">
            <Database className="w-4 h-4" />
            <span>Last sync: {formatLastSync(syncStatus.lastSync)}</span>
          </div>

        </div>
      </div>

      {syncStatus.error && (
        <div className="mt-3 p-3 bg-red-50 border border-red-200 rounded-lg">
          <div className="flex items-center space-x-2 text-red-800">
            <span className="text-sm font-medium">Error:</span>
            <span className="text-sm">{syncStatus.error}</span>
          </div>
        </div>
      )}
    </div>
  );
}
