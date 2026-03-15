// ─── Tipos ───────────────────────────────────────────────────────────────────

export interface LoginRequest {
  email: string;
  senha: string;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiracao: string;
  role: string;
  nomeCompleto: string;
}

// ─── Chamada à API ────────────────────────────────────────────────────────────

/**
 * Autentica o usuário via POST /api/{empresaId}/auth/login.
 * O empresaId é informado pelo usuário no formulário de login.
 */
export async function login(
  empresaId: string,
  data: LoginRequest
): Promise<TokenResponse> {
  const res = await fetch(`/api/${empresaId}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });

  if (!res.ok) {
    const problem = await res.json().catch(() => ({}));
    throw new Error(problem.title ?? "Credenciais inválidas");
  }

  return res.json() as Promise<TokenResponse>;
}
