import { useState, useEffect } from "react"
import { useNavigate } from "react-router-dom"

const BASE_URL = import.meta.env.VITE_API_URL || "https://autoflow-api-p4tv.onrender.com/api"

function CadastroOficinaPage() {
  const navigate = useNavigate()

  const [etapa, setEtapa] = useState(1)
  const [planos, setPlanos] = useState([])
  const [loadingPlanos, setLoadingPlanos] = useState(false)
  const [loading, setLoading] = useState(false)
  const [erro, setErro] = useState("")
  const [sucesso, setSucesso] = useState(false)

  const [form, setForm] = useState({
    nome: "",
    slug: "",
    cnpj: "",
    email: "",
    telefone: "",
    planoId: null,
  })

  useEffect(() => {
    if (etapa === 2) {
      setLoadingPlanos(true)
      fetch(`${BASE_URL}/oficinas/planos`)
        .then((r) => r.json())
        .then((d) => setPlanos(Array.isArray(d?.data) ? d.data : Array.isArray(d) ? d : []))
        .catch(() => setPlanos([]))
        .finally(() => setLoadingPlanos(false))
    }
  }, [etapa])

  function set(field, value) {
    setForm((prev) => ({ ...prev, [field]: value }))
  }

  function slugify(value) {
    return value
      .toLowerCase()
      .normalize("NFD")
      .replace(/[̀-ͯ]/g, "")
      .replace(/[^a-z0-9-]/g, "-")
      .replace(/-+/g, "-")
      .replace(/^-|-$/g, "")
  }

  function handleNomeChange(e) {
    const nome = e.target.value
    set("nome", nome)
    if (!form.slug || form.slug === slugify(form.nome)) {
      set("slug", slugify(nome))
    }
  }

  function avancar() {
    setErro("")
    if (!form.nome.trim()) return setErro("Informe o nome da oficina")
    if (!form.slug.trim()) return setErro("Informe o slug")
    if (/\s/.test(form.slug)) return setErro("Slug não pode ter espaços")
    if (!form.email.trim()) return setErro("Informe o email")
    if (!form.telefone.trim()) return setErro("Informe o telefone")
    setEtapa(2)
  }

  async function handleSubmit() {
    setErro("")
    if (!form.planoId) return setErro("Selecione um plano")

    setLoading(true)
    try {
      const res = await fetch(`${BASE_URL}/oficinas`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          nome: form.nome.trim(),
          slug: form.slug.trim(),
          cnpj: form.cnpj.trim() || null,
          email: form.email.trim(),
          telefone: form.telefone.trim(),
          planoId: form.planoId,
        }),
      })
      const data = await res.json().catch(() => ({}))
      if (!res.ok) throw new Error(data?.message || `Erro ${res.status}`)
      setSucesso(true)
    } catch (err) {
      setErro(err.message || "Erro ao cadastrar oficina")
    } finally {
      setLoading(false)
    }
  }

  if (sucesso) {
    return (
      <div className="h-screen flex items-center justify-center bg-slate-900 text-white">
        <div className="bg-slate-800 p-8 rounded-xl w-96 text-center">
          <div className="text-4xl mb-4">🎉</div>
          <h2 className="text-xl font-bold mb-2">Cadastro realizado!</h2>
          <p className="text-slate-300 text-sm mb-6">
            Seu trial de 14 dias começou. Faça login para acessar o sistema.
          </p>
          <button
            onClick={() => navigate("/")}
            className="bg-blue-600 hover:bg-blue-700 w-full py-2 rounded-lg font-medium transition-colors"
          >
            Fazer login
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-900 text-white py-10">
      <div className="bg-slate-800 p-8 rounded-xl w-full max-w-md">
        <h2 className="text-xl font-bold mb-1 text-center">Cadastrar Oficina</h2>
        <p className="text-slate-400 text-sm text-center mb-6">
          Etapa {etapa} de 2 — {etapa === 1 ? "Dados da oficina" : "Escolha do plano"}
        </p>

        <div className="flex gap-2 mb-6">
          {[1, 2].map((n) => (
            <div
              key={n}
              className={`h-1 flex-1 rounded-full transition-colors ${
                n <= etapa ? "bg-blue-500" : "bg-slate-600"
              }`}
            />
          ))}
        </div>

        {erro && <p className="text-red-400 text-sm mb-4">{erro}</p>}

        {etapa === 1 && (
          <div className="flex flex-col gap-3">
            <input
              type="text"
              placeholder="Nome da oficina *"
              className="input w-full"
              value={form.nome}
              onChange={handleNomeChange}
            />
            <div>
              <input
                type="text"
                placeholder="Slug (ex: minha-oficina) *"
                className="input w-full"
                value={form.slug}
                onChange={(e) => set("slug", e.target.value.replace(/\s/g, "-"))}
              />
              <p className="text-slate-500 text-xs mt-1 ml-1">
                Usado na URL: autoflow.app/<span className="text-slate-300">{form.slug || "seu-slug"}</span>
              </p>
            </div>
            <input
              type="text"
              placeholder="CNPJ (opcional)"
              className="input w-full"
              value={form.cnpj}
              onChange={(e) => set("cnpj", e.target.value)}
            />
            <input
              type="email"
              placeholder="Email *"
              className="input w-full"
              value={form.email}
              onChange={(e) => set("email", e.target.value)}
            />
            <input
              type="tel"
              placeholder="Telefone *"
              className="input w-full"
              value={form.telefone}
              onChange={(e) => set("telefone", e.target.value)}
            />
            <button
              onClick={avancar}
              className="bg-blue-600 hover:bg-blue-700 w-full py-2 rounded-lg font-medium mt-1 transition-colors"
            >
              Próximo →
            </button>
          </div>
        )}

        {etapa === 2 && (
          <div>
            {loadingPlanos ? (
              <p className="text-slate-400 text-sm text-center py-6">Carregando planos...</p>
            ) : planos.length === 0 ? (
              <p className="text-slate-400 text-sm text-center py-6">Nenhum plano disponível</p>
            ) : (
              <div className="flex flex-col gap-3 mb-4">
                {planos.map((plano) => (
                  <button
                    key={plano.id}
                    onClick={() => set("planoId", plano.id)}
                    className={`w-full text-left p-4 rounded-lg border transition-colors ${
                      form.planoId === plano.id
                        ? "border-blue-500 bg-blue-600/20"
                        : "border-slate-600 bg-slate-700 hover:border-slate-400"
                    }`}
                  >
                    <div className="font-semibold">{plano.nome}</div>
                    <div className="text-slate-300 text-sm mt-0.5">
                      R$ {Number(plano.preco ?? plano.valor ?? 0).toFixed(2).replace(".", ",")}
                      /mês
                    </div>
                    {(plano.limiteOrdens || plano.limite) && (
                      <div className="text-slate-400 text-xs mt-0.5">
                        Até {plano.limiteOrdens ?? plano.limite} ordens/mês
                      </div>
                    )}
                  </button>
                ))}
              </div>
            )}

            <div className="flex gap-2">
              <button
                onClick={() => { setEtapa(1); setErro("") }}
                className="flex-1 py-2 rounded-lg border border-slate-600 hover:border-slate-400 transition-colors"
              >
                ← Voltar
              </button>
              <button
                onClick={handleSubmit}
                disabled={loading || !form.planoId}
                className="flex-1 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 py-2 rounded-lg font-medium transition-colors"
              >
                {loading ? "Cadastrando..." : "Cadastrar"}
              </button>
            </div>
          </div>
        )}

        <p className="text-slate-500 text-xs text-center mt-6">
          Já tem conta?{" "}
          <a href="/" className="text-blue-400 hover:underline">
            Fazer login
          </a>
        </p>
      </div>
    </div>
  )
}

export default CadastroOficinaPage
