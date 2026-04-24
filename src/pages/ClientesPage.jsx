import { useState, useEffect } from "react"
import "../styles/ds.css"
import {
  getClientes, getClienteStats, getClienteById,
  createCliente, updateCliente, deleteCliente,
  getVeiculosByCliente, addVeiculoToCliente, createOS,
} from "../services/api"

const MEIOS_PAGAMENTO = ["Dinheiro", "PIX", "Cartão Crédito", "Cartão Débito", "Transferência", "Boleto"]

const COLORS = ["#3b82f6", "#06b6d4", "#22c55e", "#f59e0b", "#a78bfa", "#ef4444", "#f97316"]

function initials(nome) {
  return (nome ?? "?").split(" ").map(w => w[0]).join("").slice(0, 2).toUpperCase()
}

function mapListItem(c, idx) {
  return {
    id:       c.id,
    nome:     c.nome     ?? "",
    email:    c.email    ?? "",
    tel:      c.telefone ?? "",
    desde:    c.dataCadastro
      ? new Date(c.dataCadastro).toLocaleDateString("pt-BR", { month: "short", year: "numeric" })
      : "—",
    veiculo:  c.veiculoPrincipal ?? "—",
    ordens:   c.totalOrdens ?? 0,
    total:    Number(c.totalGasto ?? 0),
    status:   c.ativo !== undefined ? (c.ativo ? "Ativo" : "Inativo") : "—",
    iniciais: initials(c.nome),
    cor:      COLORS[idx % COLORS.length],
  }
}

const PER_PAGE = 5
const ANO_ATUAL = new Date().getFullYear()

function fmt(v) {
  return "R$ " + Number(v).toFixed(2).replace(".", ",").replace(/\B(?=(\d{3})+(?!\d))/g, ".")
}

const INPUT = {
  background: "var(--card2)", border: "1px solid var(--border)",
  borderRadius: "8px", padding: "8px 12px", fontSize: "12px",
  color: "var(--text)", fontFamily: "'DM Sans', sans-serif",
  outline: "none", width: "100%", boxSizing: "border-box",
}

function ClientesPage({ showToast }) {
  // ── lista ──────────────────────────────────────────────────────────────
  const [clientes, setClientes]       = useState([])
  const [stats, setStats]             = useState({ totalClientes: 0, clientesAtivos: 0, veiculosCadastrados: 0 })
  const [detail, setDetail]           = useState(null)
  const [search, setSearch]           = useState("")
  const [page, setPage]               = useState(1)
  const [selectedId, setSelectedId]   = useState(null)
  const [loadingDetail, setLoadingDetail] = useState(false)

  // ── modal Novo Cliente ─────────────────────────────────────────────────
  const [showModal, setShowModal] = useState(false)
  const [saving, setSaving]       = useState(false)
  const [formError, setFormError] = useState(null)
  const [form, setForm]           = useState({ nome: "", telefone: "", email: "", documento: "", cep: "", logradouro: "", bairro: "", cidade: "", estado: "" })
  const [cepLoading, setCepLoading] = useState(false)
  const [formVeiculos, setFormVeiculos] = useState([])

  // ── modal Nova Ordem (a partir do drawer) ──────────────────────────────
  const [showOrdemModal, setShowOrdemModal] = useState(false)
  const [ordemSaving, setOrdemSaving]       = useState(false)
  const [ordemError, setOrdemError]         = useState(null)
  const [ordemVeiculos, setOrdemVeiculos]   = useState([])
  const [ordemForm, setOrdemForm]           = useState({
    veiculoId: "", servico: "", valor: "", status: "Pendente", meioPagamento: "PIX"
  })

  // ── modal Adicionar Veículo ────────────────────────────────────────────
  const [showVeiculoModal, setShowVeiculoModal] = useState(false)
  const [veiculoSaving, setVeiculoSaving]       = useState(false)
  const [veiculoError, setVeiculoError]         = useState(null)
  const [veiculoForm, setVeiculoForm]           = useState({ marca: "", modelo: "", placa: "", ano: "" })

  // ── modal Editar Cliente ───────────────────────────────────────────────
  const [showEditModal, setShowEditModal] = useState(false)
  const [editSaving, setEditSaving]       = useState(false)
  const [editError, setEditError]         = useState(null)
  const [editForm, setEditForm]           = useState({ nome: "", telefone: "", email: "", documento: "", cep: "", logradouro: "", bairro: "", cidade: "", estado: "" })

  // ── carga inicial ──────────────────────────────────────────────────────
  useEffect(() => {
    carregarLista()
  }, [])

  async function carregarLista() {
    try {
      const [lista, statsData] = await Promise.all([getClientes(), getClienteStats()])
      const mapped = lista.map(mapListItem)
      setClientes(mapped)
      if (statsData) setStats(statsData)
      if (mapped.length > 0) selectCliente(mapped[0].id)
    } catch {
      // silently fail — table stays empty
    }
  }

  async function selectCliente(id) {
    setSelectedId(id)
    setLoadingDetail(true)
    try {
      const d = await getClienteById(id)
      setDetail(d)
    } catch {
      setDetail(null)
    } finally {
      setLoadingDetail(false)
    }
  }

  // ── modal helpers ──────────────────────────────────────────────────────
  function openModal() {
    setForm({ nome: "", telefone: "", email: "", documento: "" })
    setFormVeiculos([])
    setFormError(null)
    setShowModal(true)
  }

  function addVeiculo() {
    setFormVeiculos(prev => [...prev, { marca: "", modelo: "", placa: "", ano: ANO_ATUAL }])
  }

  function removeVeiculo(i) {
    setFormVeiculos(prev => prev.filter((_, idx) => idx !== i))
  }

  function updateVeiculo(i, field, value) {
    setFormVeiculos(prev => prev.map((v, idx) => idx === i ? { ...v, [field]: value } : v))
  }

  async function handleSalvar() {
    if (!form.nome.trim() || !form.telefone.trim()) {
      setFormError("Nome e telefone são obrigatórios.")
      return
    }

    // Validate vehicles — all 4 fields required if vehicle was added
    for (const v of formVeiculos) {
      if (!v.marca.trim() || !v.modelo.trim() || !v.placa.trim() || !v.ano) {
        setFormError("Preencha todos os campos de cada veículo (marca, modelo, placa e ano).")
        return
      }
    }

    setSaving(true)
    setFormError(null)

    try {
      // ClienteInputDTO — veiculos: [] nunca null (backend rejeita null)
      await createCliente({
        nome:       form.nome.trim(),
        telefone:   form.telefone.trim(),
        email:      form.email.trim()      || null,
        documento:  form.documento.trim()  || null,
        cep:        form.cep               || null,
        logradouro: form.logradouro.trim() || null,
        bairro:     form.bairro.trim()     || null,
        cidade:     form.cidade.trim()     || null,
        estado:     form.estado.trim()     || null,
        veiculos:  formVeiculos.map(v => ({
          marca:  v.marca.trim(),
          modelo: v.modelo.trim(),
          placa:  v.placa.trim().toUpperCase(),
          ano:    Number(v.ano),
        })),
      })

      setShowModal(false)
      setForm({ nome: "", telefone: "", email: "", documento: "", cep: "", logradouro: "", bairro: "", cidade: "", estado: "" })
      setFormVeiculos([])
      showToast?.("Cliente criado com sucesso")
      await carregarLista()
    } catch (err) {
      setFormError(err.message)
    } finally {
      setSaving(false)
    }
  }

  // ── Nova Ordem (do drawer) ────────────────────────────────────────────
  async function openOrdemModal() {
    setOrdemError(null)
    // Use vehicles already shown in the drawer — no extra fetch needed
    const fromDrawer = Array.isArray(detail?.veiculos) ? detail.veiculos : []
    const veiculos = fromDrawer.length > 0
      ? fromDrawer
      : await getVeiculosByCliente(selectedId).catch(() => [])
    const autoId = veiculos.length >= 1 ? String(veiculos[0].id) : ""
    setOrdemVeiculos(veiculos)
    setOrdemForm({ veiculoId: autoId, servico: "", valor: "", status: "Pendente", meioPagamento: "PIX" })
    setShowOrdemModal(true)
  }

  async function handleSalvarOrdem() {
    if (!ordemForm.servico || !ordemForm.valor || !ordemForm.meioPagamento) {
      setOrdemError("Preencha serviço, valor e meio de pagamento.")
      return
    }
    if (ordemVeiculos.length > 0 && !ordemForm.veiculoId) {
      setOrdemError("Selecione o veículo.")
      return
    }
    setOrdemSaving(true)
    setOrdemError(null)
    try {
      const vid = parseInt(ordemForm.veiculoId, 10)
      await createOS({
        clienteId:    selectedId,
        veiculoId:    Number.isFinite(vid) ? vid : null,
        servico:      ordemForm.servico,
        valor:        Number(ordemForm.valor),
        status:       ordemForm.status,
        meioPagamento: ordemForm.meioPagamento,
      })
      setShowOrdemModal(false)
      showToast?.("Ordem criada com sucesso")
    } catch (err) {
      setOrdemError(err.message)
    } finally {
      setOrdemSaving(false)
    }
  }

  // ── ViaCEP ────────────────────────────────────────────────────────────
  async function buscarCep(cep, setter) {
    const digits = cep.replace(/\D/g, "")
    if (digits.length !== 8) return
    setCepLoading(true)
    try {
      const res = await fetch(`https://viacep.com.br/ws/${digits}/json/`)
      const data = await res.json()
      if (data.erro) { showToast?.("CEP não encontrado", "error"); return }
      setter(f => ({
        ...f,
        logradouro: data.logradouro ?? "",
        bairro:     data.bairro     ?? "",
        cidade:     data.localidade ?? "",
        estado:     data.uf         ?? "",
      }))
    } catch {
      showToast?.("Erro ao buscar CEP", "error")
    } finally {
      setCepLoading(false)
    }
  }

  // ── Editar Cliente ────────────────────────────────────────────────────
  function openEditModal() {
    setEditError(null)
    setEditForm({
      nome:       detail?.nome       ?? selected?.nome  ?? "",
      telefone:   detail?.telefone   ?? selected?.tel   ?? "",
      email:      detail?.email      ?? selected?.email ?? "",
      documento:  detail?.documento  ?? "",
      cep:        detail?.cep        ?? "",
      logradouro: detail?.logradouro ?? "",
      bairro:     detail?.bairro     ?? "",
      cidade:     detail?.cidade     ?? "",
      estado:     detail?.estado     ?? "",
    })
    setShowEditModal(true)
  }

  async function handleSalvarEdicao() {
    if (!editForm.nome.trim() || !editForm.telefone.trim()) {
      setEditError("Nome e telefone são obrigatórios.")
      return
    }
    setEditSaving(true)
    setEditError(null)
    try {
      await updateCliente(selectedId, {
        nome:       editForm.nome.trim(),
        telefone:   editForm.telefone.trim(),
        email:      editForm.email.trim()      || null,
        documento:  editForm.documento.trim()  || null,
        cep:        editForm.cep               || null,
        logradouro: editForm.logradouro.trim() || null,
        bairro:     editForm.bairro.trim()     || null,
        cidade:     editForm.cidade.trim()     || null,
        estado:     editForm.estado.trim()     || null,
      })
      setShowEditModal(false)
      showToast?.("Cliente atualizado")
      await carregarLista()
    } catch (err) {
      setEditError(err.message)
    } finally {
      setEditSaving(false)
    }
  }

  // ── Excluir Cliente ──────────────────────────────────────────────────────
  async function handleDeletarCliente() {
    const abertas = detail?.ordensAbertas ?? 0
    const msg = abertas > 0
      ? `${selected?.nome} tem ${abertas} ordem(ns) em aberto. Excluir o cliente encerrará essas ordens. Continuar?`
      : `Excluir ${selected?.nome}? Isso também remove seus veículos. Esta ação não pode ser desfeita.`
    if (!window.confirm(msg)) return
    try {
      await deleteCliente(selectedId)
      showToast?.("Cliente excluído")
      setSelectedId(null)
      await carregarLista()
    } catch (err) {
      showToast?.(err.message || "Erro ao excluir cliente", "error")
    }
  }

  // ── Adicionar Veículo ────────────────────────────────────────────────────
  function handleAbrirModalVeiculo() {
    setVeiculoError(null)
    setVeiculoForm({ marca: "", modelo: "", placa: "", ano: "" })
    setShowVeiculoModal(true)
  }

  async function handleSalvarVeiculo() {
    const { marca, modelo, placa, ano } = veiculoForm
    if (!marca.trim() || !modelo.trim() || !placa.trim() || !ano) {
      setVeiculoError("Preencha marca, modelo, placa e ano.")
      return
    }
    setVeiculoSaving(true)
    setVeiculoError(null)
    try {
      const veiculoCriado = await addVeiculoToCliente(selectedId, {
        marca: marca.trim(),
        modelo: modelo.trim(),
        placa: placa.trim().toUpperCase(),
        ano: Number(ano),
      })
      setShowVeiculoModal(false)
      showToast?.("Veículo adicionado")
      // Append returned vehicle directly — no extra round-trip needed
      setDetail(prev => ({
        ...(prev ?? {}),
        veiculos: [...(Array.isArray(prev?.veiculos) ? prev.veiculos : []), veiculoCriado],
      }))
      // Also refresh the list so totalGasto updates in the table
      carregarLista()
    } catch (err) {
      setVeiculoError(err.message || "Erro ao salvar veículo")
    } finally {
      setVeiculoSaving(false)
    }
  }

  // ── filtro / paginação ─────────────────────────────────────────────────
  const filtered = clientes.filter(c =>
    c.nome.toLowerCase().includes(search.toLowerCase()) ||
    c.tel.includes(search) ||
    c.veiculo.toLowerCase().includes(search.toLowerCase())
  )

  const totalPages = Math.ceil(filtered.length / PER_PAGE)
  const slice      = filtered.slice((page - 1) * PER_PAGE, page * PER_PAGE)
  const selected   = clientes.find(c => c.id === selectedId) || clientes[0]

  function handleSearch(val) { setSearch(val); setPage(1) }

  const colGrid = "2fr 1.5fr 1.2fr 1fr 1fr 0.8fr"

  return (
    <div className="ds-page">
      {/* TOPBAR */}
      <div className="ds-topbar">
        <div>
          <h2>Clientes</h2>
          <p>Gerenciamento de clientes cadastrados</p>
        </div>
        <button className="ds-btn-primary" onClick={openModal}>
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
            <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
          </svg>
          Novo Cliente
        </button>
      </div>

      <div style={{ display: "flex", gap: "24px" }}>
        <div style={{ flex: 1, minWidth: 0 }}>
          {/* STATS */}
          <div className="ds-stats-grid">
            <div className="ds-stat-card">
              <div className="ds-stat-icon blue">👥</div>
              <div><div className="ds-stat-value blue">{stats.totalClientes}</div><div className="ds-stat-label">Total de Clientes</div></div>
            </div>
            <div className="ds-stat-card">
              <div className="ds-stat-icon green">✅</div>
              <div><div className="ds-stat-value green">{stats.clientesAtivos}</div><div className="ds-stat-label">Clientes Ativos</div></div>
            </div>
            <div className="ds-stat-card">
              <div className="ds-stat-icon yellow">🚗</div>
              <div><div className="ds-stat-value yellow">{stats.veiculosCadastrados}</div><div className="ds-stat-label">Veículos Cadastrados</div></div>
            </div>
          </div>

          {/* TOOLBAR */}
          <div className="ds-toolbar">
            <div className="ds-search-box">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="var(--muted)" strokeWidth="2">
                <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
              </svg>
              <input
                placeholder="Buscar por nome, telefone ou veículo..."
                value={search}
                onChange={e => handleSearch(e.target.value)}
              />
            </div>
          </div>

          {/* TABLE */}
          <div className="ds-table">
            <div className="ds-table-header" style={{ gridTemplateColumns: colGrid }}>
              {["Cliente","Telefone","Veículo","Ordens","Gasto Total","Status"].map(h => (
                <div key={h} className="ds-th">{h}</div>
              ))}
            </div>

            {slice?.map(c => (
              <div
                key={c.id}
                className={`ds-table-row${c.id === selectedId ? " selected" : ""}`}
                style={{ gridTemplateColumns: colGrid }}
                onClick={() => selectCliente(c.id)}
              >
                <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
                  <div className="ds-avatar" style={{
                    width: "34px", height: "34px", borderRadius: "10px",
                    background: c.cor, fontSize: "12px", flexShrink: 0
                  }}>
                    {c.iniciais}
                  </div>
                  <div>
                    <div style={{ fontSize: "13px", fontWeight: 500 }}>{c.nome}</div>
                    <div style={{ fontSize: "10px", color: "var(--muted)", marginTop: "1px" }}>
                      Cliente desde {c.desde}
                    </div>
                  </div>
                </div>
                <div className="ds-td">{c.tel}</div>
                <div className="ds-td muted">{c.veiculo}</div>
                <div className="ds-td">{c.ordens}</div>
                <div className="ds-td value">{fmt(c.total)}</div>
                <div className="ds-td">
                  <span className={`ds-badge ${c.status === "Ativo" ? "green" : "gray"}`}>
                    ● {c.status}
                  </span>
                </div>
              </div>
            ))}

            {slice?.length === 0 && (
              <div style={{ padding: "32px", textAlign: "center", color: "var(--muted)", fontSize: "12px" }}>
                {search ? "Nenhum cliente encontrado para a busca." : "Nenhum cliente cadastrado ainda."}
              </div>
            )}

            {/* PAGINATION */}
            {filtered.length > 0 && (
              <div className="ds-pagination">
                <span className="ds-pag-info">
                  Mostrando {(page - 1) * PER_PAGE + 1}–{Math.min(page * PER_PAGE, filtered.length)} de {filtered.length} clientes
                </span>
                <div className="ds-pag-btns">
                  <button className="ds-pag-btn" onClick={() => setPage(p => Math.max(1, p - 1))}>‹</button>
                  {Array.from({ length: totalPages }, (_, i) => (
                    <button
                      key={i + 1}
                      className={`ds-pag-btn${page === i + 1 ? " active" : ""}`}
                      onClick={() => setPage(i + 1)}
                    >
                      {i + 1}
                    </button>
                  ))}
                  <button className="ds-pag-btn" onClick={() => setPage(p => Math.min(totalPages, p + 1))}>›</button>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* DRAWER */}
        <div style={{
          width: "300px", minWidth: "300px",
          background: "var(--card)", border: "1px solid var(--border)",
          borderRadius: "12px", padding: "22px",
          position: "sticky", top: "28px", alignSelf: "flex-start",
          maxHeight: "calc(100vh - 100px)", overflowY: "auto"
        }}>
          {!selected ? (
            <div style={{ textAlign: "center", color: "var(--muted)", fontSize: "12px", padding: "20px 0" }}>
              Selecione um cliente
            </div>
          ) : (
            <>
              <div style={{
                width: "60px", height: "60px", borderRadius: "16px",
                background: `linear-gradient(135deg, ${selected.cor ?? "#3b82f6"}, #06b6d4)`,
                display: "flex", alignItems: "center", justifyContent: "center",
                fontFamily: "'Syne', sans-serif", fontSize: "22px",
                fontWeight: 700, color: "#fff", marginBottom: "12px"
              }}>
                {selected.iniciais}
              </div>

              <div style={{ fontFamily: "'Syne', sans-serif", fontSize: "18px", fontWeight: 700 }}>
                {selected.nome}
              </div>
              <div style={{ fontSize: "12px", color: "var(--muted)", marginTop: "3px" }}>
                {selected.tel}
              </div>

              <div className="ds-divider" />

              {loadingDetail ? (
                <div style={{ fontSize: "12px", color: "var(--muted)", textAlign: "center", padding: "16px 0" }}>
                  Carregando...
                </div>
              ) : (
                <>
                  <div style={{ fontSize: "10px", textTransform: "uppercase", letterSpacing: "1px", color: "var(--muted)", marginBottom: "12px" }}>
                    Resumo Financeiro
                  </div>
                  {[
                    { label: "Total Gasto",       value: fmt(detail?.totalGasto ?? 0),   style: { color: "var(--green)", fontFamily: "'Syne', sans-serif", fontSize: "15px", fontWeight: 700 } },
                    { label: "Ordens Abertas",     value: detail?.ordensAbertas    ?? "—" },
                    { label: "Ordens Finalizadas", value: detail?.ordensFinalizadas ?? "—" },
                    { label: "Último Serviço",     value: detail?.ultimoServico ? new Date(detail.ultimoServico).toLocaleDateString("pt-BR") : "—" },
                  ].map(row => (
                    <div key={row.label} style={{ display: "flex", justifyContent: "space-between", marginBottom: "10px" }}>
                      <span style={{ fontSize: "12px", color: "var(--muted)" }}>{row.label}</span>
                      <span style={{ fontSize: "13px", fontWeight: 500, ...(row.style || {}) }}>{row.value}</span>
                    </div>
                  ))}

                  <div className="ds-divider" />

                  <div style={{ fontSize: "10px", textTransform: "uppercase", letterSpacing: "1px", color: "var(--muted)", marginBottom: "10px" }}>
                    Veículos
                  </div>
                  {(detail?.veiculos ?? []).length === 0 ? (
                    <button
                      onClick={handleAbrirModalVeiculo}
                      style={{
                        width: "100%", padding: "10px", borderRadius: "8px", cursor: "pointer",
                        border: "1px dashed var(--border)", background: "transparent",
                        color: "var(--accent)", fontSize: "12px", fontWeight: 600,
                        fontFamily: "'DM Sans', sans-serif", textAlign: "center"
                      }}
                    >
                      + Adicionar Veículo
                    </button>
                  ) : (
                    <>
                      {detail.veiculos.map(v => (
                        <div key={v.id} style={{
                          display: "flex", alignItems: "center", gap: "8px",
                          background: "var(--card2)", border: "1px solid var(--border)",
                          borderRadius: "8px", padding: "9px 12px", marginBottom: "8px"
                        }}>
                          <span style={{ fontSize: "18px" }}>🚗</span>
                          <div>
                            <p style={{ fontSize: "12px", fontWeight: 500 }}>{v.marca} {v.modelo}</p>
                            <span style={{ fontSize: "10px", color: "var(--muted)" }}>{v.placa} · {v.ano}</span>
                          </div>
                        </div>
                      ))}
                      <button
                        onClick={handleAbrirModalVeiculo}
                        style={{
                          width: "100%", padding: "7px", borderRadius: "8px", cursor: "pointer",
                          border: "1px dashed var(--border)", background: "transparent",
                          color: "var(--muted)", fontSize: "11px",
                          fontFamily: "'DM Sans', sans-serif", textAlign: "center", marginTop: "4px"
                        }}
                      >
                        + Adicionar Veículo
                      </button>
                    </>
                  )}
                </>
              )}

              <div style={{ display: "flex", gap: "8px", marginTop: "18px" }}>
                <button className="ds-btn-primary"   style={{ flex: 1, justifyContent: "center" }} onClick={openOrdemModal}>Nova Ordem</button>
                <button className="ds-btn-secondary" style={{ flex: 1, justifyContent: "center" }} onClick={openEditModal}>Editar</button>
                <button
                  onClick={handleDeletarCliente}
                  title="Excluir cliente"
                  style={{
                    background: "rgba(239,68,68,0.1)", border: "1px solid rgba(239,68,68,0.3)",
                    borderRadius: "8px", padding: "8px 10px", cursor: "pointer",
                    color: "var(--red)", fontSize: "14px", flexShrink: 0
                  }}
                >
                  🗑
                </button>
              </div>
            </>
          )}
        </div>
      </div>

      {/* ── MODAL NOVA ORDEM ────────────────────────────────────────────── */}
      {showOrdemModal && (
        <div onClick={e => e.target === e.currentTarget && setShowOrdemModal(false)}
          style={{ position:"fixed",inset:0,zIndex:1000,background:"rgba(0,0,0,0.6)",backdropFilter:"blur(4px)",display:"flex",alignItems:"center",justifyContent:"center",padding:"20px" }}>
          <div style={{ background:"var(--card)",border:"1px solid var(--border)",borderRadius:"16px",width:"100%",maxWidth:"440px",boxShadow:"0 24px 64px rgba(0,0,0,0.5)" }}>
            <div style={{ display:"flex",alignItems:"center",justifyContent:"space-between",padding:"20px 24px",borderBottom:"1px solid var(--border)" }}>
              <div>
                <div style={{ fontFamily:"'Syne',sans-serif",fontSize:"17px",fontWeight:700 }}>Nova Ordem de Serviço</div>
                <div style={{ fontSize:"11px",color:"var(--muted)",marginTop:"2px" }}>Cliente: {selected?.nome}</div>
              </div>
              <button onClick={() => setShowOrdemModal(false)} style={{ background:"none",border:"none",color:"var(--muted)",cursor:"pointer",fontSize:"20px",padding:"4px" }}>×</button>
            </div>
            <div style={{ padding:"22px 24px" }}>
              {ordemVeiculos.length === 0 ? (
                <div style={{
                  background: "rgba(245,158,11,0.08)", border: "1px solid rgba(245,158,11,0.25)",
                  borderRadius: "8px", padding: "12px 14px", marginBottom: "12px",
                  fontSize: "12px", color: "var(--yellow)"
                }}>
                  Nenhum veículo encontrado para este cliente. Cadastre um veículo antes de criar a ordem.
                  <button
                    onClick={() => { setShowOrdemModal(false); handleAbrirModalVeiculo() }}
                    style={{ display: "block", marginTop: "8px", background: "none", border: "none", color: "var(--accent)", fontSize: "12px", fontWeight: 600, cursor: "pointer", padding: 0, fontFamily: "'DM Sans', sans-serif" }}
                  >
                    + Adicionar Veículo →
                  </button>
                </div>
              ) : (
                <div style={{ marginBottom:"12px" }}>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>Veículo *</label>
                  <select
                    style={INPUT}
                    value={ordemForm.veiculoId}
                    onChange={e => setOrdemForm(f => ({...f, veiculoId: e.target.value}))}
                  >
                    {ordemVeiculos.length > 1 && <option value="">Selecionar veículo...</option>}
                    {ordemVeiculos.map(v => {
                      const label = (v.nome || `${v.marca ?? ""} ${v.modelo ?? ""}`).trim() || "Veículo"
                      return <option key={v.id} value={String(v.id)}>{label} · {v.placa}</option>
                    })}
                  </select>
                </div>
              )}
              <div style={{ display:"grid",gridTemplateColumns:"1fr 1fr",gap:"10px",marginBottom:"10px" }}>
                <div>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>Serviço *</label>
                  <input style={INPUT} placeholder="ex: Troca de Óleo" value={ordemForm.servico} onChange={e => setOrdemForm(f => ({...f,servico:e.target.value}))} />
                </div>
                <div>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>Valor (R$) *</label>
                  <input style={INPUT} type="number" placeholder="0,00" value={ordemForm.valor} onChange={e => setOrdemForm(f => ({...f,valor:e.target.value}))} />
                </div>
              </div>
              <div style={{ display:"grid",gridTemplateColumns:"1fr 1fr",gap:"10px",marginBottom:"16px" }}>
                <div>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>Meio de Pagamento *</label>
                  <select style={INPUT} value={ordemForm.meioPagamento} onChange={e => setOrdemForm(f => ({...f,meioPagamento:e.target.value}))}>
                    {MEIOS_PAGAMENTO.map(m => <option key={m} value={m}>{m}</option>)}
                  </select>
                </div>
                <div>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>Status</label>
                  <select style={INPUT} value={ordemForm.status} onChange={e => setOrdemForm(f => ({...f,status:e.target.value}))}>
                    <option value="Pendente">Pendente</option>
                    <option value="EmAndamento">Em Andamento</option>
                    <option value="Finalizado">Finalizado</option>
                    <option value="Entregue">Entregue</option>
                  </select>
                </div>
              </div>
              {ordemError && <div style={{ background:"rgba(239,68,68,0.1)",border:"1px solid rgba(239,68,68,0.3)",borderRadius:"8px",padding:"10px 14px",fontSize:"12px",color:"var(--red)" }}>{ordemError}</div>}
            </div>
            <div style={{ display:"flex",gap:"10px",padding:"16px 24px",borderTop:"1px solid var(--border)",justifyContent:"flex-end" }}>
              <button className="ds-btn-secondary" onClick={() => setShowOrdemModal(false)} disabled={ordemSaving}>Cancelar</button>
              <button className="ds-btn-primary" onClick={handleSalvarOrdem} disabled={ordemSaving || (ordemVeiculos.length > 0 && !ordemForm.veiculoId)} style={{ minWidth:"110px",justifyContent:"center" }}>
                {ordemSaving ? "Salvando..." : "Criar Ordem"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ── MODAL ADICIONAR VEÍCULO ──────────────────────────────────────── */}
      {showVeiculoModal && (
        <div onClick={e => e.target === e.currentTarget && setShowVeiculoModal(false)}
          style={{ position:"fixed",inset:0,zIndex:1100,background:"rgba(0,0,0,0.6)",backdropFilter:"blur(4px)",display:"flex",alignItems:"center",justifyContent:"center",padding:"20px" }}>
          <div style={{ background:"var(--card)",border:"1px solid var(--border)",borderRadius:"16px",width:"100%",maxWidth:"400px",boxShadow:"0 24px 64px rgba(0,0,0,0.5)" }}>
            <div style={{ display:"flex",alignItems:"center",justifyContent:"space-between",padding:"20px 24px",borderBottom:"1px solid var(--border)" }}>
              <div>
                <div style={{ fontFamily:"'Syne',sans-serif",fontSize:"17px",fontWeight:700 }}>Adicionar Veículo</div>
                <div style={{ fontSize:"11px",color:"var(--muted)",marginTop:"2px" }}>Cliente: {selected?.nome}</div>
              </div>
              <button onClick={() => setShowVeiculoModal(false)} style={{ background:"none",border:"none",color:"var(--muted)",cursor:"pointer",fontSize:"20px",padding:"4px" }}>×</button>
            </div>
            <div style={{ padding:"22px 24px" }}>
              <div style={{ display:"grid",gridTemplateColumns:"1fr 1fr",gap:"10px",marginBottom:"10px" }}>
                <div>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>Marca *</label>
                  <input style={INPUT} placeholder="ex: Fiat" value={veiculoForm.marca} onChange={e => setVeiculoForm(f => ({...f,marca:e.target.value}))} />
                </div>
                <div>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>Modelo *</label>
                  <input style={INPUT} placeholder="ex: Palio" value={veiculoForm.modelo} onChange={e => setVeiculoForm(f => ({...f,modelo:e.target.value}))} />
                </div>
              </div>
              <div style={{ display:"grid",gridTemplateColumns:"1fr 1fr",gap:"10px",marginBottom:"16px" }}>
                <div>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>Placa *</label>
                  <input style={INPUT} placeholder="ex: ABC1D23" value={veiculoForm.placa} onChange={e => setVeiculoForm(f => ({...f,placa:e.target.value.toUpperCase()}))} />
                </div>
                <div>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>Ano *</label>
                  <input style={INPUT} type="number" placeholder="ex: 2019" value={veiculoForm.ano} onChange={e => setVeiculoForm(f => ({...f,ano:e.target.value}))} />
                </div>
              </div>
              {veiculoError && <div style={{ background:"rgba(239,68,68,0.1)",border:"1px solid rgba(239,68,68,0.3)",borderRadius:"8px",padding:"10px 14px",fontSize:"12px",color:"var(--red)" }}>{veiculoError}</div>}
            </div>
            <div style={{ display:"flex",gap:"10px",padding:"16px 24px",borderTop:"1px solid var(--border)",justifyContent:"flex-end" }}>
              <button className="ds-btn-secondary" onClick={() => setShowVeiculoModal(false)} disabled={veiculoSaving}>Cancelar</button>
              <button className="ds-btn-primary" onClick={handleSalvarVeiculo} disabled={veiculoSaving} style={{ minWidth:"120px",justifyContent:"center" }}>
                {veiculoSaving ? "Salvando..." : "Adicionar Veículo"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ── MODAL EDITAR CLIENTE ─────────────────────────────────────────── */}
      {showEditModal && (
        <div onClick={e => e.target === e.currentTarget && setShowEditModal(false)}
          style={{ position:"fixed",inset:0,zIndex:1000,background:"rgba(0,0,0,0.6)",backdropFilter:"blur(4px)",display:"flex",alignItems:"center",justifyContent:"center",padding:"20px" }}>
          <div style={{ background:"var(--card)",border:"1px solid var(--border)",borderRadius:"16px",width:"100%",maxWidth:"440px",boxShadow:"0 24px 64px rgba(0,0,0,0.5)" }}>
            <div style={{ display:"flex",alignItems:"center",justifyContent:"space-between",padding:"20px 24px",borderBottom:"1px solid var(--border)" }}>
              <div>
                <div style={{ fontFamily:"'Syne',sans-serif",fontSize:"17px",fontWeight:700 }}>Editar Cliente</div>
                <div style={{ fontSize:"11px",color:"var(--muted)",marginTop:"2px" }}>Atualize os dados do cliente</div>
              </div>
              <button onClick={() => setShowEditModal(false)} style={{ background:"none",border:"none",color:"var(--muted)",cursor:"pointer",fontSize:"20px",padding:"4px" }}>×</button>
            </div>
            <div style={{ padding:"22px 24px" }}>
              <div style={{ display:"grid",gridTemplateColumns:"1fr 1fr",gap:"10px",marginBottom:"10px" }}>
                <div>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>Nome *</label>
                  <input style={INPUT} value={editForm.nome} onChange={e => setEditForm(f => ({...f,nome:e.target.value}))} />
                </div>
                <div>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>Telefone *</label>
                  <input style={INPUT} value={editForm.telefone} onChange={e => setEditForm(f => ({...f,telefone:e.target.value}))} />
                </div>
              </div>
              <div style={{ display:"grid",gridTemplateColumns:"1fr 1fr",gap:"10px",marginBottom:"16px" }}>
                <div>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>E-mail</label>
                  <input style={INPUT} type="email" value={editForm.email} onChange={e => setEditForm(f => ({...f,email:e.target.value}))} />
                </div>
                <div>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>CPF / Documento</label>
                  <input style={INPUT} value={editForm.documento} onChange={e => setEditForm(f => ({...f,documento:e.target.value}))} />
                </div>
              </div>
              {/* Endereço */}
              <div style={{ fontSize:"10px",textTransform:"uppercase",letterSpacing:"1px",color:"var(--muted)",marginBottom:"10px",marginTop:"4px" }}>
                Endereço <span style={{ textTransform:"none",letterSpacing:0,color:"var(--accent)" }}>via CEP</span>
              </div>
              <div style={{ display:"grid",gridTemplateColumns:"1fr 2fr",gap:"10px",marginBottom:"10px" }}>
                <div>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>CEP</label>
                  <input style={INPUT} placeholder="00000-000" maxLength={8} value={editForm.cep}
                    onChange={e => {
                      const v = e.target.value.replace(/\D/g, "").slice(0, 8)
                      setEditForm(f => ({ ...f, cep: v }))
                      if (v.length === 8) buscarCep(v, setEditForm)
                    }} />
                </div>
                <div>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>
                    Logradouro {cepLoading && <span style={{ color:"var(--accent)" }}>buscando...</span>}
                  </label>
                  <input style={INPUT} placeholder="Rua, Avenida..." value={editForm.logradouro} onChange={e => setEditForm(f => ({...f,logradouro:e.target.value}))} />
                </div>
              </div>
              <div style={{ display:"grid",gridTemplateColumns:"1fr 1fr 0.5fr",gap:"10px",marginBottom:"16px" }}>
                <div>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>Bairro</label>
                  <input style={INPUT} value={editForm.bairro} onChange={e => setEditForm(f => ({...f,bairro:e.target.value}))} />
                </div>
                <div>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>Cidade</label>
                  <input style={INPUT} value={editForm.cidade} onChange={e => setEditForm(f => ({...f,cidade:e.target.value}))} />
                </div>
                <div>
                  <label style={{ fontSize:"11px",color:"var(--muted)",display:"block",marginBottom:"5px" }}>UF</label>
                  <input style={INPUT} maxLength={2} value={editForm.estado} onChange={e => setEditForm(f => ({...f,estado:e.target.value.toUpperCase()}))} />
                </div>
              </div>

              {editError && <div style={{ background:"rgba(239,68,68,0.1)",border:"1px solid rgba(239,68,68,0.3)",borderRadius:"8px",padding:"10px 14px",fontSize:"12px",color:"var(--red)" }}>{editError}</div>}
            </div>
            <div style={{ display:"flex",gap:"10px",padding:"16px 24px",borderTop:"1px solid var(--border)",justifyContent:"flex-end" }}>
              <button className="ds-btn-secondary" onClick={() => setShowEditModal(false)} disabled={editSaving}>Cancelar</button>
              <button className="ds-btn-primary" onClick={handleSalvarEdicao} disabled={editSaving} style={{ minWidth:"120px",justifyContent:"center" }}>
                {editSaving ? "Salvando..." : "Salvar Alterações"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ── MODAL NOVO CLIENTE ──────────────────────────────────────────── */}
      {showModal && (
        <div
          onClick={e => e.target === e.currentTarget && setShowModal(false)}
          style={{
            position: "fixed", inset: 0, zIndex: 1000,
            background: "rgba(0,0,0,0.6)", backdropFilter: "blur(4px)",
            display: "flex", alignItems: "center", justifyContent: "center",
            padding: "20px"
          }}
        >
          <div style={{
            background: "var(--card)", border: "1px solid var(--border)",
            borderRadius: "16px", width: "100%", maxWidth: "520px",
            maxHeight: "90vh", overflowY: "auto",
            boxShadow: "0 24px 64px rgba(0,0,0,0.5)"
          }}>
            {/* Header */}
            <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", padding: "20px 24px", borderBottom: "1px solid var(--border)" }}>
              <div>
                <div style={{ fontFamily: "'Syne', sans-serif", fontSize: "17px", fontWeight: 700 }}>Novo Cliente</div>
                <div style={{ fontSize: "11px", color: "var(--muted)", marginTop: "2px" }}>Preencha os dados abaixo</div>
              </div>
              <button
                onClick={() => setShowModal(false)}
                style={{ background: "none", border: "none", color: "var(--muted)", cursor: "pointer", fontSize: "20px", lineHeight: 1, padding: "4px" }}
              >
                ×
              </button>
            </div>

            {/* Body */}
            <div style={{ padding: "22px 24px" }}>
              {/* Dados pessoais */}
              <div style={{ fontSize: "10px", textTransform: "uppercase", letterSpacing: "1px", color: "var(--muted)", marginBottom: "12px" }}>
                Dados Pessoais
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "10px", marginBottom: "10px" }}>
                <div>
                  <label style={{ fontSize: "11px", color: "var(--muted)", display: "block", marginBottom: "5px" }}>
                    Nome <span style={{ color: "var(--red)" }}>*</span>
                  </label>
                  <input
                    style={INPUT}
                    placeholder="Nome completo"
                    value={form.nome}
                    onChange={e => setForm(f => ({ ...f, nome: e.target.value }))}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "11px", color: "var(--muted)", display: "block", marginBottom: "5px" }}>
                    Telefone <span style={{ color: "var(--red)" }}>*</span>
                  </label>
                  <input
                    style={INPUT}
                    placeholder="(11) 99999-9999"
                    value={form.telefone}
                    onChange={e => setForm(f => ({ ...f, telefone: e.target.value }))}
                  />
                </div>
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "10px", marginBottom: "20px" }}>
                <div>
                  <label style={{ fontSize: "11px", color: "var(--muted)", display: "block", marginBottom: "5px" }}>E-mail</label>
                  <input
                    style={INPUT}
                    type="email"
                    placeholder="opcional"
                    value={form.email}
                    onChange={e => setForm(f => ({ ...f, email: e.target.value }))}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "11px", color: "var(--muted)", display: "block", marginBottom: "5px" }}>CPF / Documento</label>
                  <input
                    style={INPUT}
                    placeholder="opcional"
                    value={form.documento}
                    onChange={e => setForm(f => ({ ...f, documento: e.target.value }))}
                  />
                </div>
              </div>

              {/* Endereço via ViaCEP */}
              <div style={{ fontSize: "10px", textTransform: "uppercase", letterSpacing: "1px", color: "var(--muted)", marginBottom: "12px" }}>
                Endereço <span style={{ textTransform: "none", letterSpacing: 0, color: "var(--accent)" }}>via CEP</span>
              </div>
              <div style={{ display: "grid", gridTemplateColumns: "1fr 2fr", gap: "10px", marginBottom: "10px" }}>
                <div>
                  <label style={{ fontSize: "11px", color: "var(--muted)", display: "block", marginBottom: "5px" }}>CEP</label>
                  <input
                    style={INPUT}
                    placeholder="00000-000"
                    value={form.cep}
                    maxLength={9}
                    onChange={e => {
                      const v = e.target.value.replace(/\D/g, "").slice(0, 8)
                      setForm(f => ({ ...f, cep: v }))
                      if (v.length === 8) buscarCep(v, setForm)
                    }}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "11px", color: "var(--muted)", display: "block", marginBottom: "5px" }}>
                    Logradouro {cepLoading && <span style={{ color: "var(--accent)" }}>buscando...</span>}
                  </label>
                  <input style={INPUT} placeholder="Rua, Avenida..." value={form.logradouro} onChange={e => setForm(f => ({ ...f, logradouro: e.target.value }))} />
                </div>
              </div>
              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 0.5fr", gap: "10px", marginBottom: "20px" }}>
                <div>
                  <label style={{ fontSize: "11px", color: "var(--muted)", display: "block", marginBottom: "5px" }}>Bairro</label>
                  <input style={INPUT} placeholder="Bairro" value={form.bairro} onChange={e => setForm(f => ({ ...f, bairro: e.target.value }))} />
                </div>
                <div>
                  <label style={{ fontSize: "11px", color: "var(--muted)", display: "block", marginBottom: "5px" }}>Cidade</label>
                  <input style={INPUT} placeholder="Cidade" value={form.cidade} onChange={e => setForm(f => ({ ...f, cidade: e.target.value }))} />
                </div>
                <div>
                  <label style={{ fontSize: "11px", color: "var(--muted)", display: "block", marginBottom: "5px" }}>UF</label>
                  <input style={INPUT} placeholder="SP" maxLength={2} value={form.estado} onChange={e => setForm(f => ({ ...f, estado: e.target.value.toUpperCase() }))} />
                </div>
              </div>

              {/* Veículos */}
              <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: "12px" }}>
                <div style={{ fontSize: "10px", textTransform: "uppercase", letterSpacing: "1px", color: "var(--muted)" }}>
                  Veículos
                </div>
                <button
                  onClick={addVeiculo}
                  style={{
                    background: "rgba(59,130,246,0.12)", border: "1px solid rgba(59,130,246,0.3)",
                    borderRadius: "6px", padding: "4px 10px", fontSize: "11px", fontWeight: 600,
                    color: "var(--accent)", cursor: "pointer", fontFamily: "'DM Sans', sans-serif"
                  }}
                >
                  + Adicionar Veículo
                </button>
              </div>

              {formVeiculos.length === 0 && (
                <div style={{
                  padding: "14px", textAlign: "center", fontSize: "12px",
                  color: "var(--muted)", background: "var(--card2)",
                  borderRadius: "8px", marginBottom: "8px",
                  border: "1px dashed var(--border)"
                }}>
                  Nenhum veículo — o cliente pode ser cadastrado sem veículo.
                </div>
              )}

              {formVeiculos.map((v, i) => (
                <div key={i} style={{
                  background: "var(--card2)", border: "1px solid var(--border)",
                  borderRadius: "10px", padding: "14px", marginBottom: "10px"
                }}>
                  <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "10px" }}>
                    <span style={{ fontSize: "11px", fontWeight: 600, color: "var(--muted)" }}>Veículo {i + 1}</span>
                    <button
                      onClick={() => removeVeiculo(i)}
                      style={{ background: "none", border: "none", color: "var(--red)", cursor: "pointer", fontSize: "15px", padding: "0 4px" }}
                    >
                      ×
                    </button>
                  </div>
                  <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "8px" }}>
                    <div>
                      <label style={{ fontSize: "10px", color: "var(--muted)", display: "block", marginBottom: "4px" }}>Marca *</label>
                      <input style={INPUT} placeholder="ex: Volkswagen" value={v.marca}
                        onChange={e => updateVeiculo(i, "marca", e.target.value)} />
                    </div>
                    <div>
                      <label style={{ fontSize: "10px", color: "var(--muted)", display: "block", marginBottom: "4px" }}>Modelo *</label>
                      <input style={INPUT} placeholder="ex: Gol" value={v.modelo}
                        onChange={e => updateVeiculo(i, "modelo", e.target.value)} />
                    </div>
                    <div>
                      <label style={{ fontSize: "10px", color: "var(--muted)", display: "block", marginBottom: "4px" }}>Placa *</label>
                      <input style={INPUT} placeholder="ABC1234" value={v.placa}
                        onChange={e => updateVeiculo(i, "placa", e.target.value.toUpperCase())} />
                    </div>
                    <div>
                      <label style={{ fontSize: "10px", color: "var(--muted)", display: "block", marginBottom: "4px" }}>Ano *</label>
                      <input style={INPUT} type="number" placeholder={ANO_ATUAL} value={v.ano}
                        onChange={e => updateVeiculo(i, "ano", e.target.value)} />
                    </div>
                  </div>
                </div>
              ))}

              {/* Error */}
              {formError && (
                <div style={{
                  background: "rgba(239,68,68,0.1)", border: "1px solid rgba(239,68,68,0.3)",
                  borderRadius: "8px", padding: "10px 14px", marginTop: "8px",
                  fontSize: "12px", color: "var(--red)"
                }}>
                  {formError}
                </div>
              )}
            </div>

            {/* Footer */}
            <div style={{
              display: "flex", gap: "10px", padding: "16px 24px",
              borderTop: "1px solid var(--border)", justifyContent: "flex-end"
            }}>
              <button
                className="ds-btn-secondary"
                onClick={() => setShowModal(false)}
                disabled={saving}
              >
                Cancelar
              </button>
              <button
                className="ds-btn-primary"
                onClick={handleSalvar}
                disabled={saving}
                style={{ minWidth: "110px", justifyContent: "center" }}
              >
                {saving ? "Salvando..." : "Salvar Cliente"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default ClientesPage
