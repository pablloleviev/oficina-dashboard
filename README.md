<div align="center">

# 🔧 AutoFlow
### Sistema Inteligente de Gestão de Oficinas Mecânicas

<p align="center">
  <em>Uma plataforma <strong>full-stack</strong> para digitalizar e otimizar o fluxo operacional de oficinas mecânicas — do cadastro do cliente até a ordem de serviço concluída.</em>
</p>

<br/>

[![Deploy](https://img.shields.io/badge/🚀_ACESSAR_DEMO-Live_em_Produção-0B2545?style=for-the-badge&labelColor=000)](https://autoflow-gestao.vercel.app/)
[![Status](https://img.shields.io/badge/status-em_desenvolvimento-success?style=for-the-badge)](https://github.com/pablloleviev/oficina-dashboard)
[![License](https://img.shields.io/badge/license-MIT-blue?style=for-the-badge)](LICENSE)

<br/>

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-11.0-239120?style=flat-square&logo=csharp&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-8.0-512BD4?style=flat-square&logo=.net&logoColor=white)
![React](https://img.shields.io/badge/React-18.2-61DAFB?style=flat-square&logo=react&logoColor=black)
![Vite](https://img.shields.io/badge/Vite-5.0-646CFF?style=flat-square&logo=vite&logoColor=white)
![TailwindCSS](https://img.shields.io/badge/Tailwind_CSS-3.4-38B2AC?style=flat-square&logo=tailwind-css&logoColor=white)
![xUnit](https://img.shields.io/badge/xUnit-Tests-512BD4?style=flat-square&logo=xunit&logoColor=white)
![Vitest](https://img.shields.io/badge/Vitest-Tests-6E9F18?style=flat-square&logo=vitest&logoColor=white)
![Vercel](https://img.shields.io/badge/Vercel-Deployed-000000?style=flat-square&logo=vercel&logoColor=white)
[![CI](https://img.shields.io/badge/CI/CD-GitHub_Actions-2088FF?style=flat-square&logo=github-actions&logoColor=white)](https://github.com/pablloleviev/oficina-dashboard/actions)

</div>

---

## 🌐 Acesso ao Produto

> ### 🔗 **[https://autoflow-gestao.vercel.app/](https://autoflow-gestao.vercel.app/)**
>
> A aplicação está publicada em produção na **Vercel**, com deploy contínuo a cada merge na branch `main`.  
> Não precisa instalar nada — basta abrir o link e começar a usar.

---

## 📑 Sumário

- [🎯 O Problema](#-o-problema)
- [💡 A Solução](#-a-solução)
- [👥 Público-Alvo](#-público-alvo)
- [✨ Funcionalidades](#-funcionalidades)
- [🏛️ Arquitetura do Sistema](#️-arquitetura-do-sistema)
- [🧰 Stack Tecnológica](#-stack-tecnológica)
- [🌍 Integração com Serviços Externos](#-integração-com-serviços-externos)
- [🧪 Qualidade e Testes](#-qualidade-e-testes)
- [⚡ CI/CD](#-cicd)
- [📦 Como Executar Localmente](#-como-executar-localmente)
- [📁 Estrutura do Repositório](#-estrutura-do-repositório)
- [🗺️ Roadmap](#️-roadmap)
- [👤 Autor](#-autor)

---

## 🎯 O Problema

Pequenas e médias oficinas mecânicas ainda operam de forma analógica: cadernos, planilhas soltas e anotações de balcão.  
Esse cenário produz uma série de dores recorrentes:

- ❌ Perda de histórico de serviços e peças
- ❌ Dificuldade em localizar rapidamente dados do cliente ou veículo
- ❌ Falta de visibilidade sobre ordens em andamento
- ❌ Retrabalho e atraso nas entregas
- ❌ Ausência de métricas para tomada de decisão

## 💡 A Solução

**AutoFlow** é uma aplicação web **full-stack** que centraliza, organiza e automatiza o fluxo operacional da oficina — do primeiro contato com o cliente até a entrega final do veículo.

Com uma interface limpa, responsiva e inspirada em sistemas modernos de ERP, o AutoFlow entrega **controle, rastreabilidade e inteligência de dados** ao pequeno empreendedor.

## 👥 Público-Alvo

| Perfil | Benefício |
|---|---|
| 🔩 **Donos de oficinas** | Gestão completa do negócio em uma única tela |
| 🛠️ **Mecânicos autônomos** | Organização profissional de clientes e serviços |
| 🚗 **Centros automotivos** | Fluxo padronizado de ordens de serviço |
| 📊 **Gerentes operacionais** | Dashboard com métricas em tempo real |

---

## ✨ Funcionalidades

- 👤 **Cadastro de Clientes** — com preenchimento automático de endereço via CEP
- 🚙 **Cadastro de Veículos** — vinculado ao cliente (relação 1:N)
- 📋 **Ordens de Serviço** — criação, edição, acompanhamento e exclusão
- 🔍 **Busca inteligente** por cliente, placa ou serviço
- 🏷️ **Filtros de status** — `Concluído`, `Em Andamento`, `Aguardando`
- 📈 **Dashboard gerencial** com KPIs e métricas operacionais
- 📊 **Visualização em tabela** com ordenação e paginação
- 📱 **Design responsivo** — funcional em desktop, tablet e mobile

---

## 🏛️ Arquitetura do Sistema

O AutoFlow adota uma arquitetura **desacoplada e em camadas**, separando claramente a camada de apresentação (frontend) da camada de domínio e persistência (backend).

```
┌────────────────────────────────┐       ┌────────────────────────────────┐       ┌────────────────────────┐
│                                │       │                                │       │                        │
│   🎨  FRONTEND (React SPA)     │◄─────►│   ⚙️  BACKEND (.NET 8 API)     │◄─────►│  🗄️  BANCO DE DADOS    │
│                                │  HTTP │                                │  EF   │    (SQL / SQLite)      │
│   • React 18 + Vite            │ REST  │   • ASP.NET Core Web API       │ Core  │                        │
│   • Tailwind CSS               │ JSON  │   • Entity Framework Core      │       │                        │
│   • Fetch API                  │       │   • Injeção de Dependência     │       │                        │
│   • Vitest                     │       │   • DTOs + Mapeamento          │       │                        │
│                                │       │   • xUnit                      │       │                        │
└────────────┬───────────────────┘       └────────────────────────────────┘       └────────────────────────┘
             │
             │  HTTP GET
             ▼
    ┌────────────────────┐
    │  🌍 API ViaCEP     │
    │  (serviço público) │
    └────────────────────┘
```

### 🎨 Frontend — React + Tailwind

- **React 18** com componentes funcionais e hooks
- **Tailwind CSS** (utility-first) para estilização consistente
- Camada de serviços HTTP centralizada
- Estado gerenciado via `useState`, `useEffect` e *custom hooks*

### ⚙️ Backend — .NET 8

- **ASP.NET Core Web API** seguindo padrão **RESTful**
- **Entity Framework Core** como ORM (mapeamento objeto-relacional)
- **Injeção de Dependência** nativa (`IServiceCollection`) com escopos adequados
- **DTOs** (Data Transfer Objects) para isolar a camada de domínio das alterações contratuais
- **Migrations** automatizadas para versionamento do banco
- Relações **1:N** modeladas via Fluent API (ex.: `Cliente → Veículos`, `OrdemDeServiço → ItensOS`)

---

## 🧰 Stack Tecnológica

### Frontend
| Tecnologia | Função |
|---|---|
| ![React](https://img.shields.io/badge/-React-61DAFB?style=flat&logo=react&logoColor=black) | Biblioteca de interface |
| ![Vite](https://img.shields.io/badge/-Vite-646CFF?style=flat&logo=vite&logoColor=white) | Build tool e dev server |
| ![Tailwind](https://img.shields.io/badge/-Tailwind-38B2AC?style=flat&logo=tailwind-css&logoColor=white) | Framework CSS utility-first |
| ![Vitest](https://img.shields.io/badge/-Vitest-6E9F18?style=flat&logo=vitest&logoColor=white) | Framework de testes |
| ![ESLint](https://img.shields.io/badge/-ESLint-4B32C3?style=flat&logo=eslint&logoColor=white) | Análise estática de código |

### Backend
| Tecnologia | Função |
|---|---|
| ![.NET](https://img.shields.io/badge/-.NET_8-512BD4?style=flat&logo=dotnet&logoColor=white) | Plataforma de execução |
| ![C#](https://img.shields.io/badge/-C%23-239120?style=flat&logo=csharp&logoColor=white) | Linguagem principal |
| ![EF Core](https://img.shields.io/badge/-EF_Core-512BD4?style=flat&logo=.net&logoColor=white) | ORM / persistência |
| ![xUnit](https://img.shields.io/badge/-xUnit-512BD4?style=flat&logo=xunit&logoColor=white) | Testes de unidade e integração |

### DevOps
| Tecnologia | Função |
|---|---|
| ![GitHub Actions](https://img.shields.io/badge/-GitHub_Actions-2088FF?style=flat&logo=github-actions&logoColor=white) | Pipeline de CI/CD |
| ![Vercel](https://img.shields.io/badge/-Vercel-000000?style=flat&logo=vercel&logoColor=white) | Hospedagem em produção |
| ![Git](https://img.shields.io/badge/-Git-F05032?style=flat&logo=git&logoColor=white) | Controle de versão |

---

## 🌍 Integração com Serviços Externos

> 💎 **Destaque da Etapa 2 — Consumo de API Pública**

O AutoFlow integra-se ao serviço público **[ViaCEP](https://viacep.com.br/)** para preencher automaticamente os campos de endereço a partir do CEP informado pelo usuário.

### 🔄 Fluxo da integração

```
  [ Usuário digita o CEP ]
            │
            ▼
  ┌────────────────────────┐
  │  onBlur do campo CEP   │
  └────────────┬───────────┘
               │
               ▼
   HTTP GET → https://viacep.com.br/ws/{cep}/json/
               │
               ▼
  ┌────────────────────────┐
  │  Resposta JSON          │
  │  desserializada em DTO  │
  └────────────┬───────────┘
               │
               ▼
  [ Campos preenchidos automaticamente:
    logradouro, bairro, cidade, UF ]
```

### ✅ Benefícios técnicos

- **UX aprimorada** — menos cliques, menos digitação
- **Integridade dos dados** — valores vindos de base oficial dos Correios
- **Tratamento de erros** — fallback para entrada manual em caso de CEP inválido
- **Resiliência** — `try/catch` e validação de schema no retorno JSON

---

## 🧪 Qualidade e Testes

A qualidade do código é garantida por uma estratégia de testes em **ambas as camadas** da aplicação.

| Camada | Framework | Escopo |
|---|---|---|
| 🎨 Frontend | **Vitest** | Componentes, hooks e serviços HTTP |
| ⚙️ Backend | **xUnit** | Controllers, services e integração com APIs externas |

### Cenários de teste cobertos

- ✅ Requisição bem-sucedida ao ViaCEP com CEP válido
- ✅ Tratamento de CEP inválido (resposta `{ "erro": true }`)
- ✅ Mock de falha de rede para validação de resiliência
- ✅ Validação do contrato da interface (schema do JSON)
- ✅ Persistência correta via EF Core
- ✅ Renderização de componentes críticos

```bash
# Rodar testes do frontend
npm run test

# Rodar testes do backend
dotnet test
```

---

## ⚡ CI/CD

![CI](https://img.shields.io/badge/build-passing-brightgreen?style=for-the-badge&logo=github-actions&logoColor=white)

O repositório conta com um *pipeline* de **Integração e Deploy Contínuos** via **GitHub Actions**, executado a cada `push` e `pull request`:

```
push / PR  ─►  📥 Install deps  ─►  🔍 Lint  ─►  🧪 Testes  ─►  🏗️ Build  ─►  🚀 Deploy (Vercel)
```

- 🟢 **Status atual:** `passing`
- 🔄 **Deploy atômico** com possibilidade de *rollback* instantâneo
- 🌐 **CDN global** com HTTPS habilitado por padrão

---

## 📦 Como Executar Localmente

### 📋 Pré-requisitos

- [Node.js 18+](https://nodejs.org/)
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Git](https://git-scm.com/)

### 1️⃣ Clone o repositório

```bash
git clone https://github.com/pablloleviev/oficina-dashboard.git
cd oficina-dashboard
```

### 2️⃣ Backend (.NET 8)

```bash
cd backend
dotnet restore             # instala dependências
dotnet ef database update  # aplica as migrations
dotnet run                 # inicia a API
```

A API estará disponível em `http://localhost:5000` (ou porta configurada em `launchSettings.json`).

### 3️⃣ Frontend (React + Vite)

Em outro terminal:

```bash
cd frontend
npm install   # instala dependências
npm run dev   # inicia o servidor de desenvolvimento
```

A aplicação estará disponível em `http://localhost:5173`.

### 4️⃣ Variáveis de ambiente

Crie um arquivo `.env` na raiz do frontend com o endpoint do backend:

```env
VITE_API_URL=http://localhost:5000/api
```

---

## 📁 Estrutura do Repositório

```
oficina-dashboard/
├── 📁 backend/                    # API .NET 8
│   ├── Controllers/               # Endpoints REST
│   ├── Services/                  # Regras de negócio
│   ├── Models/                    # Entidades de domínio
│   ├── DTOs/                      # Data Transfer Objects
│   ├── Data/                      # DbContext + Migrations
│   └── Tests/                     # Testes com xUnit
│
├── 📁 frontend/                   # SPA React
│   ├── src/
│   │   ├── components/            # Componentes reutilizáveis
│   │   ├── pages/                 # Rotas principais
│   │   ├── services/              # Camada HTTP (fetch + ViaCEP)
│   │   ├── hooks/                 # Custom hooks
│   │   └── tests/                 # Testes com Vitest
│   └── tailwind.config.js
│
├── 📁 .github/workflows/          # Pipelines de CI/CD
├── 📄 README.md                   # Este arquivo
└── 📄 LICENSE
```

---

## 🗺️ Roadmap

- [x] **Etapa 1** — Protótipo em React + JSON Server
- [x] **Etapa 2** — Migração para backend .NET 8 + integração ViaCEP + deploy
- [ ] **Etapa 3** — Autenticação JWT e controle de perfis (admin/mecânico)
- [ ] **Etapa 4** — Emissão de PDF de ordens de serviço
- [ ] **Etapa 5** — Notificações por e-mail e dashboard avançado com gráficos

---

## 👤 Autor

<div align="center">

**Pabllo Batista Morais da Costa**

*Desenvolvedor Full-Stack*

[![GitHub](https://img.shields.io/badge/GitHub-pablloleviev-181717?style=for-the-badge&logo=github)](https://github.com/pablloleviev)

📚 *Projeto desenvolvido no Bootcamp II — Turma B — Semestre 2026/1*  
🏫 *Centro Universitário de Brasília (UniCEUB) — Campus Taguatinga*

</div>

---

<div align="center">

### ⭐ Se este projeto te ajudou ou te inspirou, deixe uma estrela no repositório!

**Feito com 💙 e muito ☕ em Brasília/DF**

</div>
