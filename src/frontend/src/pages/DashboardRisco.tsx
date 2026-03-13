import { useEffect, useState } from "react";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
} from "recharts";
import {
  buscarDashboardRisco,
  buscarIndiceConformidade,
  type DashboardRiscoOutputDTO,
  type IndiceConformidadeOutputDTO,
} from "../api/dashboardApi";

interface Props {
  empresaId: string;
}

// ─── helpers ────────────────────────────────────────────────────────────────

const hoje = new Date();
const primeiroDiaMes = `${hoje.getFullYear()}-${String(hoje.getMonth() + 1).padStart(2, "0")}-01`;
const ultimoDiaMes = new Date(hoje.getFullYear(), hoje.getMonth() + 1, 0)
  .toISOString()
  .slice(0, 10);

const COR_CRITICO = "#ef4444";
const COR_ATENCAO = "#f59e0b";
const COR_INFO = "#6366f1";

const classesBadgeIndice: Record<string, string> = {
  Verde: "bg-green-100 text-green-700 ring-1 ring-green-300",
  Amarelo: "bg-yellow-100 text-yellow-700 ring-1 ring-yellow-300",
  Vermelho: "bg-red-100 text-red-700 ring-1 ring-red-300",
};

// ─── sub-componentes ─────────────────────────────────────────────────────────

function CardResumo({
  label,
  valor,
  cor,
}: {
  label: string;
  valor: number;
  cor: string;
}) {
  return (
    <div className="rounded-xl bg-white border border-gray-200 shadow-sm p-5 flex flex-col gap-1">
      <span className="text-xs font-medium text-gray-400 uppercase tracking-wide">
        {label}
      </span>
      <span className={`text-3xl font-bold ${cor}`}>{valor}</span>
    </div>
  );
}

function GaugeIndice({ indice, classificacao }: { indice: number; classificacao: string }) {
  const cor =
    classificacao === "Verde"
      ? "#22c55e"
      : classificacao === "Amarelo"
      ? "#f59e0b"
      : "#ef4444";

  const raio = 60;
  const circunferencia = Math.PI * raio; // semicírculo — mantida para referência futura
  void circunferencia;

  return (
    <div className="flex flex-col items-center gap-2">
      <svg width="160" height="90" viewBox="0 0 160 90">
        {/* trilha cinza */}
        <path
          d="M 10 80 A 70 70 0 0 1 150 80"
          fill="none"
          stroke="#e5e7eb"
          strokeWidth="14"
          strokeLinecap="round"
        />
        {/* arco colorido */}
        <path
          d="M 10 80 A 70 70 0 0 1 150 80"
          fill="none"
          stroke={cor}
          strokeWidth="14"
          strokeLinecap="round"
          strokeDasharray={`${(indice / 100) * 220} 220`}
        />
        <text x="80" y="76" textAnchor="middle" fontSize="22" fontWeight="bold" fill={cor}>
          {indice.toFixed(1)}%
        </text>
      </svg>
      <span
        className={`text-xs font-semibold px-3 py-1 rounded-full ${classesBadgeIndice[classificacao] ?? ""}`}
      >
        {classificacao}
      </span>
    </div>
  );
}

// ─── página principal ─────────────────────────────────────────────────────────

export default function DashboardRisco({ empresaId }: Props) {
  const [inicio, setInicio] = useState(primeiroDiaMes);
  const [fim, setFim] = useState(ultimoDiaMes);

  const [dashboard, setDashboard] = useState<DashboardRiscoOutputDTO | null>(null);
  const [conformidade, setConformidade] = useState<IndiceConformidadeOutputDTO | null>(null);
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  async function carregar() {
    setCarregando(true);
    setErro(null);
    try {
      const [d, c] = await Promise.all([
        buscarDashboardRisco(empresaId, inicio, fim),
        buscarIndiceConformidade(empresaId, inicio, fim),
      ]);
      setDashboard(d);
      setConformidade(c);
    } catch (e) {
      setErro(e instanceof Error ? e.message : "Erro ao carregar dashboard.");
    } finally {
      setCarregando(false);
    }
  }

  useEffect(() => {
    carregar();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <div className="max-w-5xl mx-auto px-6 space-y-6">
      {/* ── filtro de período ─────────────────────────────────── */}
      <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-5 flex flex-wrap items-end gap-4">
        <div className="flex flex-col gap-1">
          <label className="text-xs font-medium text-gray-500">Período início</label>
          <input
            type="date"
            value={inicio}
            onChange={(e) => setInicio(e.target.value)}
            className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
          />
        </div>
        <div className="flex flex-col gap-1">
          <label className="text-xs font-medium text-gray-500">Período fim</label>
          <input
            type="date"
            value={fim}
            onChange={(e) => setFim(e.target.value)}
            className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
          />
        </div>
        <button
          onClick={carregar}
          disabled={carregando}
          className="px-5 py-2 bg-indigo-600 text-white text-sm font-medium rounded-md hover:bg-indigo-700 disabled:opacity-50 transition-colors"
        >
          {carregando ? "Carregando..." : "Atualizar"}
        </button>
      </div>

      {erro && (
        <div className="rounded-md bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">
          {erro}
        </div>
      )}

      {dashboard && conformidade && (
        <>
          {/* ── cards de resumo ───────────────────────────────── */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
            <CardResumo label="Total de Alertas" valor={dashboard.totalAlertas} cor="text-gray-800" />
            <CardResumo label="Críticos" valor={dashboard.totalCriticos} cor="text-red-600" />
            <CardResumo label="Atenção" valor={dashboard.totalAtencao} cor="text-yellow-600" />
            <CardResumo label="Resolvidos" valor={dashboard.totalResolvidos} cor="text-green-600" />
          </div>

          {/* ── gráfico + gauge ───────────────────────────────── */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {/* gráfico de barras por tipo */}
            <div className="md:col-span-2 bg-white border border-gray-200 rounded-xl shadow-sm p-5">
              <h2 className="text-sm font-semibold text-gray-700 mb-4">
                Anomalias por Tipo
              </h2>
              {dashboard.anomaliasPorTipo.length === 0 ? (
                <p className="text-sm text-gray-400 text-center py-8">
                  Nenhuma anomalia no período.
                </p>
              ) : (
                <ResponsiveContainer width="100%" height={220}>
                  <BarChart
                    data={dashboard.anomaliasPorTipo}
                    margin={{ top: 4, right: 8, left: -20, bottom: 40 }}
                  >
                    <XAxis
                      dataKey="tipoAnomalia"
                      tick={{ fontSize: 10 }}
                      angle={-30}
                      textAnchor="end"
                      interval={0}
                    />
                    <YAxis tick={{ fontSize: 11 }} allowDecimals={false} />
                    <Tooltip
                      formatter={(value, name) => [value, name]}
                      contentStyle={{ fontSize: 12 }}
                    />
                    <Bar dataKey="totalCriticos" name="Críticos" stackId="a" fill={COR_CRITICO} radius={[0, 0, 0, 0]} />
                    <Bar dataKey="totalAtencao" name="Atenção" stackId="a" fill={COR_ATENCAO} radius={[0, 0, 0, 0]} />
                    <Bar dataKey="totalInformativos" name="Informativos" stackId="a" fill={COR_INFO} radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              )}
            </div>

            {/* gauge de conformidade */}
            <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-5 flex flex-col items-center justify-center gap-3">
              <h2 className="text-sm font-semibold text-gray-700">
                Índice de Conformidade
              </h2>
              <GaugeIndice
                indice={conformidade.indiceConformidade}
                classificacao={conformidade.classificacao}
              />
              <div className="w-full space-y-1 text-xs text-gray-500">
                <div className="flex justify-between">
                  <span>Funcionários ativos</span>
                  <span className="font-medium text-gray-700">{conformidade.totalFuncionariosAtivos}</span>
                </div>
                <div className="flex justify-between">
                  <span>Com alertas</span>
                  <span className="font-medium text-red-600">{conformidade.funcionariosComAlertas}</span>
                </div>
                <div className="flex justify-between">
                  <span>Sem alertas</span>
                  <span className="font-medium text-green-600">{conformidade.funcionariosSemAlertas}</span>
                </div>
              </div>
            </div>
          </div>

          {/* ── top funcionários em risco ─────────────────────── */}
          {dashboard.topFuncionariosRisco.length > 0 && (
            <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-5">
              <h2 className="text-sm font-semibold text-gray-700 mb-3">
                Top {dashboard.topFuncionariosRisco.length} Funcionários em Risco
              </h2>
              <div className="overflow-x-auto">
                <table className="w-full text-sm text-left">
                  <thead>
                    <tr className="border-b border-gray-100 text-xs text-gray-400 uppercase tracking-wide">
                      <th className="pb-2 pr-4">#</th>
                      <th className="pb-2 pr-4">ID do Funcionário</th>
                      <th className="pb-2 pr-4 text-center text-red-500">Críticos</th>
                      <th className="pb-2 text-center text-gray-500">Total</th>
                    </tr>
                  </thead>
                  <tbody>
                    {dashboard.topFuncionariosRisco.map((f, i) => (
                      <tr key={f.funcionarioId} className="border-b border-gray-50 hover:bg-gray-50">
                        <td className="py-2 pr-4 text-gray-400">{i + 1}</td>
                        <td className="py-2 pr-4 font-mono text-xs text-gray-600">
                          {f.funcionarioId.slice(0, 8)}…
                        </td>
                        <td className="py-2 pr-4 text-center">
                          <span className="inline-block bg-red-100 text-red-700 text-xs font-semibold px-2 py-0.5 rounded-full">
                            {f.totalCriticos}
                          </span>
                        </td>
                        <td className="py-2 text-center text-gray-700 font-medium">
                          {f.totalAlertas}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </>
      )}

      {/* estado vazio */}
      {!carregando && !erro && dashboard && dashboard.totalAlertas === 0 && (
        <div className="text-center py-16 text-gray-400 text-sm">
          ✅ Nenhuma anomalia encontrada no período selecionado.
        </div>
      )}
    </div>
  );
}
