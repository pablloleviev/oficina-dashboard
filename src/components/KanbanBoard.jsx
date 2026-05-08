import { useState } from "react"
import { patchOSStatus, deleteOS } from "../services/api"

function KanbanBoard({ ordens = [], setOrdens, faturar, desfaturar, role }) {

  const [loadingId, setLoadingId] = useState(null)
  const [deletingId, setDeletingId] = useState(null)

  function normalizeStatus(status) {
    if (typeof status === "number") {
      const map = {
        0: "Pendente",
        1: "EmAndamento",
        2: "Finalizado",
        3: "Entregue"
      }
      return map[status] || "Pendente"
    }

    return String(status || "Pendente")
  }

  const colunas = {
    Pendente: [],
    EmAndamento: [],
    Finalizado: [],
    Entregue: []
  }

  ordens.forEach(o => {
    const status = normalizeStatus(o.status)
    colunas[status]?.push(o)
  })

  // =========================
  async function moverStatus(os, novoStatus) {

    // 🔥 BLOQUEIO HARD FRONT
    if (os.faturado) return

    try {
      setLoadingId(os.id)

      const atualizado = await patchOSStatus(os.id, novoStatus)

      setOrdens(prev =>
        prev.map(o =>
          o.id === os.id
            ? { ...o, ...atualizado, status: normalizeStatus(atualizado?.status ?? novoStatus), faturado: atualizado?.faturado === true }
            : o
        )
      )

    } catch (err) {
      console.error(err)
      alert("Erro ao mover status")
    } finally {
      setLoadingId(null)
    }
  }

  function getActions(os) {
    const s = normalizeStatus(os.status)

    // 🔥 TRAVA TOTAL
    if (os.faturado) return []

    if (s === "Pendente") return ["iniciar"]
    if (s === "EmAndamento") return ["finalizar", "voltar"]
    if (s === "Finalizado") return ["entregar", "voltar"]
    if (s === "Entregue") return ["voltar"]

    return []
  }

  function handleAction(os, action) {

    // 🔥 FAIL FAST
    if (os.faturado) return

    const s = normalizeStatus(os.status)

    if (action === "iniciar") return moverStatus(os, "EmAndamento")
    if (action === "finalizar") return moverStatus(os, "Finalizado")
    if (action === "entregar") return moverStatus(os, "Entregue")

    if (action === "voltar") {
      if (s === "EmAndamento") return moverStatus(os, "Pendente")
      if (s === "Finalizado") return moverStatus(os, "EmAndamento")
      if (s === "Entregue") return moverStatus(os, "Finalizado")
    }
  }

  async function excluirOS(os) {
    if (!window.confirm(`Excluir ordem de ${os.cliente ?? "este cliente"}? Esta ação não pode ser desfeita.`)) return
    setDeletingId(os.id)
    try {
      await deleteOS(os.id)
      setOrdens(prev => prev.filter(o => o.id !== os.id))
    } catch {
      alert("Erro ao excluir ordem")
    } finally {
      setDeletingId(null)
    }
  }

  function Card({ os }) {

    const actions = getActions(os)
    const isLoading = loadingId === os.id
    const isDeleting = deletingId === os.id
    const status = normalizeStatus(os.status)

    const isFaturado = os.faturado
    const isEntregue = status === "Entregue"

    function getBadge() {
      if (isFaturado) return { text: "✔ Faturado", color: "text-green-400" }
      if (isEntregue) return { text: "💰 Pronto p/ faturar", color: "text-yellow-400" }
      if (status === "EmAndamento") return { text: "⚙ Em execução", color: "text-blue-400" }
      return null
    }

    const badge = getBadge()

    function str(v) {
      if (v == null || typeof v === "object") return ""
      return String(v)
    }

    return (
      <div className={`bg-slate-800 p-4 rounded-xl space-y-3 shadow border transition-all
        ${isFaturado ? "border-green-500/30 opacity-80" : "border-slate-700 hover:shadow-lg"}
      `}>

        <div className="flex justify-between items-center">
          <div className="font-semibold text-white">{str(os.cliente)}</div>
          <div className="flex items-center gap-2">
            {badge && (
              <span className={`text-xs font-bold ${badge.color}`}>
                {badge.text}
              </span>
            )}
            {!isFaturado && (
              <button
                disabled={isDeleting}
                onClick={() => excluirOS(os)}
                title="Excluir ordem"
                className="text-slate-500 hover:text-red-400 transition-colors disabled:opacity-40"
                style={{ background: "none", border: "none", cursor: "pointer", fontSize: "14px", padding: "2px 4px" }}
              >
                🗑
              </button>
            )}
          </div>
        </div>

        <div className="text-xs text-slate-400">
          {str(os.veiculo)} • {str(os.placa)}
        </div>

        <div className="text-sm text-slate-300">
          {str(os.servico)}
        </div>

        <div className={`text-lg font-bold ${
          isFaturado ? "text-green-400" : "text-blue-400"
        }`}>
          R$ {Number(os.valor || 0).toFixed(2)}
        </div>

        <div className="flex gap-2 flex-wrap pt-2">

          {/* PRINCIPAL */}
          {actions.includes("entregar") && (
            <button
              disabled={isLoading}
              onClick={() => handleAction(os, "entregar")}
              className="bg-blue-600 hover:bg-blue-700 px-3 py-1 rounded text-xs font-bold disabled:opacity-50"
            >
              🚚 Entregar
            </button>
          )}

          {actions.includes("finalizar") && (
            <button
              disabled={isLoading}
              onClick={() => handleAction(os, "finalizar")}
              className="bg-green-600 hover:bg-green-700 px-3 py-1 rounded text-xs font-bold disabled:opacity-50"
            >
              ✔ Finalizar
            </button>
          )}

          {actions.includes("iniciar") && (
            <button
              disabled={isLoading}
              onClick={() => handleAction(os, "iniciar")}
              className="bg-yellow-500 hover:bg-yellow-600 px-3 py-1 rounded text-xs font-bold disabled:opacity-50"
            >
              ▶ Iniciar
            </button>
          )}

          {/* FINANCEIRO */}
          {isEntregue && !isFaturado && (
            <button
              disabled={isLoading}
              onClick={() => faturar(os.id)}
              className="bg-purple-600 hover:bg-purple-700 px-3 py-1 rounded text-xs font-bold disabled:opacity-50"
            >
              💰 Faturar
            </button>
          )}

          {isFaturado && role === "Admin" && (
            <button
              disabled={isLoading}
              onClick={() => {
                const senha = prompt("Senha admin:")
                if (senha) desfaturar(os.id, senha)
              }}
              className="bg-red-600 hover:bg-red-700 px-3 py-1 rounded text-xs font-bold disabled:opacity-50"
            >
              ❌ Desfaturar
            </button>
          )}

          {/* SECUNDÁRIO */}
          {actions.includes("voltar") && !isFaturado && (
            <button
              disabled={isLoading}
              onClick={() => handleAction(os, "voltar")}
              className="bg-slate-600 hover:bg-slate-500 px-3 py-1 rounded text-xs disabled:opacity-50"
            >
              ← Voltar
            </button>
          )}

        </div>

        {isLoading && (
          <div className="text-xs text-slate-400 animate-pulse">
            Processando...
          </div>
        )}

      </div>
    )
  }

  function Coluna({ titulo, itens }) {
    return (
      <div className="bg-slate-900 rounded-xl p-4 w-72 min-h-[400px]">
        <h2 className="font-semibold mb-4 text-slate-300">
          {titulo} ({itens.length})
        </h2>

        <div className="space-y-3">
          {itens.map(os => (
            <Card key={os.id} os={os} />
          ))}
        </div>
      </div>
    )
  }

  return (
    <div className="flex gap-6 overflow-x-auto pb-4">
      <Coluna titulo="Pendente" itens={colunas.Pendente} />
      <Coluna titulo="Em andamento" itens={colunas.EmAndamento} />
      <Coluna titulo="Finalizado" itens={colunas.Finalizado} />
      <Coluna titulo="Entregue" itens={colunas.Entregue} />
    </div>
  )
}

export default KanbanBoard
