import { NavLink } from "react-router-dom"

const navItems = [
  {
    to: "/dashboard",
    label: "Dashboard",
    icon: (
      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/>
        <rect x="3" y="14" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/>
      </svg>
    )
  },
  {
    to: "/clientes",
    label: "Clientes",
    icon: (
      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
        <circle cx="9" cy="7" r="4"/>
        <path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>
      </svg>
    )
  },
  {
    to: "/ordens",
    label: "Ordens",
    icon: (
      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
        <polyline points="14 2 14 8 20 8"/>
        <line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/>
      </svg>
    )
  },
  {
    to: "/financeiro",
    label: "Financeiro",
    icon: (
      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <line x1="12" y1="1" x2="12" y2="23"/>
        <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/>
      </svg>
    )
  },
  {
    to: "/relatorios",
    label: "Relatórios",
    icon: (
      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <line x1="18" y1="20" x2="18" y2="10"/>
        <line x1="12" y1="20" x2="12" y2="4"/>
        <line x1="6" y1="20" x2="6" y2="14"/>
      </svg>
    )
  }
]

function normalizeRole(role) {
  if (!role) return null
  if (Array.isArray(role)) return role[0] ?? null
  if (typeof role === "string") return role
  return String(role)
}

function hasRole(role, target) {
  if (!role) return false
  if (Array.isArray(role)) return role.includes(target)
  return role === target
}

function AppSidebar({ onLogout, role }) {
  const roleLabel = normalizeRole(role)
  return (
    <aside style={{
      width: "200px", minWidth: "200px",
      background: "var(--sidebar)",
      borderRight: "1px solid var(--border)",
      display: "flex", flexDirection: "column",
      padding: "24px 16px",
      position: "fixed", top: 0, left: 0, bottom: 0,
      zIndex: 10
    }}>
      {/* LOGO */}
      <div style={{ marginBottom: "4px" }}>
        <h1 style={{
          fontFamily: "'Syne', sans-serif", fontSize: "20px",
          fontWeight: 800, color: "var(--text)", letterSpacing: "-0.3px"
        }}>
          AutoFlow
        </h1>
        <p style={{
          fontSize: "10px", color: "var(--muted)",
          textTransform: "uppercase", letterSpacing: "1px", marginTop: "2px"
        }}>
          Gestão de Serviços
        </p>
      </div>

      {/* SECTION LABEL */}
      <div style={{
        fontSize: "9px", color: "var(--muted)",
        textTransform: "uppercase", letterSpacing: "1.5px",
        margin: "24px 0 10px", paddingLeft: "4px",
        fontFamily: "'DM Sans', sans-serif"
      }}>
        Menu
      </div>

      {/* NAV */}
      <nav style={{ display: "flex", flexDirection: "column", gap: "2px" }}>
        {navItems.map(item => (
          <NavLink
            key={item.to}
            to={item.to}
            style={({ isActive }) => ({
              display: "flex", alignItems: "center", gap: "10px",
              padding: "9px 10px", borderRadius: "8px",
              fontSize: "13px", fontWeight: 400,
              color: isActive ? "var(--text)" : "var(--muted)",
              background: isActive ? "rgba(59,130,246,0.08)" : "transparent",
              borderLeft: isActive ? "2px solid var(--accent)" : "2px solid transparent",
              textDecoration: "none", transition: "all 0.15s",
              fontFamily: "'DM Sans', sans-serif"
            })}
          >
            {item.icon}
            {item.label}
          </NavLink>
        ))}

        {hasRole(role, "Admin") && (
          <NavLink
            to="/admin"
            style={({ isActive }) => ({
              display: "flex", alignItems: "center", gap: "10px",
              padding: "9px 10px", borderRadius: "8px",
              fontSize: "13px", fontWeight: 400,
              color: isActive ? "var(--text)" : "var(--muted)",
              background: isActive ? "rgba(59,130,246,0.08)" : "transparent",
              borderLeft: isActive ? "2px solid var(--accent)" : "2px solid transparent",
              textDecoration: "none", transition: "all 0.15s",
              fontFamily: "'DM Sans', sans-serif"
            })}
          >
            ⚙️ Admin
          </NavLink>
        )}
      </nav>

      {/* FOOTER */}
      <div style={{
        marginTop: "auto", paddingTop: "16px",
        borderTop: "1px solid var(--border)"
      }}>
        <div style={{ display: "flex", alignItems: "center", gap: "10px", marginBottom: "10px" }}>
          <div style={{
            width: "30px", height: "30px", borderRadius: "50%",
            background: "linear-gradient(135deg, #3b82f6, #06b6d4)",
            display: "flex", alignItems: "center", justifyContent: "center",
            fontSize: "11px", fontWeight: 700, color: "#fff", flexShrink: 0
          }}>
            AD
          </div>
          <div>
            <p style={{ fontSize: "12px", fontWeight: 500, color: "var(--text)", fontFamily: "'DM Sans', sans-serif" }}>Admin</p>
            <span style={{ fontSize: "10px", color: "var(--muted)", fontFamily: "'DM Sans', sans-serif" }}>
              {roleLabel || "Administrador"}
            </span>
          </div>
        </div>
        <button
          onClick={onLogout}
          style={{
            fontSize: "12px", color: "var(--red)",
            background: "none", border: "none", cursor: "pointer",
            padding: 0, fontFamily: "'DM Sans', sans-serif"
          }}
        >
          Sair
        </button>
      </div>
    </aside>
  )
}

export default AppSidebar
