import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { login } from "../services/api"


function Login({ onLogin }) {
  const navigate = useNavigate()

  const [email, setEmail] = useState("")
  const [senha, setSenha] = useState("")
  const [erro, setErro] = useState("")
  const [loading, setLoading] = useState(false)


  async function handleLogin() {


    if (!email || !senha) {
      return setErro("Preencha todos os campos")
    }


    setLoading(true)
    setErro("")


    try {
      const data = await login(email.trim(), senha.trim())
      localStorage.setItem("token", data.token)


      onLogin()


    } catch (err) {
      console.error(err)
      setErro("Email ou senha inválidos")
    } finally {
      setLoading(false)
    }
  }


  return (
    <div className="h-screen flex items-center justify-center bg-slate-900 text-white">
      <div className="bg-slate-800 p-8 rounded-xl w-80">


        <h2 className="text-xl font-bold mb-6 text-center">
          Login AutoFlow
        </h2>


        {erro && (
          <p className="text-red-400 text-sm mb-3">
            {erro}
          </p>
        )}


        <input
          type="email"
          placeholder="Email"
          className="input w-full mb-3"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />


        <input
          type="password"
          placeholder="Senha"
          className="input w-full mb-4"
          value={senha}
          onChange={(e) => setSenha(e.target.value)}
        />


        <button
          onClick={handleLogin}
          className="bg-blue-600 w-full py-2 rounded-lg"
          disabled={loading}
        >
          {loading ? "Entrando..." : "Entrar"}
        </button>

        <p className="text-slate-400 text-xs text-center mt-4">
          <button
            onClick={() => navigate("/cadastro")}
            className="text-blue-400 hover:underline"
          >
            Cadastrar minha oficina →
          </button>
        </p>

      </div>
    </div>
  )
}


export default Login

