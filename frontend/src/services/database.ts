import getHealthCheck from "./healthcheck/get";

/** Placeholder catalog shape for optional local/cache flows (no offline sales orders). */
export interface Item {
  id: string;
  name: string;
  description?: string;
  price: number;
  stock_quantity: number;
  category: string;
  last_updated: string;
}

class DatabaseService {
  async checkConnectivity(): Promise<boolean> {
    try {
      await getHealthCheck();
      return true;
    } catch {
      return false;
    }
  }

  async getItems(): Promise<Item[]> {
    return [];
  }

  async openExternalUrl(url: string): Promise<void> {
    window.open(url, "_blank", "noopener,noreferrer");
  }
}

export const databaseService = new DatabaseService();
