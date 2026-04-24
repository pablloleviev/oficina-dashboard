function MetricCard({titulo, valor, cor}){

return(

<div style={{
background:"#1e293b",
padding:"20px",
borderRadius:"10px",
width:"180px"
}}>

<h4 style={{color:"#94a3b8"}}>{titulo}</h4>

<h2 style={{color:cor}}>{valor}</h2>

</div>

)

}

export default MetricCard