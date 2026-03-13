function ServiceTable({ servicos = [], onDelete, onEdit, onOrdenar }) {

if (servicos.length === 0) {
return <p style={{color:"white"}}>Nenhum serviço encontrado.</p>
}

return (

<div style={{overflowX:"auto"}}>

<table style={tableStyle}>

<thead>

<tr style={{background:"#334155"}}>

<th style={th} onClick={()=>onOrdenar("cliente")}>
Cliente
</th>

<th style={th} onClick={()=>onOrdenar("servico")}>
Serviço
</th>

<th style={th} onClick={()=>onOrdenar("valor")}>
Valor
</th>

<th style={th} onClick={()=>onOrdenar("status")}>
Status
</th>

<th style={th}>
Ações
</th>

</tr>

</thead>

<tbody>

{servicos.map((servico) => (

<tr key={servico.id} style={{textAlign:"center"}}>

<td style={td}>{servico.cliente}</td>

<td style={td}>{servico.servico}</td>

<td style={td}>R$ {servico.valor}</td>

<td style={td}>{servico.status}</td>

<td style={td}>

<button
onClick={() => onEdit(servico)}
style={btnEditar}
>
Editar
</button>

<button
onClick={() => onDelete(servico.id)}
style={btnExcluir}
>
Excluir
</button>

</td>

</tr>

))}

</tbody>

</table>

</div>

)

}

const tableStyle = {
width: "100%",
borderCollapse: "collapse",
background: "#1e293b",
borderRadius: "10px",
overflow: "hidden",
boxShadow: "0 4px 20px rgba(0,0,0,0.4)"
}

const th = {
padding: "14px",
color: "white",
cursor: "pointer",
fontWeight: "600"
}

const td = {
padding: "12px",
color: "#e2e8f0"
}

const btnEditar = {
background:"#3b82f6",
border:"none",
padding:"6px 12px",
marginRight:"6px",
borderRadius:"6px",
color:"white",
cursor:"pointer"
}

const btnExcluir = {
background:"#ef4444",
border:"none",
padding:"6px 12px",
borderRadius:"6px",
color:"white",
cursor:"pointer"
}

export default ServiceTable