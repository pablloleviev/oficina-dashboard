import { useState } from "react"
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom"

import Login                from "./components/Login"
import Toast                from "./components/Toast"
import AppLayout            from "./components/AppLayout"
import ErrorBoundary        from "./components/ErrorBoundary"
import CadastroOficinaPage  from "./pages/CadastroOficinaPage"

import DashboardPage  from "./pages/DashboardPage"
import OrdensPage     from "./pages/OrdensPage"
import ClientesPage   from "./pages/ClientesPage"
import FinanceiroPage from "./pages/FinanceiroPage"
import RelatoriosPage from "./pages/RelatoriosPage"
import AdminPage      from "./pages/AdminPage"

function App() {
  const [token, setToken] = useState(() => localStorage.getItem("token"))
  const [toast, setToast] = useState({ show: false, message: "", type: "success" })

  function showToast(message, type = "success") {
    setToast({ show: true, message, type })
    setTimeout(() => setToast(prev => ({ ...prev, show: false })), 2000)
  }

  function handleLogin() {
    setToken(localStorage.getItem("token"))
  }

  function handleLogout() {
    localStorage.removeItem("token")
    setToken(null)
  }

  function getUserRole() {
    try {
      if (!token) return null
      const payload = JSON.parse(atob(token.split(".")[1]))
      return payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]
    } catch {
      return null
    }
  }

  if (!token) {
    return (
      <BrowserRouter>
        <Routes>
          <Route path="/cadastro" element={<CadastroOficinaPage />} />
          <Route path="*" element={<Login onLogin={handleLogin} />} />
        </Routes>
      </BrowserRouter>
    )
  }

  const role = getUserRole()

  return (
    <BrowserRouter>
      <Toast {...toast} />
      <ErrorBoundary>
        <Routes>
          <Route element={<AppLayout onLogout={handleLogout} role={role} />}>
            <Route index element={<Navigate to="/dashboard" replace />} />
            <Route path="/dashboard"  element={<ErrorBoundary><DashboardPage  showToast={showToast} /></ErrorBoundary>} />
            <Route path="/ordens"     element={<ErrorBoundary><OrdensPage     showToast={showToast} role={role} /></ErrorBoundary>} />
            <Route path="/clientes"   element={<ErrorBoundary><ClientesPage   showToast={showToast} /></ErrorBoundary>} />
            <Route path="/financeiro" element={<ErrorBoundary><FinanceiroPage /></ErrorBoundary>} />
            <Route path="/relatorios" element={<ErrorBoundary><RelatoriosPage /></ErrorBoundary>} />
            {role === "Admin" && (
              <Route path="/admin" element={<ErrorBoundary><AdminPage /></ErrorBoundary>} />
            )}
          </Route>
        </Routes>
      </ErrorBoundary>
    </BrowserRouter>
  )
}

export default App
