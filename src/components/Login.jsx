import { useState } from "react";
import { login } from "../services/api";

function Login({ onLogin }) {
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [erro, setErro] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleLogin(e) {
    e?.preventDefault();

    if (loading) return; // evita múltiplos cliques

    if (!email || !senha) {
      setErro("Preencha todos os campos");
      return;
    }

    try {
      setLoading(true);
      setErro("");

      const data = await login(email.trim(), senha.trim());

      // validação defensiva
      if (!data?.token) {
        throw new Error("Token não recebido");
      }

      localStorage.setItem("token", data.token);

      onLogin();

    } catch (err) {
      console.error("Login error:", err);
      setErro(err.message || "Erro ao fazer login");
    } finally {
      setLoading(false);
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

        <form onSubmit={handleLogin}>
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
            type="submit"
            className="bg-blue-600 w-full py-2 rounded-lg"
            disabled={loading}
          >
            {loading ? "Entrando..." : "Entrar"}
          </button>
        </form>

      </div>
    </div>
  );
}

export default Login;