import { useState } from "react";
import UploadFolhaPonto from "./pages/UploadFolhaPonto";
import DashboardAnomalias from "./pages/DashboardAnomalias";
import DashboardRisco from "./pages/DashboardRisco";

const EMPRESA_ID_DEV = "a1b2c3d4-0000-0000-0000-000000000001";

type Pagina = "upload" | "dashboard" | "risco";

export default function App() {
  const [pagina, setPagina] = useState<Pagina>("upload");

  const navClass = (p: Pagina) =>
    "px-4 py-2 rounded-md text-sm font-medium transition-colors " +
    (pagina === p ? "bg-indigo-100 text-indigo-700" : "text-gray-600 hover:bg-gray-100");

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 shadow-sm">
        <div className="max-w-5xl mx-auto px-6 py-4 flex items-center justify-between">
          <span className="font-bold text-indigo-700 text-lg tracking-tight">
            Vcorp Folha <span className="text-gray-400 font-normal text-sm">IA</span>
          </span>
          <nav className="flex gap-1">
            <button onClick={() => setPagina("upload")} className={navClass("upload")}>Upload de Folha</button>
            <button onClick={() => setPagina("dashboard")} className={navClass("dashboard")}>Dashboard de Anomalias</button>
            <button onClick={() => setPagina("risco")} className={navClass("risco")}>Dashboard de Risco</button>
          </nav>
        </div>
      </header>
      <main className="py-8">
        {pagina === "upload" && <UploadFolhaPonto empresaId={EMPRESA_ID_DEV} />}
        {pagina === "dashboard" && <DashboardAnomalias empresaId={EMPRESA_ID_DEV} />}
        {pagina === "risco" && <DashboardRisco empresaId={EMPRESA_ID_DEV} />}
      </main>
    </div>
  );
}