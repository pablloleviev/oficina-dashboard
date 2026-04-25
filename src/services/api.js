const BASE_URL = import.meta.env.VITE_API_URL || "https://autoflow-api-p4tv.onrender.com/api";
const OS_URL = `${BASE_URL}/OrdemServico`;

// ===============================
// AUTH
// ===============================
export async function login(email, senha) {
  const res = await fetchWithTimeout(`${BASE_URL}/Auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, senha }),
  });

  if (!res.ok) {
    throw new Error("Email ou senha inválidos");
  }

  const data = await res.json();
  const token = data?.token ?? data?.data?.token;

  if (!token) throw new Error("Email ou senha inválidos");

  return token;
}

// ===============================
// HEADERS
// ===============================
function getHeaders() {
  const token = localStorage.getItem("token");

  return {
    "Content-Type": "application/json",
    ...(token && { Authorization: `Bearer ${token}` }),
  };
}

// ===============================
// FETCH COM TIMEOUT
// ===============================
async function fetchWithTimeout(url, options = {}, timeout = 8000) {
  const controller = new AbortController();
  const id = setTimeout(() => controller.abort(), timeout);

  try {
    const res = await fetch(url, {
      ...options,
      signal: controller.signal,
    });

    return res;
  } catch (err) {
    if (err.name === "AbortError") {
      throw new Error("Servidor demorou para responder");
    }

    throw new Error("Erro de conexão com o servidor");
  } finally {
    clearTimeout(id);
  }
}

// ===============================
// RESPONSE HANDLER
// ===============================
async function handleResponse(res) {
  if (res.status === 401) {
    localStorage.removeItem("token");
    window.location.href = "/";
    throw new Error("Sessão expirada");
  }

  // 204 No Content or empty body — valid success with no payload
  const hasBody = res.status !== 204 && res.headers.get("content-length") !== "0";
  const isJson  = (res.headers.get("content-type") ?? "").includes("application/json");

  let data = null;
  if (hasBody && isJson) {
    try {
      data = await res.json();
    } catch {
      // Body present but unparseable — only throw if the request failed
      if (!res.ok) throw new Error("Resposta inválida do servidor");
    }
  } else if (hasBody && !isJson && !res.ok) {
    // Non-JSON error body (HTML error page, plain text, etc.)
    const text = await res.text().catch(() => "");
    console.error("API non-JSON error:", res.status, text.slice(0, 200));
    throw new Error(`Erro ${res.status} do servidor`);
  }

  // ApiResponse<T>: success=false é erro de negócio mesmo em HTTP 200
  if (data?.success === false) {
    const msg = Array.isArray(data.errors) && data.errors.length > 0
      ? data.errors.join(", ")
      : (data.message || "Erro na requisição");
    throw new Error(msg);
  }

  if (!res.ok) {
    console.error("API ERROR:", data);
    throw new Error(data?.message || `Erro ${res.status} na requisição`);
  }

  return data?.data ?? data ?? null;
}

// ===============================
// BASE REQUEST
// ===============================
async function request(url, options = {}) {
  const res = await fetchWithTimeout(url, {
    headers: getHeaders(),
    ...options,
  });

  return handleResponse(res);
}

// ===============================
// GET
// ===============================
export async function getOS() {
  const data = await request(OS_URL);
  return Array.isArray(data) ? data : [];
}

// ===============================
// CREATE
// ===============================
export async function createOS(payload) {
  return request(OS_URL, {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

// ===============================
// UPDATE
// ===============================
export async function updateOS(id, payload) {
  return request(`${OS_URL}/${id}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

// ===============================
// DELETE
// ===============================
export async function deleteOS(id) {
  return request(`${OS_URL}/${id}`, {
    method: "DELETE",
  });
}

// ===============================
// PATCH STATUS
// ===============================
export async function patchOSStatus(id, status) {
  return request(`${OS_URL}/${id}/status`, {
    method: "PATCH",
    body: JSON.stringify({ status }),
  });
}

// ===============================
// FATURAR
// ===============================
export async function faturarOS(id) {
  return request(`${OS_URL}/${id}/faturar`, {
    method: "PUT",
  });
}

// ===============================
// DESFATURAR
// ===============================
export async function desfaturarOS(id, senha) {
  return request(`${OS_URL}/${id}/desfaturar`, {
    method: "PUT",
    body: JSON.stringify({ senha }),
  });
}

// ===============================
// LOGS
// ===============================
export async function getLogsByOrdem(id) {
  return request(`${OS_URL}/${id}/logs`);
}

// ===============================
// DASHBOARD (resumo geral)
// ===============================
export async function getDashboard() {
  return request(`${BASE_URL}/Relatorios/dashboard`);
}

// ===============================
// CLIENTES
// ===============================
export async function getClientes() {
  const data = await request(`${BASE_URL}/Clientes`);
  return Array.isArray(data) ? data : [];
}

export async function getClienteStats() {
  return request(`${BASE_URL}/Clientes/stats`);
}

export async function getClienteById(id) {
  return request(`${BASE_URL}/Clientes/${id}`);
}

// ===============================
// EVOLUÇÃO DE FATURAMENTO
// ===============================
export async function getEvolucao() {
  const data = await request(`${BASE_URL}/Relatorios/evolucao-faturamento`);
  return Array.isArray(data) ? data : [];
}

// ===============================
// FINANCEIRO
// ===============================
export async function getFinanceiroResumo(periodo = "Este mês") {
  return request(`${BASE_URL}/Financeiro/resumo?periodo=${encodeURIComponent(periodo)}`);
}

export async function getFinanceiroTransacoes(periodo = "Este mês") {
  const data = await request(`${BASE_URL}/Financeiro/transacoes?periodo=${encodeURIComponent(periodo)}`);
  return Array.isArray(data) ? data : [];
}

export async function getFinanceiroReceitaSemanal(periodo = "Este mês") {
  const data = await request(`${BASE_URL}/Financeiro/receita-semanal?periodo=${encodeURIComponent(periodo)}`);
  return Array.isArray(data) ? data : [];
}

export async function getFinanceiroPorServico(periodo = "Este mês") {
  const data = await request(`${BASE_URL}/Financeiro/por-servico?periodo=${encodeURIComponent(periodo)}`);
  return Array.isArray(data) ? data : [];
}

// ===============================
// RELATORIOS — TOP / ATIVIDADE
// ===============================
export async function getTopClientes(limit = 4) {
  const data = await request(`${BASE_URL}/Relatorios/top-clientes?limit=${limit}`);
  return Array.isArray(data) ? data : [];
}

export async function getTopServicos(limit = 4) {
  const data = await request(`${BASE_URL}/Relatorios/top-servicos?limit=${limit}`);
  return Array.isArray(data) ? data : [];
}

export async function getAtividade() {
  const data = await request(`${BASE_URL}/Relatorios/atividade`);
  return Array.isArray(data) ? data : [];
}

// ===============================
// CLIENTES — CREATE / UPDATE / VEÍCULOS
// ===============================
export async function createCliente(dto) {
  // ClienteInputDTO: { nome, telefone, email?, documento?, veiculos:[{marca,modelo,placa,ano}] }
  const payload = {
    nome:      dto.nome,
    telefone:  dto.telefone,
    email:     dto.email     || null,
    documento: dto.documento || null,   // 400 guard: nunca enviar string vazia
    veiculos:  Array.isArray(dto.veiculos) ? dto.veiculos : [],
  };
  return request(`${BASE_URL}/Clientes`, {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export async function deleteCliente(id) {
  return request(`${BASE_URL}/Clientes/${id}`, { method: "DELETE" });
}

export async function updateCliente(id, dto) {
  const payload = {
    nome:       dto.nome,
    telefone:   dto.telefone,
    email:      dto.email      || null,
    documento:  dto.documento  || null,
    cep:        dto.cep        || null,
    logradouro: dto.logradouro || null,
    bairro:     dto.bairro     || null,
    cidade:     dto.cidade     || null,
    estado:     dto.estado     || null,
  };
  return request(`${BASE_URL}/Clientes/${id}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export async function addVeiculoToCliente(clienteId, veiculo) {
  return request(`${BASE_URL}/Clientes/${clienteId}/veiculos`, {
    method: "POST",
    body: JSON.stringify(veiculo),
  });
}

export async function getVeiculosByCliente(clienteId) {
  const url = BASE_URL + '/Veiculos/cliente/' + String(clienteId).split(':')[0];
  console.log("[api] getVeiculosByCliente →", url);
  const data = await request(url);
  return Array.isArray(data) ? data : [];
}
