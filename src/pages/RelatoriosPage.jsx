import { useState, useEffect, useMemo } from "react"
import "../styles/ds.css"
import { getDashboard, getEvolucao, getTopClientes, getTopServicos, getAtividade } from "../services/api"

const DAYS = ["Seg", "Ter", "Qua", "Qui", "Sex"]
const RANK_COLORS = {
  1: { bg: "rgba(245,158,11,0.2)",  color: "#fbbf24" },
  2: { bg: "rgba(148,163,184,0.2)", color: "#94a3b8" },
  3: { bg: "rgba(180,120,60,0.2)",  color: "#cd7f32" },
}

function RankBadge({ pos }) {
  const s = RANK_COLORS[pos] || { bg: "rgba(90,106,126,0.2)", color: "var(--muted)" }
  return (
    <div style={{
      width: "22px", height: "22px", borderRadius: "50%", flexShrink: 0,
      display: "flex", alignItems: "center", justifyContent: "center",
      fontSize: "11px", fontWeight: 700, background: s.bg, color: s.color
    }}>
      {pos}
    </div>
  )
}

function fmtBRL(v) {
  return Number(v ?? 0).toLocaleString("pt-BR", { style: "currency", currency: "BRL" })
}

function downloadCSV(filename, headers, rows) {
  const csv = [headers, ...rows]
    .map(r => r.map(c => `"${String(c ?? "").replace(/"/g, '""')}"`).join(","))
    .join("\n")
  const blob = new Blob(["\uFEFF" + csv], { type: "text/csv;charset=utf-8" })
  const url = URL.createObjectURL(blob)
  const a = document.createElement("a")
  a.href = url; a.download = filename; a.click()
  URL.revokeObjectURL(url)
}

function RelatoriosPage() {
  const [dashboard, setDashboard] = useState(null)
  const [evolucao, setEvolucao] = useState([])
  const [topClientes, setTopClientes] = useState([])
  const [topServicos, setTopServicos] = useState([])
  const [atividade, setAtividade] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    async function load() {
      try {
        const [dash, evol, clientes, servicos, atv] = await Promise.all([
          getDashboard().catch(() => null),
          getEvolucao().catch(() => []),
          getTopClientes(4).catch(() => []),
          getTopServicos(4).catch(() => []),
          getAtividade().catch(() => []),
        ])
        setDashboard(dash)
        setEvolucao(Array.isArray(evol) ? evol : [])
        setTopClientes(Array.isArray(clientes) ? clientes : [])
        setTopServicos(Array.isArray(servicos) ? servicos : [])
        setAtividade(Array.isArray(atv) ? atv : [])
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [])

  // GET /Relatorios/evolucao-faturamento → [ { mes, total } ]
  const evolMax = Math.max(...evolucao.map(e => Number(e.total ?? 0)), 1)

  // GET /Relatorios/atividade → [ { dia, semanas: number[] } ]
  const heatmapData = useMemo(() => {
    if (atividade.length > 0) {
      return DAYS.map(d => {
        const row = atividade.find(a => a.dia === d)
        return row?.semanas ?? Array(16).fill(0)
      })
    }
    return DAYS.map(() => Array(16).fill(0))
  }, [atividade])

  function heatColor(v) {
    return `rgba(34,197,94,${(Number(v) * 0.18 + 0.05).toFixed(2)})`
  }

  if (loading) {
    return (
      <div className="ds-page">
        <div className="ds-topbar">
          <div><h2>Relatórios &amp; Analytics</h2><p>Visão consolidada do desempenho do negócio</p></div>
        </div>
        <div style={{ textAlign: "center", padding: "80px 0", color: "var(--muted)", fontSize: "13px" }}>
          Carregando relatórios...
        </div>
      </div>
    )
  }

  return (
    <div className="ds-page">
      {/* TOPBAR */}
      <div className="ds-topbar">
        <div>
          <h2>Relatórios &amp; Analytics</h2>
          <p>Visão consolidada do desempenho do negócio</p>
        </div>
        <button
          className="ds-btn-secondary"
          onClick={() => {
            const sections = []

            if (evolucao.length) {
              sections.push(["--- Evolução do Faturamento ---"])
              sections.push(["Mês","Total"])
              evolucao.forEach(e => sections.push([e.mes, Number(e.total ?? 0).toFixed(2)]))
              sections.push([])
            }

            if (topClientes.length) {
              sections.push(["--- Top Clientes ---"])
              sections.push(["Posição","Nome","Ordens","Valor Total"])
              topClientes.forEach(c => sections.push([c.posicao, c.nome, c.totalOrdens, Number(c.valorTotal ?? 0).toFixed(2)]))
              sections.push([])
            }

            if (topServicos.length) {
              sections.push(["--- Top Serviços ---"])
              sections.push(["Posição","Serviço","Frequência","Valor Total"])
              topServicos.forEach(s => sections.push([s.posicao, s.nome, s.frequencia, Number(s.valorTotal ?? 0).toFixed(2)]))
            }

            if (!sections.length) return

            const csv = sections
              .map(r => r.map(c => `"${String(c ?? "").replace(/"/g, '""')}"`).join(","))
              .join("\n")
            const blob = new Blob(["\uFEFF" + csv], { type: "text/csv;charset=utf-8" })
            const url = URL.createObjectURL(blob)
            const a = document.createElement("a")
            a.href = url; a.download = "relatorio-autoflow.csv"; a.click()
            URL.revokeObjectURL(url)
          }}
        >
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/>
            <polyline points="7 10 12 15 17 10"/>
            <line x1="12" y1="15" x2="12" y2="3"/>
          </svg>
          Exportar Relatório
        </button>
      </div>

      {/* HERO — GET /Relatorios/dashboard */}
      {/* Fields: { totalClientes, totalVeiculos, ordensPendentes, ticketMedio } */}
      <div style={{
        background: "linear-gradient(135deg, rgba(59,130,246,0.15), rgba(6,182,212,0.08), rgba(34,197,94,0.08))",
        border: "1px solid rgba(59,130,246,0.2)", borderRadius: "16px",
        padding: "28px 32px", marginBottom: "22px",
        display: "grid", gridTemplateColumns: "1fr auto", gap: "32px", alignItems: "center",
        position: "relative", overflow: "hidden"
      }}>
        <div style={{
          position: "absolute", right: "-40px", top: "-40px",
          width: "200px", height: "200px",
          background: "radial-gradient(circle, rgba(59,130,246,0.15), transparent 70%)",
          pointerEvents: "none"
        }} />
        <div>
          <div style={{ fontSize: "11px", color: "var(--accent)", textTransform: "uppercase", letterSpacing: "1.5px", marginBottom: "10px" }}>
            Ticket Médio
          </div>
          <div style={{ fontFamily: "'Syne', sans-serif", fontSize: "42px", fontWeight: 800, color: "var(--green)", lineHeight: 1, marginBottom: "8px" }}>
            {fmtBRL(dashboard?.ticketMedio)}
          </div>
          <div style={{ fontSize: "12px", color: "var(--muted)" }}>
            Valor médio por ordem de serviço
          </div>
        </div>
        <div style={{ display: "flex", gap: "28px" }}>
          {[
            { value: Number(dashboard?.totalClientes   ?? 0) || 0, label: "Clientes",         color: "var(--accent)" },
            { value: Number(dashboard?.totalVeiculos   ?? 0) || 0, label: "Veículos",         color: "var(--green)"  },
            { value: Number(dashboard?.ordensPendentes ?? 0) || 0, label: "Ordens Pendentes", color: "var(--yellow)" },
          ].map(s => (
            <div key={s.label} style={{ textAlign: "right" }}>
              <div style={{ fontFamily: "'Syne', sans-serif", fontSize: "22px", fontWeight: 700, color: s.color, lineHeight: 1, marginBottom: "4px" }}>
                {s.value}
              </div>
              <div style={{ fontSize: "11px", color: "var(--muted)" }}>{s.label}</div>
            </div>
          ))}
        </div>
      </div>

      {/* GRAPHS */}
      <div style={{ display: "grid", gridTemplateColumns: "1.6fr 1fr", gap: "16px", marginBottom: "22px" }}>

        {/* EVOLUÇÃO — GET /Relatorios/evolucao-faturamento → [ { mes, total } ] */}
        <div className="ds-card" style={{ padding: "20px" }}>
          <div className="ds-card-title">Evolução do Faturamento</div>
          <div className="ds-card-subtitle">Últimos {evolucao.length || "—"} meses</div>
          {evolucao.length === 0 ? (
            <div style={{ height: "120px", display: "flex", alignItems: "center", justifyContent: "center", color: "var(--muted)", fontSize: "12px" }}>
              Sem dados disponíveis
            </div>
          ) : (
            <>
              <div style={{ height: "120px", display: "flex", alignItems: "flex-end", gap: "6px", padding: "0 4px", marginTop: "16px" }}>
                {evolucao.map((e, i) => (
                  <div key={i} style={{ flex: 1, display: "flex", flexDirection: "column", alignItems: "center", height: "100%", justifyContent: "flex-end" }}>
                    <div style={{
                      width: "100%", borderRadius: "4px 4px 0 0",
                      background: "var(--green)", opacity: 0.8,
                      height: `${(Number(e.total ?? 0) / evolMax) * 100}%`,
                      minHeight: "3px"
                    }} title={fmtBRL(e.total)} />
                  </div>
                ))}
              </div>
              <div style={{ display: "flex", justifyContent: "space-between", marginTop: "8px" }}>
                {evolucao.map((e, i) => (
                  <span key={i} style={{ fontSize: "10px", color: "var(--muted)", flex: 1, textAlign: "center" }}>{e.mes}</span>
                ))}
              </div>
            </>
          )}
        </div>

        {/* HEATMAP — GET /Relatorios/atividade → [ { dia, semanas: number[] } ] */}
        <div className="ds-card" style={{ padding: "20px" }}>
          <div className="ds-card-title">Atividade por Dia</div>
          <div className="ds-card-subtitle">Ordens por dia da semana</div>

          <div style={{ display: "flex", gap: "4px", marginBottom: "4px", paddingLeft: "28px" }}>
            {Array.from({ length: 16 }, (_, i) => (
              <div key={i} style={{ flex: 1, fontSize: "10px", color: "var(--muted)", textAlign: "center" }}>
                {i % 4 === 1 ? `S${Math.floor(i / 4) + 1}` : ""}
              </div>
            ))}
          </div>

          {DAYS.map((d, di) => (
            <div key={d} style={{ display: "flex", alignItems: "center", gap: "4px", marginBottom: "4px" }}>
              <div style={{ fontSize: "9px", color: "var(--muted)", width: "24px", textAlign: "right", flexShrink: 0 }}>{d}</div>
              <div style={{ display: "flex", gap: "3px", flex: 1 }}>
                {(heatmapData[di] ?? []).map((v, wi) => (
                  <div key={wi} style={{
                    height: "14px", flex: 1, borderRadius: "2px",
                    background: heatColor(v), cursor: "default"
                  }} title={`${v} ordens`} />
                ))}
              </div>
            </div>
          ))}

          <div style={{ display: "flex", alignItems: "center", gap: "6px", marginTop: "10px", justifyContent: "flex-end" }}>
            <span style={{ fontSize: "10px", color: "var(--muted)" }}>Menos</span>
            <div style={{ display: "flex", gap: "3px" }}>
              {[0,1,2,3,4,5].map(v => (
                <div key={v} style={{ width: "12px", height: "12px", borderRadius: "2px", background: heatColor(v) }} />
              ))}
            </div>
            <span style={{ fontSize: "10px", color: "var(--muted)" }}>Mais</span>
          </div>
        </div>
      </div>

      {/* RANKINGS */}
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "16px" }}>

        {/* TOP CLIENTES — GET /Relatorios/top-clientes */}
        {/* Fields: [ { posicao, nome, totalOrdens, valorTotal } ] */}
        <div className="ds-card" style={{ padding: "20px" }}>
          <div className="ds-card-title" style={{ marginBottom: "4px" }}>Top Clientes</div>
          <div className="ds-card-subtitle">Por valor gasto</div>
          {topClientes.length === 0 ? (
            <div style={{ padding: "20px 0", color: "var(--muted)", fontSize: "12px", textAlign: "center" }}>Sem dados</div>
          ) : topClientes.map(c => (
            <div key={c.posicao} style={{
              background: "var(--card2)", borderRadius: "8px",
              padding: "10px 12px", marginBottom: "8px",
              display: "flex", alignItems: "center", gap: "10px"
            }}>
              <RankBadge pos={c.posicao} />
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ fontSize: "12px", fontWeight: 500, whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }}>{c.nome}</div>
                <div style={{ fontSize: "10px", color: "var(--muted)", marginTop: "1px" }}>{c.totalOrdens} ordens</div>
              </div>
              <div style={{ fontFamily: "'Syne', sans-serif", fontSize: "12px", fontWeight: 700, color: "var(--green)", whiteSpace: "nowrap" }}>
                {fmtBRL(c.valorTotal)}
              </div>
            </div>
          ))}
        </div>

        {/* TOP SERVIÇOS — GET /Relatorios/top-servicos */}
        {/* Fields: [ { posicao, nome, frequencia, valorTotal } ] */}
        <div className="ds-card" style={{ padding: "20px" }}>
          <div className="ds-card-title" style={{ marginBottom: "4px" }}>Top Serviços</div>
          <div className="ds-card-subtitle">Por frequência</div>
          {topServicos.length === 0 ? (
            <div style={{ padding: "20px 0", color: "var(--muted)", fontSize: "12px", textAlign: "center" }}>Sem dados</div>
          ) : topServicos.map(s => (
            <div key={s.posicao} style={{
              background: "var(--card2)", borderRadius: "8px",
              padding: "10px 12px", marginBottom: "8px",
              display: "flex", alignItems: "center", gap: "10px"
            }}>
              <RankBadge pos={s.posicao} />
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ fontSize: "12px", fontWeight: 500, whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }}>{s.nome}</div>
                <div style={{ fontSize: "10px", color: "var(--muted)", marginTop: "1px" }}>{s.frequencia}x realizado</div>
              </div>
              <div style={{ fontFamily: "'Syne', sans-serif", fontSize: "12px", fontWeight: 700, color: "var(--accent)", whiteSpace: "nowrap" }}>
                {fmtBRL(s.valorTotal)}
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}

export default RelatoriosPage
