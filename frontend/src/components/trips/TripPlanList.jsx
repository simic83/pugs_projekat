import { ClipboardList, LoaderCircle } from "lucide-react";
import { EmptyState } from "./EmptyState.jsx";
import { TripPlanCard } from "./TripPlanCard.jsx";

export function TripPlanList({ isLoading, onDelete, onSelect, selectedTripPlanId, tripPlans }) {
  return (
    <section className="section-card">
      <div className="section-header">
        <div>
          <h2 className="section-title section-title-row">
            <ClipboardList className="section-title-icon" aria-hidden="true" />
            Planovi
          </h2>
          <p className="section-subtitle">Ukupno: {tripPlans.length}</p>
        </div>
        {isLoading ? (
          <span className="badge badge-muted">
            <LoaderCircle className="badge-icon" aria-hidden="true" />
            Ucitava se...
          </span>
        ) : null}
      </div>

      <div className="trip-list">
        {tripPlans.map((tripPlan) => (
          <TripPlanCard
            isActive={tripPlan.id === selectedTripPlanId}
            key={tripPlan.id}
            onDelete={onDelete}
            onSelect={onSelect}
            tripPlan={tripPlan}
          />
        ))}

        {!isLoading && tripPlans.length === 0 ? <EmptyState>Nema sacuvanih planova.</EmptyState> : null}
      </div>
    </section>
  );
}
