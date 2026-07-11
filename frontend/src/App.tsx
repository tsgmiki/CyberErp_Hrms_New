import "./App.css";
import { BrowserRouter } from "react-router-dom";
import LoadingBar from "react-top-loading-bar";
import AppRoutes from "./routes";
import { Toaster } from "@/components/common/toast";

function App() {
  return (
    <BrowserRouter>
      <LoadingBar />
      <Toaster />
      <AppRoutes />
    </BrowserRouter>
  );
}

export default App
