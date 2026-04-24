import { useState } from "react";
import Timeline from "./Timeline";

function ServiceCard({
  ordem,
  onDelete,
  onEdit,
  onFaturar,
  onDesfaturar,
}) {
  const [showTimeline, setShowTimeline] = useState(false);

  return (
    <div className="card w-[250px]">
      <h3 className="font-bold text-lg">{ordem.cliente}</h3>

      <p className="text-sm text-slate-300">{ordem.servico}</p>

      <p className="text-green-400 font-semibold">
        R$ {ordem.valor}
      </p>

      <p className="text-xs text-slate-400">{ordem.status}</p>

      <div className="flex gap-2 mt-3 flex-wrap">
        <button
          onClick={onDelete}
          className="bg-red-500 px-2 py-1 rounded text-xs"
        >
          Excluir
        </button>

        <button
          onClick={onEdit}
          className="bg-blue-500 px-2 py-1 rounded text-xs"
        >
          Editar
        </button>

        {!ordem.faturado && (
          <button
            onClick={onFaturar}
            className="bg-green-600 px-2 py-1 rounded text-xs"
          >
            Faturar
          </button>
        )}

        {ordem.faturado && (
          <button
            onClick={onDesfaturar}
            className="bg-yellow-500 px-2 py-1 rounded text-xs"
          >
            Desfaturar
          </button>
        )}

        <button
          onClick={() => setShowTimeline(!showTimeline)}
          className="bg-slate-600 px-2 py-1 rounded text-xs"
        >
          Histórico
        </button>
      </div>

      {showTimeline && (
        <Timeline ordemId={ordem.id} />
      )}
    </div>
  );
}

export default ServiceCard;