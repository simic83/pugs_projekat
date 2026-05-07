import { ArrowLeft, Printer } from "lucide-react";
import { useCallback, useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { useApp } from "../context/AppContext.jsx";
import { EXPENSE_CATEGORIES } from "../models/budget.js";
import { ACTIVITY_STATUSES } from "../models/tripPlan.js";

export function TripPlanReportPage() {
  const { tripPlanReportService } = useApp();
  const { tripPlanId } = useParams();
  const [tripPlan, setTripPlan] = useState(null);
  const [destinations, setDestinations] = useState([]);
  const [activities, setActivities] = useState([]);
  const [expenses, setExpenses] = useState([]);
  const [budgetSummary, setBudgetSummary] = useState(null);
  const [checklistItems, setChecklistItems] = useState([]);
  const [notes, setNotes] = useState([]);
  const [reminders, setReminders] = useState([]);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  const loadReport = useCallback(async () => {
    if (!tripPlanId) {
      setError("Plan nije izabran.");
      setIsLoading(false);
      return;
    }

    setError("");
    setIsLoading(true);

    try {
      const report = await tripPlanReportService.getByTripPlanId(tripPlanId);
      setTripPlan(report.tripPlan);
      setDestinations(report.destinations);
      setActivities(report.activities);
      setExpenses(report.expenses);
      setBudgetSummary(report.budgetSummary);
      setChecklistItems(report.checklistItems);
      setNotes(report.notes);
      setReminders(report.reminders);
    } catch (requestError) {
      setTripPlan(null);
      setError(requestError.message || "Izvestaj nije moguce ucitati.");
    } finally {
      setIsLoading(false);
    }
  }, [tripPlanId, tripPlanReportService]);

  useEffect(() => {
    void loadReport();
  }, [loadReport]);

  const sortedDestinations = useMemo(
    () => [...destinations].sort((first, second) => compareDates(first.arrivalDate, second.arrivalDate)),
    [destinations],
  );

  const sortedActivities = useMemo(() => [...activities].sort(compareActivities), [activities]);
  const sortedExpenses = useMemo(
    () => [...expenses].sort((first, second) => compareDates(first.expenseDate, second.expenseDate)),
    [expenses],
  );
  const sortedReminders = useMemo(
    () => [...reminders].sort((first, second) => compareDates(first.reminderAt, second.reminderAt)),
    [reminders],
  );

  const plannedBudget = budgetSummary?.plannedBudget ?? tripPlan?.plannedBudget ?? 0;
  const totalExpenses =
    budgetSummary?.totalExpenses ?? expenses.reduce((total, expense) => total + Number(expense.amount ?? 0), 0);
  const remainingBudget = budgetSummary?.remainingBudget ?? plannedBudget - totalExpenses;

  const printReport = () => {
    window.print();
  };

  return (
    <div className="report-page">
      <div className="report-toolbar no-print">
        <Link className="btn btn-secondary" to="/">
          <ArrowLeft className="btn-icon" aria-hidden="true" />
          Nazad na planove
        </Link>
        <button className="btn btn-primary" onClick={printReport} type="button">
          <Printer className="btn-icon" aria-hidden="true" />
          Stampaj / Sacuvaj kao PDF
        </button>
      </div>

      {error ? <p className="alert alert-error">{error}</p> : null}
      {isLoading ? <div className="empty-state">Ucitavanje izvestaja...</div> : null}

      {!isLoading && tripPlan ? (
        <article className="report-document">
          <header className="report-header">
            <p className="report-kicker">Travel Planner</p>
            <h1 className="report-title">Travel Planner - Izvestaj plana</h1>
            <p className="report-subtitle">{tripPlan.title}</p>
          </header>

          <ReportSection title="Osnovni podaci">
            <dl className="report-details-grid">
              <ReportDetail label="Naziv" value={tripPlan.title} />
              <ReportDetail label="Opis" value={tripPlan.description} />
              <ReportDetail label="Start date" value={formatDate(tripPlan.startDate)} />
              <ReportDetail label="End date" value={formatDate(tripPlan.endDate)} />
              <ReportDetail label="Planned budget" value={formatMoney(tripPlan.plannedBudget)} />
              <ReportDetail label="Notes plana" value={tripPlan.notes} />
            </dl>
          </ReportSection>

          <ReportTable
            columns={["Naziv", "Lokacija", "Dolazak", "Odlazak", "Opis"]}
            items={sortedDestinations}
            renderRow={(destination) => [
              destination.name,
              destination.location,
              formatDate(destination.arrivalDate),
              formatDate(destination.departureDate),
              destination.description,
            ]}
            title="Destinacije"
          />

          <ReportTable
            columns={["Naziv", "Datum", "Vreme", "Lokacija", "Status", "Proc. trosak", "Opis"]}
            items={sortedActivities}
            renderRow={(activity) => [
              activity.title,
              formatDate(activity.activityDate),
              formatTime(activity.activityTime),
              activity.location,
              getStatusLabel(activity.status),
              formatMoney(activity.estimatedCost),
              activity.description,
            ]}
            title="Aktivnosti"
          />

          <ReportTable
            columns={["Naziv", "Kategorija", "Iznos", "Datum", "Opis"]}
            items={sortedExpenses}
            renderRow={(expense) => [
              expense.title,
              getExpenseCategoryLabel(expense.category),
              formatMoney(expense.amount),
              formatDate(expense.expenseDate),
              expense.description,
            ]}
            title="Troskovi"
          />

          <ReportSection title="Budzet">
            <dl className="report-details-grid report-budget-grid">
              <ReportDetail label="Planirani budzet" value={formatMoney(plannedBudget)} />
              <ReportDetail label="Ukupno potroseno" value={formatMoney(totalExpenses)} />
              <ReportDetail label="Preostali budzet" value={formatMoney(remainingBudget)} />
            </dl>
          </ReportSection>

          <ReportTable
            columns={["Naziv stavke", "Status"]}
            items={checklistItems}
            renderRow={(checklistItem) => [
              checklistItem.title,
              checklistItem.isCompleted ? "Zavrseno" : "Nezavrseno",
            ]}
            title="Checklist"
          />

          <ReportTable
            columns={["Naslov", "Sadrzaj"]}
            items={notes}
            renderRow={(note) => [note.title, note.content]}
            title="Beleske"
          />

          <ReportTable
            columns={["Naziv", "Opis", "Datum/vreme", "Status"]}
            items={sortedReminders}
            renderRow={(reminder) => [
              reminder.title,
              reminder.description,
              formatDateTime(reminder.reminderAt),
              reminder.isCompleted ? "Zavrseno" : "Nezavrseno",
            ]}
            title="Podsjetnici"
          />
        </article>
      ) : null}
    </div>
  );
}

function ReportSection({ children, title }) {
  return (
    <section className="report-section">
      <h2 className="report-section-title">{title}</h2>
      {children}
    </section>
  );
}

function ReportDetail({ label, value }) {
  return (
    <div className="report-detail">
      <dt>{label}</dt>
      <dd>{hasValue(value) ? value : "-"}</dd>
    </div>
  );
}

function ReportTable({ columns, items, renderRow, title }) {
  return (
    <ReportSection title={title}>
      {items.length > 0 ? (
        <div className="report-table-wrapper">
          <table className="report-table">
            <thead>
              <tr>
                {columns.map((column) => (
                  <th key={column}>{column}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {items.map((item, itemIndex) => (
                <tr key={item.id ?? itemIndex}>
                  {renderRow(item).map((cell, cellIndex) => (
                    <td key={`${item.id ?? itemIndex}-${cellIndex}`}>{hasValue(cell) ? cell : "-"}</td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <p className="report-empty">Nema unetih podataka.</p>
      )}
    </ReportSection>
  );
}

function compareActivities(first, second) {
  const dateCompareResult = compareDates(first.activityDate, second.activityDate);
  if (dateCompareResult !== 0) {
    return dateCompareResult;
  }

  return String(first.activityTime ?? "").localeCompare(String(second.activityTime ?? ""));
}

function compareDates(firstValue, secondValue) {
  const firstTime = firstValue ? new Date(firstValue).getTime() : Number.MAX_SAFE_INTEGER;
  const secondTime = secondValue ? new Date(secondValue).getTime() : Number.MAX_SAFE_INTEGER;

  return firstTime - secondTime;
}

function formatDate(value) {
  if (!value) {
    return "";
  }

  return new Date(value).toLocaleDateString();
}

function formatDateTime(value) {
  if (!value) {
    return "";
  }

  return new Date(value).toLocaleString();
}

function formatTime(value) {
  if (!value) {
    return "";
  }

  return String(value).slice(0, 5);
}

function formatMoney(value) {
  return Number(value ?? 0).toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
}

function getStatusLabel(value) {
  const numericValue = Number(value);
  return ACTIVITY_STATUSES.find((status) => status.value === numericValue)?.label ?? "Planned";
}

function getExpenseCategoryLabel(value) {
  const numericValue = Number(value);
  return EXPENSE_CATEGORIES.find((category) => category.value === numericValue)?.label ?? "Other";
}

function hasValue(value) {
  return value !== null && value !== undefined && String(value).trim() !== "";
}
