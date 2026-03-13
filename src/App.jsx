import { useState, useEffect } from "react"
import Dashboard from "./components/Dashboard"
import Sidebar from "./components/Sidebar"
import MetricCard from "./components/MetricCard"
import ServiceTable from "./components/ServiceTable"

function App() {

const [servicos, setServicos] = useState([])

const [cliente, setCliente] = useState("")
const [servicoInput, setServicoInput] = useState("")
const [valor, setValor] = useState("")
const [status, setStatus] = useState("")
const [editIndex, setEditIndex] = useState(null)

const [filtro, setFiltro] = useState("Todos")
const [busca, setBusca] = useState("")
const [ordenarPor, setOrdenarPor] = useState("cliente")
const [ordemAsc, setOrdemAsc] = useState(true)

// ============================
// BUSCAR SERVIÇOS
// ============================

useEffect(() => {

fetch("http://localhost:3001/servicos")
.then(res => res.json())
.then(data => setServicos(data))
.catch(() => alert("Erro ao carregar serviços"))

}, [])

// ============================
// EXCLUIR
// ============================

function excluirServico(id) {

fetch(`http://localhost:3001/servicos/${id}`, {
method: "DELETE"
})

setServicos(servicos.filter(s => s.id !== id))

}

// ============================
// EDITAR
// ============================

function editarServico(servico) {

setCliente(servico.cliente)
setServicoInput(servico.servico)
setValor(servico.valor)
setStatus(servico.status)

setEditIndex(servico.id)

}

// ============================
// SALVAR
// ============================

function salvarServico() {

if(!cliente || !servicoInput || !valor || !status){
alert("Preencha todos os campos")
return
}

const novoServico = {
cliente,
servico: servicoInput,
valor,
status
}

if (editIndex) {

fetch(`http://localhost:3001/servicos/${editIndex}`, {

method:"PUT",

headers:{
"Content-Type":"application/json"
},

body:JSON.stringify(novoServico)

})
.then(res=>res.json())
.then(data=>{

setServicos(servicos.map(s =>
s.id === editIndex ? data : s
))

setEditIndex(null)

})

} else {

fetch("http://localhost:3001/servicos",{

method:"POST",

headers:{
"Content-Type":"application/json"
},

body:JSON.stringify(novoServico)

})
.then(res=>res.json())
.then(data=>{

setServicos([...servicos,data])

})

}

setCliente("")
setServicoInput("")
setValor("")
setStatus("")

}

// ============================
// FILTROS
// ============================

const servicosFiltrados =
servicos
.filter(s =>
filtro === "Todos" || s.status === filtro
)
.filter(s =>
s.cliente.toLowerCase().includes(busca.toLowerCase())
)

// ============================
// MÉTRICAS
// ============================

const total = servicosFiltrados.length

const concluidos =
servicosFiltrados.filter(s => s.status === "Concluído").length

const andamento =
servicosFiltrados.filter(s => s.status === "Em andamento").length

const aguardando =
servicosFiltrados.filter(s => s.status === "Aguardando").length

const servicosOrdenados = [...servicosFiltrados].sort((a, b) => {

if (ordenarPor === "valor") {
return ordemAsc
? Number(a.valor) - Number(b.valor)
: Number(b.valor) - Number(a.valor)
}

return ordemAsc
? a[ordenarPor].localeCompare(b[ordenarPor])
: b[ordenarPor].localeCompare(a[ordenarPor])

})

return (

<>
<Sidebar/>

<div style={{
marginLeft:"240px",
padding:"40px",
fontFamily:"Arial",
background:"#0f172a",
color:"white",
minHeight:"100vh"
}}>

<h1>Sistema da Oficina</h1>

{/* CARDS DE MÉTRICAS */}

<div style={{
display:"flex",
gap:"20px",
marginBottom:"30px"
}}>

<MetricCard titulo="Total" valor={total} cor="white"/>
<MetricCard titulo="Concluídos" valor={concluidos} cor="green"/>
<MetricCard titulo="Em andamento" valor={andamento} cor="orange"/>
<MetricCard titulo="Aguardando" valor={aguardando} cor="red"/>

</div>

{/* DASHBOARD */}

<Dashboard servicos={servicosFiltrados} />

{/* BUSCA */}

<input
placeholder="Buscar cliente"
value={busca}
onChange={(e)=>setBusca(e.target.value)}
style={{
marginTop:"20px",
marginBottom:"20px",
background:"#1e293b",
color:"white",
border:"1px solid #334155",
padding:"8px",
borderRadius:"6px"
}}
/>

{/* FORMULÁRIO */}

<div style={{
marginBottom:"20px",
display:"flex",
gap:"10px",
flexWrap:"wrap"
}}>

<input
placeholder="Cliente"
value={cliente}
onChange={(e)=>setCliente(e.target.value)}
style={inputStyle}
/>

<input
placeholder="Serviço"
value={servicoInput}
onChange={(e)=>setServicoInput(e.target.value)}
style={inputStyle}
/>

<input
type="number"
placeholder="Valor"
value={valor}
onChange={(e)=>setValor(e.target.value)}
style={inputStyle}
/>

<select
value={status}
onChange={(e)=>setStatus(e.target.value)}
style={inputStyle}
>

<option value="">Status</option>
<option value="Concluído">Concluído</option>
<option value="Em andamento">Em andamento</option>
<option value="Aguardando">Aguardando</option>

</select>

<button onClick={salvarServico} style={btnPrincipal}>
{editIndex ? "Salvar Alteração" : "Adicionar Serviço"}
</button>

</div>

{/* FILTROS */}

<div style={{marginBottom:"20px"}}>

<button onClick={()=>setFiltro("Todos")} style={btnFiltro}>Todos</button>

<button onClick={()=>setFiltro("Concluído")} style={btnFiltro}>
Concluídos
</button>

<button onClick={()=>setFiltro("Em andamento")} style={btnFiltro}>
Em andamento
</button>

<button onClick={()=>setFiltro("Aguardando")} style={btnFiltro}>
Aguardando
</button>

</div>

{/* TABELA */}

<ServiceTable
servicos={servicosOrdenados}
onDelete={excluirServico}
onEdit={editarServico}
onOrdenar={(campo)=>{
if(ordenarPor === campo){
setOrdemAsc(!ordemAsc)
}else{
setOrdenarPor(campo)
setOrdemAsc(true)
}
}}
/>

</div>
</>

)

}

const inputStyle = {
background:"#1e293b",
color:"white",
border:"1px solid #334155",
padding:"8px",
borderRadius:"6px"
}

const btnFiltro = {
background:"#334155",
color:"white",
border:"none",
padding:"6px 12px",
marginRight:"6px",
borderRadius:"6px",
cursor:"pointer"
}

const btnPrincipal = {
background:"#2563eb",
color:"white",
border:"none",
padding:"8px 14px",
borderRadius:"6px",
cursor:"pointer"
}

export default App