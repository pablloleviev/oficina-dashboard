import { useEffect, useState } from "react";
import { getLogsByOrdem } from "../services/api";

const Timeline = ({ ordemId }) => {
  const [logs, setLogs] = useState([]);

  useEffect(() => {
    const fetchLogs = async () => {
      try {
        const data = await getLogsByOrdem(ordemId);
        setLogs(data || []);
      } catch (err) {
        console.error(err);
      }
    };

    fetchLogs();
  }, [ordemId]);

  return (
    <div className="timeline">
      {logs.length === 0 && <p>Sem histórico</p>}

      {logs.map((log, index) => (
        <div key={index} className="timeline-item">
          <div className="timeline-date">
            {new Date(log.data).toLocaleString()}
          </div>

          <div className="timeline-content">
            <strong>
              {log.acao === "FATURADO" && "💰 Faturado"}
              {log.acao === "DESFATURADO" && "↩️ Desfaturado"}
              {!["FATURADO", "DESFATURADO"].includes(log.acao) &&
                log.acao}
            </strong>

            <p className="text-xs text-slate-400">
              Usuário ID: {log.usuarioId}
            </p>
          </div>
        </div>
      ))}
    </div>
  );
};

export default Timeline;