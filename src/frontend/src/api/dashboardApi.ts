// Cliente HTTP tipado para os endpoints do DashboardController

export interface AnomaliasPorTipoDTO {
  tipoAnomalia: string;
  total: number;
  totalCriticos: number;
  totalAtencao: number;
  totalInformativos: number;
}

export interface FuncionarioRiscoDTO {
  funcionarioId: string;
  totalCriticos: number;
  totalAlertas: number;
}

export interface DashboardRiscoOutputDTO {
  empresaId: string;
  periodoInicio: string;
  periodoFim: string;
  totalAlertas: number;
  totalCriticos: number;
  totalAtencao: number;
  totalInformativos: number;
  totalResolvidos: number;
  anomaliasPorTipo: AnomaliasPorTipoDTO[];
  topFuncionariosRisco: FuncionarioRiscoDTO[];
}

export interface IndiceConformidadeOutputDTO {
  empresaId: string;
  periodoInicio: string;
  periodoFim: string;
  indiceConformidade: number;
  classificacao: "Verde" | "Amarelo" | "Vermelho";
  totalFuncionariosAtivos: number;
  funcionariosComAlertas: number;
  funcionariosSemAlertas: number;
}

async function get<T>(url: string): Promise<T> {
  const res = await fetch(url);
  if (!res.ok) {
    const prob = await res.json().catch(() => ({ title: "Erro desconhecido" }));
    throw new Error(prob.title ?? `HTTP ${res.status}`);
  }
  return res.json() as Promise<T>;
}

export function buscarDashboardRisco(
  empresaId: string,
  periodoInicio: string,
  periodoFim: string
): Promise<DashboardRiscoOutputDTO> {
  const params = new URLSearchParams({ periodoInicio, periodoFim });
  return get<DashboardRiscoOutputDTO>(
    `/api/${empresaId}/dashboard/risco?${params}`
  );
}

export function buscarIndiceConformidade(
  empresaId: string,
  periodoInicio: string,
  periodoFim: string
): Promise<IndiceConformidadeOutputDTO> {
  const params = new URLSearchParams({ periodoInicio, periodoFim });
  return get<IndiceConformidadeOutputDTO>(
    `/api/${empresaId}/dashboard/conformidade?${params}`
  );
}
