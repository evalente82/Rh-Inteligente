import { useState } from "react";
import type { FormEvent } from "react";
import { login } from "../api/authApi";

// ─── Empresas disponíveis (seed determinístico) ───────────────────────────────
const EMPRESAS = [
  { id: "a1b2c3d4-0000-0000-0000-000000000001", label: "Vcorp Tecnologia Ltda" },
  { id: "b2c3d4e5-0000-0000-0000-000000000002", label: "Inova Serviços S/A" },
];

export interface AuthState {
  token: string;
  empresaId: string;
  role: string;
  nome: string;
}

interface LoginProps {
  onLogin: (state: AuthState) => void;
}

export default function Login({ onLogin }: LoginProps) {
  const [empresaId, setEmpresaId] = useState(EMPRESAS[0].id);
  const [email, setEmail]         = useState("");
  const [senha, setSenha]         = useState("");
  const [erro, setErro]           = useState<string | null>(null);
  const [carregando, setCarregando] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setErro(null);
    setCarregando(true);
    try {
      const resp = await login(empresaId, { email, senha });
      onLogin({ token: resp.accessToken, empresaId, role: resp.role, nome: resp.nomeCompleto });
    } catch (err: unknown) {
      setErro(err instanceof Error ? err.message : "Erro ao autenticar");
    } finally {
      setCarregando(false);
    }
  }

  return (
    <div className="min-h-screen bg-linear-to-br from-indigo-50 to-slate-100 flex items-center justify-center px-4">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-8">

        {/* Logo / título */}
        <div className="text-center mb-8">
          <span className="text-3xl font-bold text-indigo-700 tracking-tight">
            Vcorp Folha <span className="text-gray-400 font-normal text-lg">IA</span>
          </span>
          <p className="mt-2 text-sm text-gray-500">Gestão Inteligente de RH</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-5">

          {/* Seletor de empresa */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Empresa
            </label>
            <select
              value={empresaId}
              onChange={(e) => setEmpresaId(e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm
                         focus:outline-none focus:ring-2 focus:ring-indigo-400 bg-white"
            >
              {EMPRESAS.map((e) => (
                <option key={e.id} value={e.id}>
                  {e.label}
                </option>
              ))}
            </select>
          </div>

          {/* E-mail */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              E-mail
            </label>
            <input
              type="email"
              autoComplete="username"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="seu@email.com"
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm
                         focus:outline-none focus:ring-2 focus:ring-indigo-400"
            />
          </div>

          {/* Senha */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Senha
            </label>
            <input
              type="password"
              autoComplete="current-password"
              required
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
              placeholder="••••••••"
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm
                         focus:outline-none focus:ring-2 focus:ring-indigo-400"
            />
          </div>

          {/* Mensagem de erro */}
          {erro && (
            <div className="bg-red-50 border border-red-200 rounded-lg px-4 py-3 text-sm text-red-700">
              {erro}
            </div>
          )}

          {/* Botão */}
          <button
            type="submit"
            disabled={carregando}
            className="w-full bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-400
                       text-white font-semibold rounded-lg py-2.5 text-sm transition-colors"
          >
            {carregando ? "Autenticando..." : "Entrar"}
          </button>
        </form>

        {/* Credenciais de demonstração */}
        <div className="mt-8 border-t pt-6">
          <p className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-3">
            Credenciais de demonstração
          </p>
          <div className="space-y-3">
            <CredCard
              empresa="Vcorp Tecnologia"
              role="Dono"
              email="dono@vcorp.com"
              senha="Vcorp@2026!"
              color="indigo"
            />
            <CredCard
              empresa="Vcorp Tecnologia"
              role="Gestor"
              email="gestor@vcorp.com"
              senha="Gestor@2026!"
              color="teal"
            />
            <CredCard
              empresa="Vcorp Tecnologia"
              role="Colaborador"
              email="joao@vcorp.com"
              senha="Colab@2026!"
              color="orange"
            />
            <CredCard
              empresa="Inova Serviços"
              role="Dono"
              email="dono@inova.com"
              senha="Inova@2026!"
              color="purple"
            />
          </div>
        </div>
      </div>
    </div>
  );
}

// ─── Componente auxiliar — card de credencial ─────────────────────────────────
interface CredCardProps {
  empresa: string;
  role: string;
  email: string;
  senha: string;
  color: "indigo" | "teal" | "orange" | "purple";
}

const colorMap: Record<string, string> = {
  indigo: "bg-indigo-50 border-indigo-200 text-indigo-700",
  teal:   "bg-teal-50 border-teal-200 text-teal-700",
  orange: "bg-orange-50 border-orange-200 text-orange-700",
  purple: "bg-purple-50 border-purple-200 text-purple-700",
};

function CredCard({ empresa, role, email, senha, color }: CredCardProps) {
  return (
    <div className={`border rounded-lg px-3 py-2 text-xs ${colorMap[color]}`}>
      <div className="flex justify-between items-center">
        <span className="font-semibold">{empresa} — {role}</span>
      </div>
      <div className="mt-1 font-mono">{email}</div>
      <div className="font-mono text-gray-500">{senha}</div>
    </div>
  );
}
