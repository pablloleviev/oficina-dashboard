import { Doughnut, Line } from "react-chartjs-2"
import {
  Chart as ChartJS,
  ArcElement,
  LineElement,
  CategoryScale,
  LinearScale,
  PointElement,
  Tooltip,
  Legend,
  Filler
} from "chart.js"

ChartJS.register(
  ArcElement,
  LineElement,
  CategoryScale,
  LinearScale,
  PointElement,
  Tooltip,
  Legend,
  Filler
)

function num(v) {
  const n = Number(v)
  return isNaN(n) ? 0 : n
}

function Dashboard({ servicos = [], kpis = null }) {

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

  // 🔥 NORMALIZAÇÃO SEGURA
  const dados = servicos.map(s => ({
    ...s,
    status: normalizeStatus(s.status),
    valor: Number(s.valor || 0),
    faturado: Boolean(s.faturado)
  }))

  // =========================
  // STATUS
  // =========================
  const finalizados = dados.filter(s =>
    s.status === "Finalizado" || s.status === "Entregue"
  )

  const andamento = dados.filter(s => s.status === "EmAndamento")
  const aguardando = dados.filter(s => s.status === "Pendente")

  // =========================
  // FINANCEIRO (CORRIGIDO)
  // =========================

  // ✔ Já entrou dinheiro
  const faturados = dados.filter(s => s.faturado)

  // ✔ Pode cobrar (REGRA PROFISSIONAL)
  const naoFaturados = dados.filter(s =>
    (s.status === "Finalizado" || s.status === "Entregue") &&
    !s.faturado
  )

  const faturamentoTotal = faturados.reduce(
    (acc, s) => acc + s.valor,
    0
  )

  const aReceber = naoFaturados.reduce(
    (acc, s) => acc + s.valor,
    0
  )

  const ticketMedio = faturados.length > 0
    ? faturamentoTotal / faturados.length
    : 0

  // =========================
  // ALERTA
  // =========================
  const temAlerta = kpis ? (kpis.ordensPendentes ?? 0) > 0 : aReceber > 0

  // =========================
  // GRÁFICO STATUS
  // =========================
  const pieData = {
    datasets: [
      {
        data: [
          finalizados.length,
          andamento.length,
          aguardando.length
        ],
        backgroundColor: ["#22c55e", "#eab308", "#ef4444"],
        borderWidth: 0
      }
    ]
  }

  const pieOptions = {
    cutout: "75%",
    plugins: { legend: { display: false } }
  }

  // =========================
  // FATURAMENTO 7 DIAS (CORRIGIDO)
  // =========================
  const hoje = new Date()
  const dias = []

  for (let i = 6; i >= 0; i--) {
    const d = new Date()
    d.setDate(hoje.getDate() - i)

    dias.push({
      label: d.toLocaleDateString("pt-BR", {
        day: "2-digit",
        month: "2-digit"
      }),
      date: d.toISOString().split("T")[0]
    })
  }

  const faturamento = dias.map(d =>
    faturados
      .filter(s => {
        if (!s.dataFaturamento) return false

        const data = new Date(s.dataFaturamento)
        if (isNaN(data)) return false

        return data.toISOString().split("T")[0] === d.date
      })
      .reduce((acc, s) => acc + s.valor, 0)
  )

  const lineData = {
    labels: dias.map(d => d.label),
    datasets: [
      {
        data: faturamento,
        borderColor: "#3b82f6",
        backgroundColor: "#3b82f633",
        tension: 0.4,
        fill: true
      }
    ]
  }

  const lineOptions = {
    plugins: { legend: { display: false } },
    scales: {
      x: { ticks: { color: "#94a3b8" } },
      y: { ticks: { color: "#94a3b8" } }
    }
  }

  // =========================
  // UI
  // =========================
  return (
    <div className="space-y-8">

      {/* ALERTA */}
      {temAlerta && (
        <div className="bg-yellow-500/10 border border-yellow-500 text-yellow-400 p-4 rounded-lg text-sm">
          {kpis
            ? <span>⚠ <b>{num(kpis.ordensPendentes)}</b> ordens pendentes de atendimento</span>
            : <span>⚠ Você tem <b>R$ {aReceber.toFixed(2)}</b> aguardando faturamento</span>
          }
        </div>
      )}

      {/* MÉTRICAS */}
      <div className="grid grid-cols-4 gap-6">

        <div className="card">
          <p className="text-slate-400 text-sm">Receita Total</p>
          <h2 className="text-2xl font-bold text-green-400">
            R$ {num(kpis?.totalReceitas ?? faturamentoTotal).toFixed(2)}
          </h2>
        </div>

        <div className="card">
          <p className="text-slate-400 text-sm">Saldo</p>
          <h2 className="text-2xl font-bold" style={{ color: num(kpis?.saldo ?? aReceber) >= 0 ? "#22c55e" : "#ef4444" }}>
            R$ {num(kpis?.saldo ?? aReceber).toFixed(2)}
          </h2>
        </div>

        <div className="card">
          <p className="text-slate-400 text-sm">Ticket Médio</p>
          <h2 className="text-2xl font-bold">
            R$ {num(kpis?.ticketMedio ?? ticketMedio).toFixed(2)}
          </h2>
        </div>

        <div className="card">
          <p className="text-slate-400 text-sm">Total de Clientes</p>
          <h2 className="text-2xl font-bold text-blue-400">
            {num(kpis?.totalClientes ?? finalizados.length)}
          </h2>
        </div>

      </div>

      {/* GRÁFICOS */}
      <div className="grid grid-cols-2 gap-8">

        <div className="card flex flex-col items-center justify-center">
          <h2 className="text-lg mb-6">Status das Ordens</h2>

          <div className="relative w-[220px] h-[220px]">
            <Doughnut data={pieData} options={pieOptions} />

            <div className="absolute inset-0 flex flex-col items-center justify-center">
              <span className="text-3xl font-bold">{dados.length}</span>
              <span className="text-sm text-slate-400">ordens</span>
            </div>
          </div>
        </div>

        <div className="card">
          <h2 className="text-lg mb-4">Faturamento (7 dias)</h2>
          <Line data={lineData} options={lineOptions} />
        </div>

      </div>

    </div>
  )
}

export default Dashboard
