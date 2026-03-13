function ServiceCard({ cliente, servico, valor, status, onDelete, onEdit }) {

return (

<div style={{
border:"1px solid #ccc",
padding:"15px",
margin:"10px",
borderRadius:"8px",
backgroundColor:"#f9f9f9",
width:"250px"
}}>

<h3>Cliente: {cliente}</h3>

<p>Serviço: {servico}</p>

<p>Valor: R$ {valor}</p>

<p>Status: {status}</p>

<button
onClick={onDelete}
style={{
marginTop:"10px",
backgroundColor:"#ff4d4d",
color:"white",
border:"none",
padding:"8px",
cursor:"pointer",
borderRadius:"5px"
}}
>
Excluir
</button>

<button
onClick={onEdit}
style={{
marginTop:"10px",
marginLeft:"10px",
backgroundColor:"#3498db",
color:"white",
border:"none",
padding:"8px",
cursor:"pointer",
borderRadius:"5px"
}}
>
Editar
</button>

</div>

)

}

export default ServiceCard