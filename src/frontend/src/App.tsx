import { useState } from "react";
import UploadFolhaPonto from "./pages/UploadFolhaPonto";
import DashboardAnomalias from "./pages/DashboardAnomalias";
import DashboardRisco from "./pages/DashboardRisco";
import Login from "./pages/Login";
import type { AuthState } from "./pages/Login";

type Pagina = "upload" | "dashboard" | "risco";

const ROLE_LABEL: Record<string, string> = {
  Dono: "Dono",
  Gestor: "Gestor",
  Colaborador: "Colaborador",
};

const ROLE_COLOR: Record<string, string> = {
  Dono: "bg-purple-100 text-purple-700",
  Gestor: "bg-blue-100 text-blue-700",
  Colaborador: "bg-green-100 text-green-700",
};

function loadAuth(): AuthState | null {
  const token = localStorage.getItem("token");
  const empresaId = localStorage.getItem("empresaId");
  const role = localStorage.getItem("role");
  const nome = localStorage.getItem("nome");
  if (token && empresaId && role && nome) return { token, empresaId, role, nome };
  return null;
}

export default function App() {
  const [auth, setAuth] = useState<AuthState | null>(loadAuth);
  const [pagina, setPagina] = useState<Pagina>("upload");

  function handleLogin(state: AuthState) {
    localStorage.setItem("token", state.token);
    localStorage.setItem("empresaId", state.empresaId);
    localStorage.setItem("role", state.role);
    localStorage.setItem("nome", state.nome);
    setAuth(state);
  }

  function handleLogout() {
    localStorage.removeItem("token");
    localStorage.removeItem("empresaId");
    localStorage.removeItem("role");
    localStorage.removeItem("nome");
    setAuth(null);
    setPagina("upload");
  }

  if (!auth) {
    return <Login onLogin={handleLogin} />;
  }

  const navClass = (p: Pagina) =>
    "px-4 py-2 rounded-md text-sm font-medium transition-colors " +
    (pagina === p ? "bg-indigo-100 text-indigo-700" : "text-gray-600 hover:bg-gray-100");

  const roleColor = ROLE_COLOR[auth.role] ?? "bg-gray-100 text-gray-600";
  const roleLabel = ROLE_LABEL[auth.role] ?? auth.role;

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 shadow-sm">
        <div className="max-w-5xl mx-auto px-6 py-4 flex items-center justify-between">
          <span className="font-bold text-indigo-700 text-lg tracking-tight">
            Vcorp Folha <span className="text-gray-400 font-normal text-sm">IA</span>
          </span>
          <nav className="flex gap-1 flex-1 justify-center">
            <button onClick={() => setPagina("upload")} className={navClass("upload")}>Upload de Folha</button>
            <button onClick={() => setPagina("dashboard")} className={navClass("dashboard")}>Dashboard de Anomalias</button>
            <button onClick={() => setPagina("risco")} className={navClass("risco")}>Dashboard de Risco</button>
          </nav>
          <div className="flex items-center gap-3">
            <span className="text-sm text-gray-700 font-medium hidden sm:inline">{auth.nome.split(" ")[0]}</span>
            <span className={`text-xs font-semibold px-2 py-1 rounded-full ${roleColor}`}>{roleLabel}</span>
            <button
              onClick={handleLogout}
              className="text-sm text-gray-500 hover:text-red-600 transition-colors px-3 py-1.5 rounded-md hover:bg-red-50 border border-gray-200"
            >
              Sair
            </button>
          </div>
        </div>
      </header>
      <main className="py-8">
        {pagina === "upload" && <UploadFolhaPonto empresaId={auth.empresaId} />}
        {pagina === "dashboard" && <DashboardAnomalias empresaId={auth.empresaId} />}
        {pagina === "risco" && <DashboardRisco empresaId={auth.empresaId} />}
      </main>
    </div>
  );
}
