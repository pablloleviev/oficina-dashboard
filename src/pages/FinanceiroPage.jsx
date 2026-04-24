import { useState, useEffect } from "react"
import "../styles/ds.css"
import {
  getFinanceiroResumo,
  getFinanceiroTransacoes,
  getFinanceiroReceitaSemanal,
  getFinanceiroPorServico,
} from "../services/api"

const PERIODS = ["7 dias", "Este mês", "3 meses", "Ano"]
const TYPE_ICON  = { entrada: "↑", saida: "↓", pendente: "⏳" }
const TYPE_BG    = { entrada: "rgba(34,197,94,0.12)", saida: "rgba(239,68,68,0.12)", pendente: "rgba(245,158,11,0.12)" }
const TYPE_COLOR = { entrada: "var(--green)", saida: "var(--red)", pendente: "var(--yellow)" }
const SERV_COLORS = ["var(--green)","var(--accent)","var(--yellow)","var(--cyan)","var(--purple)"]

function fmtBRL(v) {
  return Number(v ?? 0).toLocaleString("pt-BR", { style: "currency", currency: "BRL" })
}

function fmtDate(iso) {
  if (!iso) return "—"
  try { return new Date(iso).toLocaleDateString("pt-BR") } catch { return String(iso) }
}

function EmptyRow({ cols, msg = "Sem dados para o período" }) {
  return (
    <div style={{ gridColumn: `1 / ${cols + 1}`, padding: "32px", textAlign: "center", color: "var(--muted)", fontSize: "12px" }}>
      {msg}
    </div>
  )
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

function FinanceiroPage() {
  const [period, setPeriod] = useState("Este mês")
  const [resumo, setResumo] = useState(null)
  const [transacoes, setTransacoes] = useState([])
  const [receitaSemanal, setReceitaSemanal] = useState([])
  const [porServico, setPorServico] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    async function load() {
      setLoading(true)
      setError(null)
      try {
        const [res, trans, semanal, servicos] = await Promise.all([
          getFinanceiroResumo(period),
          getFinanceiroTransacoes(period),
          getFinanceiroReceitaSemanal(period).catch(() => []),
          getFinanceiroPorServico(period).catch(() => []),
        ])
        setResumo(res)
        setTransacoes(trans)
        setReceitaSemanal(semanal)
        setPorServico(servicos)
      } catch (err) {
        setError(err.message)
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [period])

  const barMax = Math.max(...receitaSemanal.map(s => Math.max(Number(s.receita ?? 0), Number(s.despesa ?? 0))), 1)
  const serviMax = Math.max(...porServico.map(s => Number(s.percentual ?? 0)), 1)
  const transColGrid = "0.5fr 2fr 1.5fr 1fr 1fr 1fr"

  return (
    <div className="ds-page">
      {/* TOPBAR */}
      <div className="ds-topbar">
        <div>
          <h2>Financeiro</h2>
          <p>Entradas, saídas e controle de caixa</p>
        </div>
        <div style={{
          background: "var(--card)", border: "1px solid var(--border)",
          borderRadius: "9px", padding: "3px", display: "flex", gap: "2px"
        }}>
          {PERIODS.map(p => (
            <button
              key={p}
              onClick={() => setPeriod(p)}
              style={{
                padding: "6px 14px", borderRadius: "7px", fontSize: "12px",
                fontWeight: p === period ? 600 : 400,
                color: p === period ? "#fff" : "var(--muted)",
                background: p === period ? "var(--accent)" : "none",
                border: "none", cursor: "pointer",
                fontFamily: "'DM Sans', sans-serif", transition: "all 0.15s"
              }}
            >
              {p}
            </button>
          ))}
        </div>
      </div>

      {/* ERROR */}
      {error && (
        <div style={{
          background: "rgba(239,68,68,0.1)", border: "1px solid rgba(239,68,68,0.3)",
          borderRadius: "10px", padding: "14px 18px", marginBottom: "18px",
          fontSize: "13px", color: "var(--red)"
        }}>
          ⚠ {error}
        </div>
      )}

      {loading ? (
        <div style={{ textAlign: "center", padding: "80px 0", color: "var(--muted)", fontSize: "13px" }}>
          Carregando dados financeiros...
        </div>
      ) : !error && (
        <>
          {/* SUMMARY CARDS — GET /Financeiro/resumo */}
          {/* Fields: { totalReceitas, totalDespesas, saldo, ordensPendentes, ordensEmAndamento } */}
          <div style={{
            display: "grid", gridTemplateColumns: "1.5fr 1fr 1fr 1fr",
            gap: "14px", marginBottom: "22px"
          }}>
            {[
              {
                label: "Receita Total",
                value: fmtBRL(resumo?.totalReceitas),
                color: "var(--green)", big: true, highlight: true,
              },
              {
                label: "Despesas",
                value: fmtBRL(resumo?.totalDespesas),
                color: "var(--red)",
              },
              {
                label: "Saldo",
                value: fmtBRL(resumo?.saldo),
                color: (resumo?.saldo ?? 0) >= 0 ? "var(--green)" : "var(--red)",
              },
              {
                label: "Ordens Pendentes",
                value: resumo?.ordensPendentes ?? 0,
                color: "var(--yellow)",
              },
            ].map(s => (
              <div
                key={s.label}
                style={{
                  background: s.highlight
                    ? "linear-gradient(135deg, rgba(34,197,94,0.12), rgba(6,182,212,0.08))"
                    : "var(--card)",
                  border: `1px solid ${s.highlight ? "rgba(34,197,94,0.25)" : "var(--border)"}`,
                  borderRadius: "12px", padding: "20px 20px 16px",
                }}
              >
                <div style={{ fontSize: "11px", color: "var(--muted)", textTransform: "uppercase", letterSpacing: "0.8px", marginBottom: "8px" }}>
                  {s.label}
                </div>
                <div style={{
                  fontFamily: "'Syne', sans-serif", fontWeight: 700, color: s.color, lineHeight: 1,
                  fontSize: s.big ? "28px" : "22px"
                }}>
                  {s.value}
                </div>
              </div>
            ))}
          </div>

          {/* CENTER GRID */}
          <div style={{ display: "grid", gridTemplateColumns: "1.8fr 1fr", gap: "16px", marginBottom: "22px" }}>

            {/* BAR CHART — GET /Financeiro/receita-semanal */}
            <div className="ds-card" style={{ padding: "20px" }}>
              <div className="ds-card-title">Receita x Despesas</div>
              <div className="ds-card-subtitle">Comparativo semanal no período</div>
              {receitaSemanal.length === 0 ? (
                <div style={{ height: "140px", display: "flex", alignItems: "center", justifyContent: "center", color: "var(--muted)", fontSize: "12px" }}>
                  Sem dados para o período
                </div>
              ) : (
                <>
                  <div style={{ height: "140px", display: "flex", alignItems: "flex-end", gap: "20px", padding: "0 4px", marginTop: "12px" }}>
                    {receitaSemanal.map((d, i) => (
                      <div key={i} style={{ display: "flex", alignItems: "flex-end", gap: "4px", flex: 1 }}>
                        <div style={{
                          flex: 1, borderRadius: "4px 4px 0 0",
                          background: "rgba(59,130,246,0.7)",
                          height: `${(Number(d.receita ?? 0) / barMax) * 100}%`,
                          minHeight: "2px"
                        }} />
                        <div style={{
                          flex: 1, borderRadius: "4px 4px 0 0",
                          background: "rgba(239,68,68,0.5)",
                          height: `${(Number(d.despesa ?? 0) / barMax) * 100}%`,
                          minHeight: "2px"
                        }} />
                      </div>
                    ))}
                  </div>
                  <div style={{ display: "flex", justifyContent: "space-around", marginTop: "8px" }}>
                    {receitaSemanal.map((d, i) => (
                      <span key={i} style={{ fontSize: "11px", color: "var(--muted)", flex: 1, textAlign: "center" }}>
                        {d.semana ?? `S${i + 1}`}
                      </span>
                    ))}
                  </div>
                </>
              )}
              <div style={{ display: "flex", gap: "16px", marginTop: "12px" }}>
                {[
                  { color: "rgba(59,130,246,0.7)", label: "Receita" },
                  { color: "rgba(239,68,68,0.5)", label: "Despesas" }
                ].map(l => (
                  <div key={l.label} style={{ display: "flex", alignItems: "center", gap: "6px", fontSize: "11px", color: "var(--muted)" }}>
                    <div style={{ width: "10px", height: "10px", borderRadius: "2px", background: l.color }} />
                    {l.label}
                  </div>
                ))}
              </div>
            </div>

            {/* POR SERVIÇO — GET /Financeiro/por-servico */}
            {/* Fields: [ { nome, valor, percentual } ] */}
            <div className="ds-card" style={{ padding: "20px" }}>
              <div className="ds-card-title">Por Serviço</div>
              <div className="ds-card-subtitle">Receita por categoria</div>
              {porServico.length === 0 ? (
                <div style={{ padding: "20px 0", color: "var(--muted)", fontSize: "12px", textAlign: "center" }}>
                  Sem dados para o período
                </div>
              ) : porServico.map((b, i) => {
                const cor = SERV_COLORS[i % SERV_COLORS.length]
                const pct = Number(b.percentual ?? 0)
                return (
                  <div key={b.nome} style={{ marginBottom: "14px" }}>
                    <div style={{ display: "flex", justifyContent: "space-between", marginBottom: "5px" }}>
                      <span style={{ fontSize: "12px", fontWeight: 500 }}>{b.nome}</span>
                      <div>
                        <span style={{ fontFamily: "'Syne', sans-serif", fontSize: "13px", fontWeight: 700, color: cor }}>
                          {fmtBRL(b.valor)}
                        </span>
                        <span style={{ fontSize: "10px", color: "var(--muted)", marginLeft: "4px" }}>{pct.toFixed(1)}%</span>
                      </div>
                    </div>
                    <div style={{ height: "5px", background: "#1e2d3d", borderRadius: "3px", overflow: "hidden" }}>
                      <div style={{ height: "100%", width: `${(pct / serviMax) * 100}%`, background: cor, borderRadius: "3px" }} />
                    </div>
                  </div>
                )
              })}
            </div>
          </div>

          {/* TRANSACTIONS — GET /Financeiro/transacoes */}
          {/* Fields: [ { tipo, descricao, valor, data, categoria, status, meioPagamento, referenciaId } ] */}
          <div className="ds-table">
            <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", padding: "16px 20px", borderBottom: "1px solid var(--border)" }}>
              <div>
                <div className="ds-card-title" style={{ marginBottom: "2px" }}>Histórico de Transações</div>
                <div style={{ fontSize: "11px", color: "var(--muted)" }}>Últimas movimentações</div>
              </div>
              <button
                className="ds-btn-secondary"
                style={{ fontSize: "12px" }}
                onClick={() => {
                  if (!transacoes.length) return
                  downloadCSV(
                    `financeiro-${period.replace(/ /g, "-")}.csv`,
                    ["Tipo","Descrição","Categoria","Data","Valor","Status","Meio Pagamento"],
                    transacoes.map(t => [
                      t.tipo, t.descricao, t.categoria, fmtDate(t.data),
                      Number(t.valor ?? 0).toFixed(2), t.status, t.meioPagamento
                    ])
                  )
                }}
              >
                Exportar CSV
              </button>
            </div>

            <div className="ds-table-header" style={{ gridTemplateColumns: transColGrid }}>
              {["Tipo","Descrição","Categoria","Data","Valor","Status"].map(h => (
                <div key={h} className="ds-th">{h}</div>
              ))}
            </div>

            {transacoes.length === 0 ? (
              <div style={{ padding: "32px", textAlign: "center", color: "var(--muted)", fontSize: "12px" }}>
                Nenhuma transação no período
              </div>
            ) : transacoes.map((t, i) => {
              const tipo = (t.tipo ?? "entrada").toLowerCase()
              const valor = Number(t.valor ?? 0)
              const valorStr = tipo === "entrada"
                ? `+${fmtBRL(valor)}`
                : tipo === "saida"
                  ? `-${fmtBRL(valor)}`
                  : fmtBRL(valor)
              const stPago = t.status === "Recebido" || t.status === "Pago"
              const stBg    = stPago ? "rgba(34,197,94,0.12)" : "rgba(245,158,11,0.12)"
              const stColor = stPago ? "var(--green)" : "var(--yellow)"
              return (
                <div key={i} className="ds-table-row" style={{ gridTemplateColumns: transColGrid }}>
                  <div>
                    <div style={{
                      width: "28px", height: "28px", borderRadius: "8px",
                      display: "flex", alignItems: "center", justifyContent: "center",
                      fontSize: "13px", fontWeight: 700,
                      background: TYPE_BG[tipo] ?? TYPE_BG.entrada,
                      color: TYPE_COLOR[tipo] ?? TYPE_COLOR.entrada
                    }}>
                      {TYPE_ICON[tipo] ?? "↑"}
                    </div>
                  </div>
                  <div style={{ fontSize: "12px", fontWeight: 500 }}>{t.descricao ?? "—"}</div>
                  <div className="ds-td">{t.categoria ?? "—"}</div>
                  <div className="ds-td muted">{fmtDate(t.data)}</div>
                  <div style={{
                    fontFamily: "'Syne', sans-serif", fontSize: "13px", fontWeight: 700,
                    color: TYPE_COLOR[tipo] ?? TYPE_COLOR.entrada
                  }}>
                    {valorStr}
                  </div>
                  <div>
                    <span style={{
                      display: "inline-flex", alignItems: "center",
                      fontSize: "11px", fontWeight: 500, padding: "3px 10px",
                      borderRadius: "20px", background: stBg, color: stColor
                    }}>
                      {t.status ?? "—"}
                    </span>
                  </div>
                </div>
              )
            })}
          </div>
        </>
      )}
    </div>
  )
}

export default FinanceiroPage
