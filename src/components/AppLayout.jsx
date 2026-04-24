import { Outlet } from "react-router-dom"
import AppSidebar from "./AppSidebar"

function AppLayout({ onLogout, role }) {
  return (
    <div style={{
      display: "flex",
      minHeight: "100vh",
      background: "var(--bg)",
      fontFamily: "'DM Sans', sans-serif",
      color: "var(--text)"
    }}>
      <AppSidebar onLogout={onLogout} role={role} />
      <main style={{
        marginLeft: "200px",
        flex: 1,
        overflowY: "auto",
        minHeight: "100vh"
      }}>
        <Outlet />
      </main>
    </div>
  )
}

export default AppLayout
