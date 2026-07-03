
import "./App.css";
import { BrowserRouter } from "react-router-dom";
import LoadingBar from "react-top-loading-bar";
import AppRoutes from "./routes";
import { useEffect } from "react";
import { backgroundSyncService } from "./services/background/backgroundSyncService";
import { Toaster } from "@/components/common/toast";

function App() {
  useEffect(() => {
    // Initialize background sync service when app starts
    console.log('🚀 Initializing background sync service...');
    
    // Cleanup on unmount
    return () => {
      backgroundSyncService.stopBackgroundSync();
    };
  }, []);

  return (
    <BrowserRouter>
      <LoadingBar />
      <Toaster />
      <AppRoutes />
    </BrowserRouter>
  );
}

export default App
