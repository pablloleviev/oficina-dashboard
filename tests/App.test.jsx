import { render, screen } from "@testing-library/react"
import App from "../src/App"

describe("Sistema da Oficina", () => {

  test("Renderiza título principal", () => {
    render(<App />)
    expect(screen.getByText(/Sistema da Oficina/i)).toBeInTheDocument()
  })

  test("Renderiza botão", () => {
    render(<App />)
    expect(screen.getByText(/Adicionar Serviço/i)).toBeInTheDocument()
  })

  test("Renderiza campo de busca", () => {
    render(<App />)
    expect(screen.getByPlaceholderText(/Buscar cliente/i)).toBeInTheDocument()
  })

})