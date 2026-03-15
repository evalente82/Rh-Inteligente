import { useState, useEffect, useRef } from "react";
import type { FormEvent, CSSProperties } from "react";
import { login } from "../api/authApi";
import vcorpLogo from "../assets/vcorpLogo_sem fundo.png";
import vcorpIcon from "../assets/Icone_fundo_Cinza.png";

// ─── Paleta oficial Vcorp Sistem ──────────────────────────────────────────────
const B = {
  accent:       "#00B090",
  accentLight:  "#00C8A8",
  accentDark:   "#007060",
  accentGlow:   "rgba(0,176,144,0.22)",
  accentGlow2:  "rgba(0,176,144,0.08)",
  bg:           "#0F1613",
  bgLeft:       "#0C1411",
  bgLeftDark:   "#08100D",
  bgLeftDeep:   "#050C09",
  card:         "#141E1A",
  border:       "#1E2E28",
  inputBg:      "#0F1613",
  inputBorder:  "#253028",
  inputFocus:   "#00B090",
  textPrimary:  "#EDF2F0",
  textMuted:    "#5E7A72",
  textSecondary:"#8EADA5",
  error:        "#F87171",
} as const;

// ─── Estilos inline (replicando exatamente o sistema original) ────────────────
const S: Record<string, CSSProperties> = {
  page:        { display:"flex", minHeight:"100vh", backgroundColor:B.bg, fontFamily:"'Inter','Segoe UI',system-ui,sans-serif" },
  left:        { flex:"1 1 45%", display:"flex", flexDirection:"column", alignItems:"center", justifyContent:"center", padding:"60px 48px", background:`linear-gradient(175deg,${B.bgLeft} 0%,${B.bgLeftDark} 55%,${B.bgLeftDeep} 100%)`, borderRight:`1px solid ${B.border}`, position:"relative", overflow:"hidden" },
  glowCircle:  { position:"absolute", top:"28%", left:"50%", transform:"translate(-50%,-50%)", width:"380px", height:"380px", borderRadius:"50%", background:`radial-gradient(circle,${B.accentGlow} 0%,transparent 68%)`, pointerEvents:"none" },
  glowCircle2: { position:"absolute", bottom:"10%", right:"-60px", width:"200px", height:"200px", borderRadius:"50%", background:`radial-gradient(circle,${B.accentGlow2} 0%,transparent 70%)`, pointerEvents:"none" },
  logoImg:     { width:"240px", marginBottom:"28px", zIndex:1, objectFit:"contain", display:"block", filter:"drop-shadow(0 0 18px rgba(0,176,144,0.35)) drop-shadow(0 2px 8px rgba(0,0,0,0.6))" },
  taglineHeading: { fontSize:"1.15rem", fontWeight:700, color:B.textPrimary, textAlign:"center", maxWidth:"300px", lineHeight:1.5, marginBottom:"8px", zIndex:1 },
  taglineAccent:  { color:B.accentLight },
  taglineSub:  { fontSize:"0.88rem", color:B.textSecondary, textAlign:"center", maxWidth:"300px", lineHeight:1.7, marginBottom:"32px", zIndex:1 },
  divider:     { width:"48px", height:"2px", background:`linear-gradient(90deg,${B.accent},transparent)`, margin:"0 auto 28px", zIndex:1 },
  featureList: { listStyle:"none", padding:0, margin:0, zIndex:1, width:"100%", maxWidth:"280px" },
  featureItem: { display:"flex", alignItems:"flex-start", gap:"12px", marginBottom:"14px" },
  featureIcon: { width:"20px", height:"20px", borderRadius:"50%", background:`linear-gradient(135deg,${B.accent},${B.accentDark})`, display:"flex", alignItems:"center", justifyContent:"center", fontSize:"10px", flexShrink:0, marginTop:"1px", color:"#fff" },
  featureText: { color:B.textSecondary, fontSize:"0.84rem", lineHeight:1.5 },
  right:       { flex:"1 1 55%", display:"flex", flexDirection:"column", alignItems:"center", justifyContent:"center", padding:"60px 48px", backgroundColor:B.bg },
  formWrap:    { width:"100%", maxWidth:"400px" },
  formEyebrow: { fontSize:"2rem", fontWeight:800, color:B.accent, letterSpacing:"-0.5px", marginBottom:"4px", lineHeight:1.2 },
  formTitle:   { fontSize:"1.1rem", fontWeight:400, color:B.textMuted, marginBottom:"4px" },
  formSubtitle:{ fontSize:"0.9rem", color:B.textMuted, marginBottom:"32px" },
  formGroup:   { marginBottom:"20px" },
  label:       { display:"block", fontSize:"0.78rem", fontWeight:600, color:B.textSecondary, marginBottom:"7px", textTransform:"uppercase", letterSpacing:"0.6px" },
  input:       { width:"100%", padding:"12px 15px", backgroundColor:B.inputBg, border:`1px solid ${B.inputBorder}`, borderRadius:"8px", color:B.textPrimary, fontSize:"0.95rem", boxSizing:"border-box", outline:"none", transition:"border-color 0.2s,box-shadow 0.2s" },
  select:      { width:"100%", padding:"12px 15px", backgroundColor:B.inputBg, border:`1px solid ${B.inputBorder}`, borderRadius:"8px", color:B.textPrimary, fontSize:"0.95rem", boxSizing:"border-box", outline:"none", cursor:"pointer" },
  button:      { width:"100%", padding:"13px", background:`linear-gradient(135deg,${B.accent} 0%,${B.accentDark} 100%)`, border:"none", borderRadius:"8px", color:"#fff", fontWeight:700, fontSize:"0.95rem", cursor:"pointer", marginTop:"8px", letterSpacing:"0.3px", boxShadow:`0 4px 24px ${B.accentGlow}`, transition:"opacity 0.2s,transform 0.1s" },
  errorBox:    { backgroundColor:"rgba(248,113,113,0.08)", border:"1px solid rgba(248,113,113,0.25)", borderRadius:"8px", color:B.error, fontSize:"0.85rem", padding:"11px 15px", marginBottom:"18px", display:"flex", alignItems:"center", gap:"8px" },
  footer:      { marginTop:"44px", textAlign:"center", fontSize:"0.76rem", color:B.textMuted, borderTop:`1px solid ${B.border}`, paddingTop:"20px", display:"flex", flexDirection:"column", alignItems:"center", gap:"8px" },
  demoSection: { marginTop:"28px", borderTop:`1px solid ${B.border}`, paddingTop:"20px" },
  demoTitle:   { fontSize:"0.72rem", fontWeight:600, color:B.textMuted, textTransform:"uppercase" as const, letterSpacing:"0.7px", marginBottom:"12px" },
  demoGrid:    { display:"grid", gridTemplateColumns:"1fr 1fr", gap:"8px" },
  demoCard:    { backgroundColor:B.card, border:`1px solid ${B.border}`, borderRadius:"8px", padding:"10px 12px", cursor:"pointer", transition:"border-color 0.2s" },
  demoCardLabel:{ fontSize:"0.7rem", fontWeight:700, color:B.accent, textTransform:"uppercase" as const, letterSpacing:"0.5px", marginBottom:"4px" },
  demoCardEmail:{ fontSize:"0.78rem", color:B.textPrimary, fontFamily:"monospace", whiteSpace:"nowrap" as const, overflow:"hidden", textOverflow:"ellipsis" },
  demoCardPass: { fontSize:"0.75rem", color:B.textSecondary, fontFamily:"monospace" },
};

// ─── Empresas disponíveis (seed determinístico) ───────────────────────────────
const EMPRESAS = [
  { id: "a1b2c3d4-0000-0000-0000-000000000001", label: "Vcorp Tecnologia Ltda" },
  { id: "b2c3d4e5-0000-0000-0000-000000000002", label: "Inova Serviços S/A" },
];

const FEATURES = [
  "Auditoria autônoma de ponto com IA e RAG",
  "Gestão completa de funcionários e admissões",
  "Leitura de CCT via PDF e vetorização semântica",
  "Fechamento de folha com análise de anomalias",
  "Holerite narrativo gerado por Gemini 2.5 Flash",
  "Dashboards de risco em tempo real",
  "Multi-tenant — isolamento total por empresa",
];

const DEMO_CREDS = [
  { empresa: "a1b2c3d4-0000-0000-0000-000000000001", label: "Vcorp · Dono",        email: "dono@vcorp.com",    senha: "Vcorp@2026!" },
  { empresa: "a1b2c3d4-0000-0000-0000-000000000001", label: "Vcorp · Gestor",       email: "gestor@vcorp.com",  senha: "Gestor@2026!" },
  { empresa: "a1b2c3d4-0000-0000-0000-000000000001", label: "Vcorp · Colaborador",  email: "joao@vcorp.com",    senha: "Colab@2026!" },
  { empresa: "b2c3d4e5-0000-0000-0000-000000000002", label: "Inova · Dono",         email: "dono@inova.com",    senha: "Inova@2026!" },
  { empresa: "b2c3d4e5-0000-0000-0000-000000000002", label: "Inova · Gestor",       email: "gestor@inova.com",  senha: "Gestor@2026!" },
  { empresa: "b2c3d4e5-0000-0000-0000-000000000002", label: "Inova · Colaborador",  email: "maria@inova.com",   senha: "Colab@2026!" },
];

// ─── Interfaces ───────────────────────────────────────────────────────────────
export interface AuthState {
  token: string;
  empresaId: string;
  role: string;
  nome: string;
}

interface LoginProps {
  onLogin: (state: AuthState) => void;
}

// ─── Componente principal ─────────────────────────────────────────────────────
export default function Login({ onLogin }: LoginProps) {
  const [empresaId, setEmpresaId]     = useState(EMPRESAS[0].id);
  const [email, setEmail]             = useState("");
  const [senha, setSenha]             = useState("");
  const [erro, setErro]               = useState<string | null>(null);
  const [carregando, setCarregando]   = useState(false);
  const emailRef = useRef<HTMLInputElement>(null);
  const senhaRef = useRef<HTMLInputElement>(null);

  // Foco no campo de e-mail ao montar
  useEffect(() => { emailRef.current?.focus(); }, []);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setErro(null);
    setCarregando(true);
    try {
      const resp = await login(empresaId, { email, senha });
      onLogin({ token: resp.accessToken, empresaId, role: resp.role, nome: resp.nomeCompleto });
    } catch (err: unknown) {
      setErro(err instanceof Error ? err.message : "E-mail ou senha incorretos. Tente novamente.");
    } finally {
      setCarregando(false);
    }
  }

  /** Preenche o formulário ao clicar num card de demo */
  function preencherDemo(cred: typeof DEMO_CREDS[0]) {
    setEmpresaId(cred.empresa);
    setEmail(cred.email);
    setSenha(cred.senha);
    setErro(null);
    senhaRef.current?.focus();
  }

  function focusInput(el: HTMLInputElement | HTMLSelectElement | null) {
    if (!el) return;
    el.style.borderColor = B.inputFocus;
    el.style.boxShadow   = `0 0 0 3px ${B.accentGlow2}`;
  }
  function blurInput(el: HTMLInputElement | HTMLSelectElement | null) {
    if (!el) return;
    el.style.borderColor = B.inputBorder;
    el.style.boxShadow   = "none";
  }

  return (
    <div style={S.page}>

      {/* ── Coluna esquerda: identidade Vcorp RH ────────────────────────── */}
      <div style={S.left}>
        <div style={S.glowCircle} />
        <div style={S.glowCircle2} />

        <img src={vcorpLogo} alt="Vcorp Sistem" style={S.logoImg} />

        <h2 style={S.taglineHeading}>
          Gestão de RH com{" "}
          <span style={S.taglineAccent}>inteligência artificial</span>{" "}
          de verdade.
        </h2>

        <p style={S.taglineSub}>
          Do upload do ponto até o fechamento da folha — a IA audita,
          detecta anomalias e gera holerites narrativos de forma autônoma.
        </p>

        <div style={S.divider} />

        <ul style={S.featureList}>
          {FEATURES.map((f) => (
            <li key={f} style={S.featureItem}>
              <div style={S.featureIcon}>✓</div>
              <span style={S.featureText}>{f}</span>
            </li>
          ))}
        </ul>
      </div>

      {/* ── Coluna direita: formulário ───────────────────────────────────── */}
      <div style={S.right}>
        <div style={S.formWrap}>
          <h1 style={S.formEyebrow}>Vcorp RH Inteligente</h1>
          <p style={S.formTitle}>Bem-vindo de volta</p>
          <p style={S.formSubtitle}>Acesse sua conta para continuar</p>

          {/* Erro de autenticação */}
          {erro && (
            <div style={S.errorBox}>
              <span>⚠</span> {erro}
            </div>
          )}

          <form onSubmit={handleSubmit}>
            {/* Empresa */}
            <div style={S.formGroup}>
              <label style={S.label}>Empresa</label>
              <select
                value={empresaId}
                onChange={(e) => setEmpresaId(e.target.value)}
                style={S.select}
                onFocus={(e) => focusInput(e.target)}
                onBlur={(e)  => blurInput(e.target)}
              >
                {EMPRESAS.map((emp) => (
                  <option key={emp.id} value={emp.id} style={{ backgroundColor: B.card }}>
                    {emp.label}
                  </option>
                ))}
              </select>
            </div>

            {/* E-mail */}
            <div style={S.formGroup}>
              <label style={S.label}>E-mail</label>
              <input
                ref={emailRef}
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                style={S.input}
                placeholder="seu@email.com"
                autoComplete="username"
                required
                onFocus={(e) => focusInput(e.target)}
                onBlur={(e)  => blurInput(e.target)}
              />
            </div>

            {/* Senha */}
            <div style={S.formGroup}>
              <label style={S.label}>Senha</label>
              <input
                ref={senhaRef}
                type="password"
                value={senha}
                onChange={(e) => setSenha(e.target.value)}
                style={S.input}
                placeholder="••••••••"
                autoComplete="current-password"
                required
                onFocus={(e) => focusInput(e.target)}
                onBlur={(e)  => blurInput(e.target)}
              />
            </div>

            <button
              type="submit"
              style={{ ...S.button, opacity: carregando ? 0.72 : 1 }}
              disabled={carregando}
              onMouseEnter={(e) => { if (!carregando) e.currentTarget.style.opacity = "0.88"; }}
              onMouseLeave={(e) => { if (!carregando) e.currentTarget.style.opacity = "1"; }}
            >
              {carregando ? "Autenticando…" : "Entrar na plataforma"}
            </button>
          </form>

          {/* ── Credenciais de demonstração ─────────────────────────────── */}
          <div style={S.demoSection}>
            <p style={S.demoTitle}>Credenciais de demonstração — clique para preencher</p>
            <div style={S.demoGrid}>
              {DEMO_CREDS.map((c) => (
                <button
                  key={c.email}
                  type="button"
                  onClick={() => preencherDemo(c)}
                  style={S.demoCard}
                  onMouseEnter={(e) => (e.currentTarget.style.borderColor = B.accent)}
                  onMouseLeave={(e) => (e.currentTarget.style.borderColor = B.border)}
                >
                  <div style={S.demoCardLabel}>{c.label}</div>
                  <div style={S.demoCardEmail}>{c.email}</div>
                  <div style={S.demoCardPass}>{c.senha}</div>
                </button>
              ))}
            </div>
          </div>

          {/* Footer */}
          <div style={S.footer}>
            <img
              src={vcorpIcon}
              alt="Vcorp"
              style={{ width: "52px", display: "block", filter: "drop-shadow(0 0 6px rgba(0,176,144,0.3))", opacity: 0.85 }}
            />
            <span>© {new Date().getFullYear()} Vcorp Sistem · Todos os direitos reservados</span>
          </div>
        </div>
      </div>
    </div>
  );
}
