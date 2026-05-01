import { useState, useEffect } from "react"
import KanbanBoard from "../components/KanbanBoard"
import Toast from "../components/Toast"
import { getOS, createOS, updateOS, faturarOS, desfaturarOS, getClientes, getVeiculosByCliente } from "../services/api"

const MEIOS_PAGAMENTO = ["Dinheiro", "PIX", "Cartão Crédito", "Cartão Débito", "Transferência", "Boleto"]

function OrdensPage({ showToast, role }) {
  const [ordens, setOrdens] = useState([])
  const [loading, setLoading] = useState(true)

  // form state
  const [clienteId, setClienteId]       = useState("")
  const [veiculoId, setVeiculoId]       = useState("")
  const [servico, setServico]           = useState("")
  const [valor, setValor]               = useState("")
  const [status, setStatus]             = useState("Pendente")
  const [meioPagamento, setMeioPagamento] = useState("PIX")

  // dropdown data
  const [clientesLista, setClientesLista]   = useState([])
  const [veiculosLista, setVeiculosLista]   = useState([])

  function normalizeStatus(status) {
    if (typeof status === "number") {
      const map = { 0: "Pendente", 1: "EmAndamento", 2: "Finalizado", 3: "Entregue" }
      return map[status] || "Pendente"
    }
    return String(status || "Pendente").trim()
  }

  function normalizeFaturado(value) {
    return value === true
  }

  useEffect(() => {
    async function carregar() {
      try {
        const [data, clientes] = await Promise.all([getOS(), getClientes()])
        const normalizado = Array.isArray(data)
          ? data.map(o => ({
              ...o,
              status: normalizeStatus(o.status),
              faturado: normalizeFaturado(o.faturado)
            }))
          : []
        setOrdens(normalizado)
        setClientesLista(clientes)
      } catch {
        showToast?.("Erro ao carregar ordens", "error")
      } finally {
        setLoading(false)
      }
    }
    carregar()
  }, [])

  useEffect(() => {
    if (!clienteId) { setVeiculosLista([]); setVeiculoId(""); return }
    const cliente = clientesLista.find(c => c.id === Number(clienteId))
    setVeiculosLista(cliente?.veiculos || [])
    setVeiculoId("")
  }, [clienteId, clientesLista])

  async function salvar() {
    if (!clienteId || !servico || !valor || !meioPagamento) {
      return showToast?.("Preencha cliente, serviço, valor e meio de pagamento", "error")
    }
    if (veiculosLista.length > 0 && !veiculoId) {
      return showToast?.("Selecione o veículo do cliente", "error")
    }

    const payload = {
      clienteId:     Number(clienteId),
      veiculoId:     veiculoId ? Number(veiculoId) : null,
      servico,
      valor:         Number(valor),
      status,
      meioPagamento,
    }

    try {
      const criado = await createOS(payload)
      setOrdens(prev => [
        ...prev,
        {
          ...criado,
          status: normalizeStatus(criado?.status),
          faturado: normalizeFaturado(criado?.faturado)
        }
      ])
      showToast?.("Ordem adicionada com sucesso")
      setClienteId(""); setVeiculoId(""); setServico("")
      setValor(""); setStatus("Pendente"); setMeioPagamento("PIX")
    } catch {
      showToast?.("Erro ao salvar ordem", "error")
    }
  }

  async function faturar(id) {
    try {
      const atualizado = await faturarOS(id)
      setOrdens(prev =>
        prev.map(o =>
          o.id === id
            ? { ...atualizado, status: normalizeStatus(atualizado.status), faturado: true }
            : o
        )
      )
      showToast?.("Faturado com sucesso")
    } catch {
      showToast?.("Erro ao faturar", "error")
    }
  }

  async function desfaturar(id, senha) {
    try {
      const atualizado = await desfaturarOS(id, senha)
      setOrdens(prev =>
        prev.map(o =>
          o.id === id
            ? { ...atualizado, status: normalizeStatus(atualizado.status), faturado: false }
            : o
        )
      )
      showToast?.("Desfaturado com sucesso")
    } catch {
      showToast?.("Erro ao desfaturar", "error")
    }
  }

  const inputStyle = {
    background: "var(--card2)", border: "1px solid var(--border)",
    borderRadius: "8px", padding: "8px 12px", fontSize: "12px",
    color: "var(--text)", fontFamily: "'DM Sans', sans-serif",
    outline: "none", width: "100%"
  }

  return (
    <div style={{ padding: "28px 32px" }}>
      {/* TOPBAR */}
      <div style={{ display: "flex", alignItems: "flex-start", justifyContent: "space-between", marginBottom: "20px" }}>
        <div>
          <h2 style={{ fontFamily: "'Syne', sans-serif", fontSize: "22px", fontWeight: 700, color: "#fff" }}>
            Ordens de Serviço
          </h2>
          <p style={{ fontSize: "12px", color: "var(--muted)", marginTop: "3px" }}>
            Gestão operacional do fluxo de trabalho
          </p>
        </div>
      </div>

      {/* FORM */}
      <div style={{
        background: "var(--card)", border: "1px solid var(--border)",
        borderRadius: "12px", padding: "18px 20px", marginBottom: "22px"
      }}>
        <div style={{
          fontSize: "11px", color: "var(--muted)",
          textTransform: "uppercase", letterSpacing: "1px", marginBottom: "12px"
        }}>
          Adicionar Nova Ordem
        </div>
        <div style={{
          display: "grid",
          gridTemplateColumns: "2fr 1.5fr 1.5fr 1fr 1.2fr 1fr auto",
          gap: "8px"
        }}>
          {/* Cliente dropdown */}
          <select style={inputStyle} value={clienteId} onChange={e => setClienteId(e.target.value)}>
            <option value="">Selecionar cliente...</option>
            {clientesLista?.map(c => (
              <option key={c.id} value={c.id}>{c.nome}</option>
            ))}
          </select>

          {/* Veículo dropdown (depende do cliente) */}
          <select style={inputStyle} value={veiculoId} onChange={e => setVeiculoId(e.target.value)} disabled={!clienteId}>
            <option value="">{clienteId ? "Selecionar veículo..." : "← escolha um cliente"}</option>
            {veiculosLista?.map(v => {
              const label = (v.nome || `${v.marca ?? ""} ${v.modelo ?? ""}`).trim() || "Veículo"
              return <option key={v.id} value={v.id}>{label} · {v.placa}</option>
            })}
          </select>

          <input style={inputStyle} placeholder="Serviço" value={servico} onChange={e => setServico(e.target.value)} />
          <input style={inputStyle} type="number" placeholder="Valor" value={valor} onChange={e => setValor(e.target.value)} />

          {/* Meio de pagamento */}
          <select style={inputStyle} value={meioPagamento} onChange={e => setMeioPagamento(e.target.value)}>
            {MEIOS_PAGAMENTO.map(m => <option key={m} value={m}>{m}</option>)}
          </select>

          <select style={inputStyle} value={status} onChange={e => setStatus(e.target.value)}>
            <option value="Pendente">Pendente</option>
            <option value="EmAndamento">Em Andamento</option>
            <option value="Finalizado">Finalizado</option>
            <option value="Entregue">Entregue</option>
          </select>

          <button
            onClick={salvar}
            style={{
              background: "var(--accent)", color: "#fff", border: "none",
              borderRadius: "8px", padding: "8px 16px", fontSize: "12px",
              fontWeight: 600, fontFamily: "'DM Sans', sans-serif",
              cursor: "pointer", whiteSpace: "nowrap"
            }}
          >
            Adicionar
          </button>
        </div>
      </div>

      {/* KANBAN */}
      {loading ? (
        <div style={{ textAlign: "center", padding: "40px", color: "var(--muted)" }}>
          Carregando ordens...
        </div>
      ) : (
        <KanbanBoard
          ordens={ordens}
          setOrdens={setOrdens}
          faturar={faturar}
          desfaturar={desfaturar}
          role={role}
        />
      )}
    </div>
  )
}

export default OrdensPage
