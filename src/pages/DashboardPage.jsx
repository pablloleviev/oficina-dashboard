import { useState, useEffect } from "react"
import Dashboard from "../components/Dashboard"
import { getOS, getDashboard, getFinanceiroResumo } from "../services/api"

function num(v) {
  const n = Number(v)
  return isNaN(n) ? 0 : n
}

function DashboardPage({ showToast }) {
  const [ordens, setOrdens] = useState([])
  const [kpis, setKpis] = useState(null)
  const [loading, setLoading] = useState(true)

  function normalizeStatus(status) {
    if (typeof status === "number") {
      const map = { 0: "Pendente", 1: "EmAndamento", 2: "Finalizado", 3: "Entregue" }
      return map[status] || "Pendente"
    }
    return String(status || "Pendente").trim()
  }

  useEffect(() => {
    async function carregar() {
      try {
        const [data, relatorio, financeiro] = await Promise.all([
          getOS(),
          getDashboard(),
          getFinanceiroResumo(),
        ])
        const normalizado = Array.isArray(data)
          ? data.map(o => ({
              ...o,
              status: normalizeStatus(o.status),
              faturado: o.faturado === true
            }))
          : []
        setOrdens(normalizado)
        setKpis({
          // GET /Relatorios/dashboard → { totalClientes, totalVeiculos, ordensPendentes, ticketMedio }
          totalClientes:   num(relatorio?.totalClientes),
          totalVeiculos:   num(relatorio?.totalVeiculos),
          ordensPendentes: num(relatorio?.ordensPendentes),
          ticketMedio:     num(relatorio?.ticketMedio),
          // GET /Financeiro/resumo → { totalReceitas, totalDespesas, saldo, ordensPendentes, ordensEmAndamento }
          totalReceitas:   num(financeiro?.totalReceitas),
          totalDespesas:   num(financeiro?.totalDespesas),
          saldo:           num(financeiro?.saldo),
          ordensEmAndamento: num(financeiro?.ordensEmAndamento),
        })
      } catch {
        showToast?.("Erro ao carregar dados", "error")
      } finally {
        setLoading(false)
      }
    }
    carregar()
  }, [])

  if (loading) {
    return (
      <div style={{
        display: "flex", alignItems: "center", justifyContent: "center",
        height: "100vh", color: "var(--muted)", fontFamily: "'DM Sans', sans-serif"
      }}>
        Carregando...
      </div>
    )
  }

  return (
    <div style={{ padding: "28px 32px" }}>
      <div style={{ marginBottom: "24px" }}>
        <h2 style={{
          fontFamily: "'Syne', sans-serif", fontSize: "22px",
          fontWeight: 700, color: "#fff"
        }}>
          Dashboard
        </h2>
        <p style={{ fontSize: "12px", color: "var(--muted)", marginTop: "3px" }}>
          Visão geral do negócio
        </p>
      </div>
      <Dashboard servicos={ordens} kpis={kpis} />
    </div>
  )
}

export default DashboardPage
