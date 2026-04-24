// ===============================
// FORMATAR TEXTO
// ===============================
function formatarTexto(texto) {
  if (!texto) return ""

  return texto
    .toLowerCase()
    .split(" ")
    .map(p => p.charAt(0).toUpperCase() + p.slice(1))
    .join(" ")
}

// ===============================
// NORMALIZAR STATUS
// ===============================
function normalizarStatus(status) {
  if (!status) return ""

  const s = status.toLowerCase()

  if (s.includes("concl")) return "Concluído"
  if (s.includes("and")) return "Em andamento"
  if (s.includes("aguard")) return "Aguardando"

  return "Aguardando"
}

// ===============================
// INTERPRETAR TEXTO
// ===============================
export function interpretarTexto(texto) {
  if (!texto) return null

  const partes = texto.trim().split(" ")

  if (partes.length < 4) return null

  const cliente = formatarTexto(partes[0])
  const valor = partes.find(p => !isNaN(p))

  const statusBruto = partes[partes.length - 1]
  const status = normalizarStatus(statusBruto)

  const servico = formatarTexto(
    partes.slice(1, -2).join(" ")
  )

  return {
    cliente,
    servico,
    valor,
    status
  }
}

// ===============================
// GERAR DESCRIÇÃO INTELIGENTE (PRONTO PRA IA)
// ===============================
export async function gerarDescricaoInteligente(servico) {

  // FUTURO: aqui entra IA

  return `Serviço de ${servico.servico.toLowerCase()} realizado para o cliente ${servico.cliente}, no valor de R$${Number(servico.valor).toFixed(2)}, com status ${servico.status.toLowerCase()}.`

}
