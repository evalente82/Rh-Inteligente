import { useState } from 'react';
import { buscarAnomalias } from '../api/folhaPontoApi';
import type { AlertaAnomaliaOutputDTO } from '../api/folhaPontoApi';

// ─── Badge de gravidade ───────────────────────────────────────────────────────

const GRAVIDADE_CONFIG: Record<number, { label: string; className: string }> = {
  1: { label: 'Informativo', className: 'bg-blue-100 text-blue-700' },
  2: { label: 'Atenção',    className: 'bg-yellow-100 text-yellow-700' },
  3: { label: 'Crítico',    className: 'bg-red-100 text-red-700' },
};

function BadgeGravidade({ gravidade }: { gravidade: number }) {
  const config = GRAVIDADE_CONFIG[gravidade] ?? { label: String(gravidade), className: 'bg-gray-100 text-gray-600' };
  return (
    <span className={`inline-block px-2 py-0.5 rounded-full text-xs font-semibold ${config.className}`}>
      {config.label}
    </span>
  );
}

// ─── Página principal ─────────────────────────────────────────────────────────

interface Props {
  empresaId: string;
}

export default function DashboardAnomalias({ empresaId }: Props) {
  const [funcionarioId, setFuncionarioId] = useState('');
  const [periodoInicio, setPeriodoInicio] = useState('');
  const [periodoFim, setPeriodoFim] = useState('');
  const [alertas, setAlertas] = useState<AlertaAnomaliaOutputDTO[]>([]);
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const [buscou, setBuscou] = useState(false);

  async function handleBuscar(e: React.FormEvent) {
    e.preventDefault();
    setCarregando(true);
    setErro(null);

    try {
      const dados = await buscarAnomalias(empresaId, funcionarioId, periodoInicio, periodoFim);
      setAlertas(dados);
      setBuscou(true);
    } catch (err: unknown) {
      setErro(err instanceof Error ? err.message : 'Erro desconhecido');
    } finally {
      setCarregando(false);
    }
  }

  const totalCriticos = alertas.filter((a) => a.gravidade === 3).length;
  const pendentes = alertas.filter((a) => !a.resolvido).length;

  return (
    <div className="max-w-5xl mx-auto p-6">
      <h1 className="text-2xl font-bold text-gray-800 mb-1">Dashboard de Anomalias</h1>
      <p className="text-sm text-gray-500 mb-6">
        Anomalias detectadas pela IA após auditoria dos registros de ponto.
      </p>

      {/* Filtros */}
      <form onSubmit={handleBuscar} className="flex flex-wrap gap-3 items-end mb-6">
        <div>
          <label className="block text-xs font-medium text-gray-600 mb-1">ID do Funcionário</label>
          <input
            type="text"
            placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
            value={funcionarioId}
            onChange={(e) => setFuncionarioId(e.target.value)}
            required
            className="border border-gray-300 rounded-md px-3 py-2 text-sm w-72 focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
        </div>
        <div>
          <label className="block text-xs font-medium text-gray-600 mb-1">Início</label>
          <input
            type="date"
            value={periodoInicio}
            onChange={(e) => setPeriodoInicio(e.target.value)}
            required
            className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
        </div>
        <div>
          <label className="block text-xs font-medium text-gray-600 mb-1">Fim</label>
          <input
            type="date"
            value={periodoFim}
            onChange={(e) => setPeriodoFim(e.target.value)}
            required
            className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
        </div>
        <button
          type="submit"
          disabled={carregando}
          className="bg-indigo-600 hover:bg-indigo-700 disabled:bg-gray-300 text-white font-semibold
            px-5 py-2 rounded-md text-sm transition-colors"
        >
          {carregando ? 'Buscando...' : 'Analisar'}
        </button>
      </form>

      {/* Erro */}
      {erro && (
        <div className="rounded-md bg-red-50 border border-red-200 p-4 mb-4">
          <p className="text-red-700 text-sm">✗ {erro}</p>
        </div>
      )}

      {/* Resumo */}
      {buscou && !erro && (
        <div className="grid grid-cols-3 gap-4 mb-6">
          <div className="rounded-lg border border-gray-200 p-4">
            <p className="text-xs text-gray-500">Total de anomalias</p>
            <p className="text-3xl font-bold text-gray-800">{alertas.length}</p>
          </div>
          <div className="rounded-lg border border-red-100 bg-red-50 p-4">
            <p className="text-xs text-red-500">Críticos</p>
            <p className="text-3xl font-bold text-red-700">{totalCriticos}</p>
          </div>
          <div className="rounded-lg border border-yellow-100 bg-yellow-50 p-4">
            <p className="text-xs text-yellow-600">Pendentes</p>
            <p className="text-3xl font-bold text-yellow-700">{pendentes}</p>
          </div>
        </div>
      )}

      {/* Tabela */}
      {buscou && alertas.length > 0 && (
        <div className="overflow-x-auto rounded-lg border border-gray-200">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-xs text-gray-500 uppercase tracking-wide">
              <tr>
                <th className="px-4 py-3 text-left">Data Ref.</th>
                <th className="px-4 py-3 text-left">Tipo</th>
                <th className="px-4 py-3 text-left">Descrição</th>
                <th className="px-4 py-3 text-left">Gravidade</th>
                <th className="px-4 py-3 text-left">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {alertas.map((alerta) => (
                <tr key={alerta.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-4 py-3 font-mono text-gray-700">{alerta.dataReferencia}</td>
                  <td className="px-4 py-3 text-gray-600">{alerta.tipoAnomalia}</td>
                  <td className="px-4 py-3 text-gray-700 max-w-xs truncate" title={alerta.descricao}>
                    {alerta.descricao}
                  </td>
                  <td className="px-4 py-3">
                    <BadgeGravidade gravidade={alerta.gravidade} />
                  </td>
                  <td className="px-4 py-3">
                    {alerta.resolvido ? (
                      <span className="text-green-600 text-xs font-medium">✓ Resolvido</span>
                    ) : (
                      <span className="text-orange-500 text-xs font-medium">● Pendente</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {buscou && alertas.length === 0 && !erro && (
        <div className="text-center py-12 text-gray-400">
          <p className="text-lg">✓ Nenhuma anomalia encontrada no período.</p>
        </div>
      )}
    </div>
  );
}
