import { CalendarDays, Eye, Trash2, Wallet } from "lucide-react";
import { formatDateRange, formatMoney } from "./tripDisplayUtils.js";

export function TripPlanCard({ isActive, onDelete, onSelect, tripPlan }) {
  return (
    <article className={`trip-card${isActive ? " is-active" : ""}`}>
      <div className="trip-card-header">
        <h3 className="trip-title">{tripPlan.title}</h3>
        <span className="badge">
          <Wallet className="badge-icon" aria-hidden="true" />
          {formatMoney(tripPlan.plannedBudget)}
        </span>
      </div>

      {tripPlan.description ? <p className="trip-description">{tripPlan.description}</p> : null}

      <div className="meta-grid">
        <span className="meta-item">
          <span className="meta-label">
            <CalendarDays className="meta-icon" aria-hidden="true" />
            Datumi
          </span>
          <span className="meta-value">{formatDateRange(tripPlan.startDate, tripPlan.endDate)}</span>
        </span>
        <span className="meta-item">
          <span className="meta-label">
            <Wallet className="meta-icon" aria-hidden="true" />
            Budzet
          </span>
          <span className="meta-value">{formatMoney(tripPlan.plannedBudget)}</span>
        </span>
      </div>

      <div className="card-actions">
        <button className="btn btn-primary btn-small" onClick={() => onSelect(tripPlan.id)} type="button">
          <Eye className="btn-icon" aria-hidden="true" />
          Otvori detalje
        </button>
        <button className="btn btn-danger-soft btn-small" onClick={() => onDelete(tripPlan.id)} type="button">
          <Trash2 className="btn-icon" aria-hidden="true" />
          Obrisi
        </button>
      </div>
    </article>
  );
}
