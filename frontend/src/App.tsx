import "./App.css";
import { BrowserRouter } from "react-router-dom";
import LoadingBar from "react-top-loading-bar";
import AppRoutes from "./routes";
import { Toaster } from "@/components/common/toast";
import { RouteErrorBoundary } from "@/components/common/errorBoundary";

function App() {
  return (
    <BrowserRouter>
      <LoadingBar />
      <Toaster />
      {/* App-wide safety net: a render error shows a fallback instead of blanking the whole SPA. */}
      <RouteErrorBoundary>
        <AppRoutes />
      </RouteErrorBoundary>
    </BrowserRouter>
  );
}

export default App
