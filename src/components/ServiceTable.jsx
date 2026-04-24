function ServiceTable({ servicos = [], onDelete, onEdit }) {

  // =========================
  // VALIDAÇÃO
  // =========================
  if (!Array.isArray(servicos) || servicos.length === 0) {
    return (
      <div className="bg-slate-800 p-6 rounded-xl text-center text-slate-400">
        Nenhum registro encontrado.
      </div>
    )
  }

  // =========================
  // NORMALIZA STATUS (ENUM + STRING)
  // =========================
  function formatStatus(status) {
    if (status === null || status === undefined) return "—"

    if (typeof status === "number") {
      const map = {
        0: "Pendente",
        1: "Em andamento",
        2: "Finalizado",
        3: "Entregue"
      }
      return map[status] || "—"
    }

    const s = String(status).toLowerCase()

    if (s.includes("pendente")) return "Pendente"
    if (s.includes("andamento")) return "Em andamento"
    if (s.includes("finalizado")) return "Finalizado"
    if (s.includes("entregue")) return "Entregue"

    return status
  }

  // =========================
  // COR DO STATUS
  // =========================
  function getStatusColor(status) {
    const s = formatStatus(status)

    if (s === "Finalizado" || s === "Entregue") return "bg-green-500/20 text-green-400"
    if (s === "Em andamento") return "bg-yellow-500/20 text-yellow-400"
    if (s === "Pendente") return "bg-red-500/20 text-red-400"

    return "bg-slate-600 text-white"
  }

  // =========================
  return (
    <div className="bg-slate-800 p-6 rounded-xl overflow-x-auto">

      <table className="w-full text-sm">

        <thead className="text-slate-400 text-xs uppercase border-b border-slate-700">
          <tr>
            <th className="p-3 text-left">Cliente</th>
            <th className="p-3 text-left">Veículo</th>
            <th className="p-3 text-left">Serviço</th>
            <th className="p-3 text-left">Valor</th>
            <th className="p-3 text-left">Status</th>
            <th className="p-3 text-center">Ações</th>
          </tr>
        </thead>

        <tbody>

          {servicos.map((s) => (

            <tr key={s.id} className="border-b border-slate-700 hover:bg-slate-700/40 transition">

              {/* CLIENTE + TELEFONE */}
              <td className="p-3">
                <div className="font-semibold">{s.cliente || "—"}</div>
                <div className="text-xs text-slate-400">
                  {s.telefone || ""}
                </div>
              </td>

              {/* VEÍCULO + PLACA */}
              <td className="p-3">
                <div>{s.veiculo || "—"}</div>
                <div className="text-xs text-slate-400">
                  {s.placa || ""}
                </div>
              </td>

              {/* SERVIÇO */}
              <td className="p-3">
                {s.servico || "—"}
              </td>

              {/* VALOR */}
              <td className="p-3 font-semibold">
                R$ {Number(s.valor || 0).toFixed(2)}
              </td>

              {/* STATUS COM BADGE */}
              <td className="p-3">
                <span className={`px-3 py-1 rounded-full text-xs font-semibold ${getStatusColor(s.status)}`}>
                  {formatStatus(s.status)}
                </span>
              </td>

              {/* AÇÕES */}
              <td className="p-3">

                <div className="flex justify-center gap-2">

                  <button
                    onClick={() => onEdit(s)}
                    className="bg-blue-500 hover:bg-blue-600 px-3 py-1 rounded-lg text-xs font-semibold transition"
                  >
                    Editar
                  </button>

                  <button
                    onClick={() => onDelete(s.id)}
                    className="bg-red-500 hover:bg-red-600 px-3 py-1 rounded-lg text-xs font-semibold transition"
                  >
                    Excluir
                  </button>

                </div>

              </td>

            </tr>

          ))}

        </tbody>

      </table>

    </div>
  )
}

export default ServiceTable
