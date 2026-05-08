import { useState, useEffect, useRef } from "react"

const BASE_URL = import.meta.env.VITE_API_URL || "https://autoflow-api-p4tv.onrender.com/api"

function authHeaders() {
  const token = localStorage.getItem("token")
  return {
    "Content-Type": "application/json",
    ...(token && { Authorization: `Bearer ${token}` }),
  }
}

function Badge({ status }) {
  const map = {
    Trial:     { bg: "rgba(234,179,8,0.15)",   color: "#facc15", label: "Trial" },
    Ativa:     { bg: "rgba(34,197,94,0.15)",   color: "#4ade80", label: "Ativa" },
    Suspensa:  { bg: "rgba(239,68,68,0.15)",   color: "#f87171", label: "Suspensa" },
    Inativa:   { bg: "rgba(100,116,139,0.15)", color: "#94a3b8", label: "Inativa" },
  }
  const s = map[status] ?? { bg: "rgba(100,116,139,0.15)", color: "#94a3b8", label: status ?? "—" }
  return (
    <span style={{
      background: s.bg, color: s.color,
      padding: "2px 10px", borderRadius: "999px",
      fontSize: "11px", fontWeight: 600, whiteSpace: "nowrap"
    }}>
      {s.label}
    </span>
  )
}

function PlanoDropdown({ oficina, planos, onTrocar }) {
  const [open, setOpen] = useState(false)
  const [loading, setLoading] = useState(false)
  const ref = useRef(null)

  useEffect(() => {
    function handle(e) { if (ref.current && !ref.current.contains(e.target)) setOpen(false) }
    document.addEventListener("mousedown", handle)
    return () => document.removeEventListener("mousedown", handle)
  }, [])

  async function trocar(planoId) {
    setLoading(true)
    setOpen(false)
    try {
      const res = await fetch(`${BASE_URL}/oficinas/${oficina.id}/plano`, {
        method: "PUT",
        headers: authHeaders(),
        body: JSON.stringify({ planoId }),
      })
      if (!res.ok) throw new Error(`Erro ${res.status}`)
      onTrocar(oficina.id, planoId)
    } catch (err) {
      alert(err.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div ref={ref} style={{ position: "relative", display: "inline-block" }}>
      <button
        onClick={() => setOpen(v => !v)}
        disabled={loading}
        style={{
          fontSize: "11px", padding: "4px 10px", borderRadius: "6px",
          border: "1px solid var(--border)", background: "var(--card)",
          color: "var(--text)", cursor: "pointer", whiteSpace: "nowrap"
        }}
      >
        {loading ? "..." : "Trocar Plano ▾"}
      </button>
      {open && (
        <div style={{
          position: "absolute", top: "calc(100% + 4px)", right: 0, zIndex: 50,
          background: "var(--card)", border: "1px solid var(--border)",
          borderRadius: "8px", minWidth: "150px", overflow: "hidden",
          boxShadow: "0 8px 24px rgba(0,0,0,0.4)"
        }}>
          {planos.map(p => (
            <button
              key={p.id}
              onClick={() => trocar(p.id)}
              style={{
                display: "block", width: "100%", textAlign: "left",
                padding: "8px 12px", fontSize: "12px",
                color: p.id === oficina.planoId ? "var(--accent)" : "var(--text)",
                background: "none", border: "none", cursor: "pointer",
                borderBottom: "1px solid var(--border)"
              }}
            >
              {p.nome}
              <span style={{ color: "var(--muted)", marginLeft: "6px", fontSize: "10px" }}>
                R$ {Number(p.preco ?? p.valor ?? 0).toFixed(2).replace(".", ",")}
              </span>
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

function StatCard({ label, value, color }) {
  return (
    <div style={{
      background: "var(--card)", border: "1px solid var(--border)",
      borderRadius: "12px", padding: "20px 24px", flex: 1, minWidth: "140px"
    }}>
      <p style={{ fontSize: "11px", color: "var(--muted)", textTransform: "uppercase", letterSpacing: "1px", marginBottom: "8px" }}>
        {label}
      </p>
      <p style={{ fontSize: "28px", fontWeight: 700, color: color ?? "var(--text)" }}>
        {value ?? "—"}
      </p>
    </div>
  )
}

function fmt(dateStr) {
  if (!dateStr) return "—"
  try { return new Date(dateStr).toLocaleDateString("pt-BR") } catch { return dateStr }
}

export default function AdminPage() {
  const [oficinas, setOficinas] = useState([])
  const [planos, setPlanos] = useState([])
  const [loading, setLoading] = useState(true)
  const [erro, setErro] = useState("")
  const [busca, setBusca] = useState("")

  useEffect(() => {
    Promise.all([
      fetch(`${BASE_URL}/oficinas`, { headers: authHeaders() }).then(r => r.json()),
      fetch(`${BASE_URL}/oficinas/planos`, { headers: authHeaders() }).then(r => r.json()),
    ])
      .then(([oRes, pRes]) => {
        const oData = Array.isArray(oRes?.data) ? oRes.data : Array.isArray(oRes) ? oRes : []
        const pData = Array.isArray(pRes?.data) ? pRes.data : Array.isArray(pRes) ? pRes : []
        setOficinas(oData)
        setPlanos(pData)
      })
      .catch(err => setErro(err.message || "Erro ao carregar dados"))
      .finally(() => setLoading(false))
  }, [])

  function handleTrocarPlano(oficinaid, planoId) {
    setOficinas(prev =>
      prev.map(o => o.id === oficinaid ? { ...o, planoId, plano: planos.find(p => p.id === planoId)?.nome } : o)
    )
  }

  const filtradas = oficinas.filter(o =>
    !busca || o.nome?.toLowerCase().includes(busca.toLowerCase()) || o.slug?.toLowerCase().includes(busca.toLowerCase())
  )

  const stats = {
    total:    oficinas.length,
    trial:    oficinas.filter(o => o.status === "Trial").length,
    ativas:   oficinas.filter(o => o.status === "Ativa").length,
    suspensas: oficinas.filter(o => o.status === "Suspensa").length,
  }

  return (
    <div style={{ padding: "32px", maxWidth: "1200px" }}>
      <h1 style={{ fontSize: "20px", fontWeight: 700, color: "var(--text)", marginBottom: "4px" }}>
        Painel Admin
      </h1>
      <p style={{ fontSize: "13px", color: "var(--muted)", marginBottom: "28px" }}>
        Gestão global de oficinas e planos
      </p>

      {/* CARDS */}
      <div style={{ display: "flex", gap: "16px", flexWrap: "wrap", marginBottom: "32px" }}>
        <StatCard label="Total de Oficinas" value={stats.total} />
        <StatCard label="Em Trial"   value={stats.trial}    color="#facc15" />
        <StatCard label="Ativas"     value={stats.ativas}   color="#4ade80" />
        <StatCard label="Suspensas"  value={stats.suspensas} color="#f87171" />
      </div>

      {/* TABELA */}
      <div style={{
        background: "var(--card)", border: "1px solid var(--border)",
        borderRadius: "12px", overflow: "hidden"
      }}>
        <div style={{ padding: "16px 20px", borderBottom: "1px solid var(--border)", display: "flex", alignItems: "center", gap: "12px" }}>
          <h2 style={{ fontSize: "14px", fontWeight: 600, color: "var(--text)", flex: 1 }}>
            Oficinas
          </h2>
          <input
            type="text"
            placeholder="Buscar por nome ou slug..."
            value={busca}
            onChange={e => setBusca(e.target.value)}
            style={{
              background: "var(--bg)", border: "1px solid var(--border)",
              borderRadius: "8px", padding: "6px 12px", fontSize: "12px",
              color: "var(--text)", outline: "none", width: "220px"
            }}
          />
        </div>

        {loading && (
          <p style={{ padding: "40px", textAlign: "center", color: "var(--muted)", fontSize: "13px" }}>
            Carregando...
          </p>
        )}

        {erro && (
          <p style={{ padding: "40px", textAlign: "center", color: "#f87171", fontSize: "13px" }}>
            {erro}
          </p>
        )}

        {!loading && !erro && (
          <div style={{ overflowX: "auto" }}>
            <table style={{ width: "100%", borderCollapse: "collapse", fontSize: "13px" }}>
              <thead>
                <tr style={{ borderBottom: "1px solid var(--border)" }}>
                  {["Nome", "Slug", "Plano", "Status", "Trial até", "Criado em", "Ação"].map(h => (
                    <th key={h} style={{
                      padding: "10px 16px", textAlign: "left",
                      fontSize: "11px", fontWeight: 600,
                      color: "var(--muted)", textTransform: "uppercase", letterSpacing: "0.5px",
                      whiteSpace: "nowrap"
                    }}>
                      {h}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {filtradas.length === 0 && (
                  <tr>
                    <td colSpan={7} style={{ padding: "40px", textAlign: "center", color: "var(--muted)" }}>
                      Nenhuma oficina encontrada
                    </td>
                  </tr>
                )}
                {filtradas.map((o, i) => (
                  <tr key={o.id} style={{
                    borderBottom: i < filtradas.length - 1 ? "1px solid var(--border)" : "none",
                    transition: "background 0.1s"
                  }}
                    onMouseEnter={e => e.currentTarget.style.background = "rgba(255,255,255,0.02)"}
                    onMouseLeave={e => e.currentTarget.style.background = ""}
                  >
                    <td style={{ padding: "12px 16px", color: "var(--text)", fontWeight: 500 }}>
                      {o.nome}
                    </td>
                    <td style={{ padding: "12px 16px", color: "var(--muted)", fontFamily: "monospace", fontSize: "12px" }}>
                      {o.slug}
                    </td>
                    <td style={{ padding: "12px 16px", color: "var(--text)" }}>
                      {o.plano ?? "—"}
                    </td>
                    <td style={{ padding: "12px 16px" }}>
                      <Badge status={o.status} />
                    </td>
                    <td style={{ padding: "12px 16px", color: "var(--muted)", whiteSpace: "nowrap" }}>
                      {fmt(o.trialAte ?? o.trialExpiraEm ?? o.trialEnd)}
                    </td>
                    <td style={{ padding: "12px 16px", color: "var(--muted)", whiteSpace: "nowrap" }}>
                      {fmt(o.criadoEm ?? o.createdAt ?? o.dataCriacao)}
                    </td>
                    <td style={{ padding: "12px 16px" }}>
                      <PlanoDropdown oficina={o} planos={planos} onTrocar={handleTrocarPlano} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}
