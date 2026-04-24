import { Component } from "react"

class ErrorBoundary extends Component {
  constructor(props) {
    super(props)
    this.state = { error: null }
  }

  static getDerivedStateFromError(error) {
    return { error }
  }

  componentDidCatch(error, info) {
    console.error("[ErrorBoundary]", error, info.componentStack)
  }

  render() {
    if (this.state.error) {
      return (
        <div style={{
          minHeight: "100vh", display: "flex", alignItems: "center", justifyContent: "center",
          background: "var(--bg, #0b1120)", padding: "32px"
        }}>
          <div style={{
            background: "var(--card, #0f1b2d)", border: "1px solid rgba(239,68,68,0.3)",
            borderRadius: "16px", padding: "36px 40px", maxWidth: "480px", width: "100%",
            textAlign: "center", boxShadow: "0 24px 64px rgba(0,0,0,0.5)"
          }}>
            <div style={{ fontSize: "36px", marginBottom: "16px" }}>⚠</div>
            <div style={{ fontFamily: "'Syne', sans-serif", fontSize: "20px", fontWeight: 700, color: "#fff", marginBottom: "10px" }}>
              Algo deu errado
            </div>
            <div style={{ fontSize: "13px", color: "rgba(148,163,184,0.8)", marginBottom: "8px" }}>
              {this.state.error.message}
            </div>
            <div style={{ fontSize: "11px", color: "rgba(148,163,184,0.4)", marginBottom: "28px" }}>
              Verifique o console para mais detalhes.
            </div>
            <button
              onClick={() => this.setState({ error: null })}
              style={{
                background: "#3b82f6", color: "#fff", border: "none", borderRadius: "8px",
                padding: "10px 24px", fontSize: "13px", fontWeight: 600, cursor: "pointer",
                fontFamily: "'DM Sans', sans-serif"
              }}
            >
              Tentar novamente
            </button>
          </div>
        </div>
      )
    }
    return this.props.children
  }
}

export default ErrorBoundary
