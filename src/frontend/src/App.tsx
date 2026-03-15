import { useState } from "react";
import UploadFolhaPonto from "./pages/UploadFolhaPonto";
import DashboardAnomalias from "./pages/DashboardAnomalias";
import DashboardRisco from "./pages/DashboardRisco";
import Login from "./pages/Login";
import type { AuthState } from "./pages/Login";
import vcorpIcon from "./assets/Icone_fundo_Cinza.png";

// ─── Paleta Vcorp (header dark) ───────────────────────────────────────────────
const B = {
  bg:          "#0F1613",
  border:      "#1E2E28",
  accent:      "#00B090",
  accentLight: "#00C8A8",
  textPrimary: "#EDF2F0",
  textMuted:   "#5E7A72",
  card:        "#141E1A",
};

const ROLE_BADGE: Record<string, { label: string; color: string; bg: string }> = {
  Dono:        { label: "Dono",        color: "#C084FC", bg: "rgba(192,132,252,0.12)" },
  Gestor:      { label: "Gestor",      color: "#60A5FA", bg: "rgba(96,165,250,0.12)"  },
  Colaborador: { label: "Colaborador", color: B.accentLight, bg: "rgba(0,200,168,0.12)" },
};

type Pagina = "upload" | "dashboard" | "risco";

function loadAuth(): AuthState | null {
  const token     = localStorage.getItem("token");
  const empresaId = localStorage.getItem("empresaId");
  const role      = localStorage.getItem("role");
  const nome      = localStorage.getItem("nome");
  if (token && empresaId && role && nome) return { token, empresaId, role, nome };
  return null;
}

export default function App() {
  const [auth, setAuth]     = useState<AuthState | null>(loadAuth);
  const [pagina, setPagina] = useState<Pagina>("upload");

  function handleLogin(state: AuthState) {
    localStorage.setItem("token",     state.token);
    localStorage.setItem("empresaId", state.empresaId);
    localStorage.setItem("role",      state.role);
    localStorage.setItem("nome",      state.nome);
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

  if (!auth) return <Login onLogin={handleLogin} />;

  const badge = ROLE_BADGE[auth.role] ?? { label: auth.role, color: B.textMuted, bg: B.card };

  const navBtn = (p: Pagina, label: string) => (
    <button
      key={p}
      onClick={() => setPagina(p)}
      style={{
        padding: "7px 16px",
        borderRadius: "6px",
        fontSize: "0.85rem",
        fontWeight: 500,
        cursor: "pointer",
        border: "none",
        transition: "background 0.2s, color 0.2s",
        background: pagina === p ? "rgba(0,176,144,0.15)" : "transparent",
        color:      pagina === p ? B.accentLight            : B.textMuted,
      }}
    >
      {label}
    </button>
  );

  return (
    <div style={{ minHeight: "100vh", backgroundColor: "#0F1A17", fontFamily: "'Inter','Segoe UI',system-ui,sans-serif" }}>

      {/* Header dark Vcorp */}
      <header style={{ backgroundColor: B.bg, borderBottom: `1px solid ${B.border}`, position: "sticky", top: 0, zIndex: 100 }}>
        <div style={{ maxWidth: "1100px", margin: "0 auto", padding: "0 24px", height: "58px", display: "flex", alignItems: "center", justifyContent: "space-between", gap: "16px" }}>

          {/* Logo */}
          <div style={{ display: "flex", alignItems: "center", gap: "10px", flexShrink: 0 }}>
            <img src={vcorpIcon} alt="Vcorp" style={{ width: "32px", filter: "drop-shadow(0 0 6px rgba(0,176,144,0.4))" }} />
            <span style={{ fontWeight: 800, fontSize: "1rem", color: B.accent, letterSpacing: "-0.3px" }}>
              Vcorp <span style={{ color: B.textMuted, fontWeight: 400 }}>RH</span>
            </span>
          </div>

          {/* Navegação central */}
          <nav style={{ display: "flex", gap: "4px", flex: 1, justifyContent: "center" }}>
            {navBtn("upload",    "Upload de Folha")}
            {navBtn("dashboard", "Anomalias")}
            {navBtn("risco",     "Dashboard de Risco")}
          </nav>

          {/* Usuário + logout */}
          <div style={{ display: "flex", alignItems: "center", gap: "12px", flexShrink: 0 }}>
            <span style={{ fontSize: "0.85rem", color: B.textPrimary, fontWeight: 500 }}>
              {auth.nome.split(" ")[0]}
            </span>
            <span style={{ fontSize: "0.72rem", fontWeight: 700, padding: "3px 10px", borderRadius: "20px", backgroundColor: badge.bg, color: badge.color, letterSpacing: "0.3px" }}>
              {badge.label}
            </span>
            <button
              onClick={handleLogout}
              style={{ padding: "6px 14px", borderRadius: "6px", border: `1px solid ${B.border}`, background: "transparent", color: B.textMuted, fontSize: "0.82rem", cursor: "pointer", transition: "color 0.2s, border-color 0.2s" }}
              onMouseEnter={(e) => { e.currentTarget.style.color = "#F87171"; e.currentTarget.style.borderColor = "rgba(248,113,113,0.3)"; }}
              onMouseLeave={(e) => { e.currentTarget.style.color = B.textMuted; e.currentTarget.style.borderColor = B.border; }}
            >
              Sair
            </button>
          </div>
        </div>
      </header>

      <main style={{ padding: "32px 0" }}>
        {pagina === "upload"    && <UploadFolhaPonto   empresaId={auth.empresaId} />}
        {pagina === "dashboard" && <DashboardAnomalias empresaId={auth.empresaId} />}
        {pagina === "risco"     && <DashboardRisco     empresaId={auth.empresaId} />}
      </main>
    </div>
  );
}

