import getHealthCheck from "../healthcheck/get";

class BackgroundSyncService {
  private syncInterval: ReturnType<typeof setInterval> | null = null;
  private readonly SYNC_INTERVAL = 5 * 60 * 1000;

  constructor() {
    void this.startBackgroundSync();
  }

  async startBackgroundSync() {
    await this.performFullSync();

    this.syncInterval = setInterval(async () => {
      await this.performIncrementalSync();
    }, this.SYNC_INTERVAL);
  }

  async stopBackgroundSync() {
    if (this.syncInterval) {
      clearInterval(this.syncInterval);
      this.syncInterval = null;
    }
  }

  async performFullSync() {
    console.log("Starting full background sync...");

    try {
      const isOnline = await this.checkConnectivity();
      if (!isOnline) {
        console.log("Offline - skipping sync");
        return;
      }

      await Promise.all([

      ]);
      console.log("Full background sync completed");
    } catch (error) {
      console.error("Full background sync failed:", error);
    }
  }

  private async performIncrementalSync() {
    await this.performFullSync();
  }

  private async checkConnectivity(): Promise<boolean> {
    try {
      await getHealthCheck();
      return true;
    } catch {
      return false;
    }
  }

}

export const backgroundSyncService = new BackgroundSyncService();
export { BackgroundSyncService };
