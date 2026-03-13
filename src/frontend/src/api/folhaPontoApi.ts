// ─── Tipos espelhando os DTOs da API ─────────────────────────────────────────

export interface OperacaoAceitaOutputDTO {
  processoId: string;
  mensagem: string;
}

export interface AlertaAnomaliaOutputDTO {
  id: string;
  funcionarioId: string;
  tipoAnomalia: string;
  dataReferencia: string;
  descricao: string;
  gravidade: number;
  geradoEm: string;
  resolvido: boolean;
  resolvidoEm: string | null;
}

// ─── Cliente HTTP centralizado ────────────────────────────────────────────────

const BASE_URL = '/api/folhaponto';

export async function uploadResumoFolha(
  empresaId: string,
  arquivo: File,
  periodoInicio: string,
  periodoFim: string
): Promise<OperacaoAceitaOutputDTO> {
  const formData = new FormData();
  formData.append('arquivo', arquivo);

  const params = new URLSearchParams({ periodoInicio, periodoFim });
  const res = await fetch(
    `${BASE_URL}/${empresaId}/upload?${params}`,
    { method: 'POST', body: formData }
  );

  if (!res.ok) {
    const problem = await res.json().catch(() => ({}));
    throw new Error(problem?.detail ?? `Erro ${res.status}`);
  }

  return res.json();
}

export async function buscarAnomalias(
  empresaId: string,
  funcionarioId: string,
  periodoInicio: string,
  periodoFim: string
): Promise<AlertaAnomaliaOutputDTO[]> {
  const params = new URLSearchParams({ periodoInicio, periodoFim });
  const res = await fetch(
    `${BASE_URL}/${empresaId}/funcionarios/${funcionarioId}/anomalias?${params}`
  );

  if (!res.ok) {
    const problem = await res.json().catch(() => ({}));
    throw new Error(problem?.detail ?? `Erro ${res.status}`);
  }

  return res.json();
}
