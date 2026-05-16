import { Info, PiggyBank, Receipt, Wallet } from "lucide-react";
import { formatDateRange, formatMoney } from "./tripDisplayUtils.js";
import { TripPlanForm } from "./TripPlanForm.jsx";

export function TripPlanDetails({ budgetSummary, errors = {}, form, onChange, onReset, onSubmit, tripPlan }) {
  return (
    <section className="section-card">
      <div className="section-header">
        <div>
          <h2 className="section-title section-title-row">
            <Info className="section-title-icon" aria-hidden="true" />
            Osnovni podaci
          </h2>
          <p className="section-subtitle">Naziv, period, budzet i napomene za izabrani plan.</p>
        </div>
      </div>

      <div className="detail-heading">
        <div>
          <h3 className="detail-title">{tripPlan.title}</h3>
          <p className="section-subtitle">{formatDateRange(tripPlan.startDate, tripPlan.endDate)}</p>
        </div>
      </div>

      <div className="overview-grid">
        <span className="stat-tile">
          <span className="stat-label">
            <Wallet className="stat-icon" aria-hidden="true" />
            Planirani budzet
          </span>
          <span className="stat-value">{formatMoney(budgetSummary?.plannedBudget ?? tripPlan.plannedBudget)}</span>
        </span>
        <span className="stat-tile">
          <span className="stat-label">
            <Receipt className="stat-icon" aria-hidden="true" />
            Ukupno potroseno
          </span>
          <span className="stat-value">{formatMoney(budgetSummary?.totalExpenses)}</span>
        </span>
        <span className="stat-tile">
          <span className="stat-label">
            <PiggyBank className="stat-icon" aria-hidden="true" />
            Preostalo
          </span>
          <span className="stat-value">{formatMoney(budgetSummary?.remainingBudget ?? tripPlan.plannedBudget)}</span>
        </span>
      </div>

      {tripPlan.description ? <p className="list-item-description">{tripPlan.description}</p> : null}
      {tripPlan.notes ? <p className="note-box">{tripPlan.notes}</p> : null}

      <div className="form-panel">
        <h3 className="subsection-title">Izmena osnovnih podataka</h3>
        <TripPlanForm
          errors={errors}
          form={form}
          onChange={onChange}
          onReset={onReset}
          onSubmit={onSubmit}
          showReset
          submitLabel="Sacuvaj osnovne podatke"
        />
      </div>
    </section>
  );
}
