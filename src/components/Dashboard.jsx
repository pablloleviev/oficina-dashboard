import { Pie } from "react-chartjs-2"
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from "chart.js"

ChartJS.register(ArcElement, Tooltip, Legend)

function Dashboard({ servicos }) {

const concluidos =
servicos.filter(s => s.status === "Concluído").length

const andamento =
servicos.filter(s => s.status === "Em andamento").length

const aguardando =
servicos.filter(s => s.status === "Aguardando").length

const data = {

labels: ["Concluídos", "Em andamento", "Aguardando"],

datasets: [
{
data: [concluidos, andamento, aguardando],

backgroundColor: [
"#2ecc71",
"#f1c40f",
"#e74c3c"
]

}
]

}

return (

<div style={{
width: "350px",
marginBottom: "30px"
}}>

<h2>Dashboard</h2>

<Pie data={data} />

</div>

)

}

export default Dashboard