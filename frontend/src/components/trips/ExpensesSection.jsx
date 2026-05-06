import { Pencil, PiggyBank, Plus, Receipt, Save, Trash2, Wallet, X } from "lucide-react";
import { EXPENSE_CATEGORIES } from "../../models/budget.js";
import { EmptyState } from "./EmptyState.jsx";
import { FormFieldError } from "./FormFieldError.jsx";
import { formatDate, formatMoney, getExpenseCategoryLabel } from "./tripDisplayUtils.js";

export function ExpensesSection({
  budgetSummary,
  editingExpenseId,
  errors = {},
  expenses,
  form,
  onCancelEdit,
  onChange,
  onDelete,
  onEdit,
  onSubmit,
  selectedTripPlan,
}) {
  const SubmitIcon = editingExpenseId ? Save : Plus;

  return (
    <section className="section-card">
      <div className="section-header">
        <div>
          <h2 className="section-title section-title-row">
            <Wallet className="section-title-icon" aria-hidden="true" />
            Troskovi i budzet
          </h2>
          <p className="section-subtitle">Transport, smestaj, hrana, karte i ostalo.</p>
        </div>
        <span className="badge">{expenses.length}</span>
      </div>

      <div className="overview-grid">
        <span className="stat-tile">
          <span className="stat-label">
            <Wallet className="stat-icon" aria-hidden="true" />
            Planirani budzet
          </span>
          <span className="stat-value">
            {formatMoney(budgetSummary?.plannedBudget ?? selectedTripPlan.plannedBudget)}
          </span>
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
            Preostali budzet
          </span>
          <span className="stat-value">
            {formatMoney(budgetSummary?.remainingBudget ?? selectedTripPlan.plannedBudget)}
          </span>
        </span>
      </div>

      <form className="form-grid" noValidate onSubmit={onSubmit}>
        <label className="field">
          <span className="field-label">Naziv troska</span>
          <input
            className={`input${errors.title ? " input-error" : ""}`}
            name="title"
            onChange={onChange}
            placeholder="Avionska karta"
            required
            value={form.title}
          />
          <FormFieldError message={errors.title} />
        </label>

        <div className="form-row">
          <label className="field">
            <span className="field-label">Kategorija</span>
            <select
              className={`select${errors.category ? " input-error" : ""}`}
              name="category"
              onChange={onChange}
              required
              value={form.category}
            >
              {EXPENSE_CATEGORIES.map((category) => (
                <option key={category.value} value={category.value}>
                  {category.label}
                </option>
              ))}
            </select>
            <FormFieldError message={errors.category} />
          </label>
          <label className="field">
            <span className="field-label">Iznos</span>
            <input
              className={`input${errors.amount ? " input-error" : ""}`}
              min="0"
              name="amount"
              onChange={onChange}
              required
              step="0.01"
              type="number"
              value={form.amount}
            />
            <FormFieldError message={errors.amount} />
          </label>
        </div>

        <label className="field">
          <span className="field-label">Datum</span>
          <input
            className={`input${errors.expenseDate ? " input-error" : ""}`}
            name="expenseDate"
            onChange={onChange}
            required
            type="date"
            value={form.expenseDate}
          />
          <FormFieldError message={errors.expenseDate} />
        </label>

        <label className="field">
          <span className="field-label">Opis</span>
          <textarea
            className="textarea"
            name="description"
            onChange={onChange}
            placeholder="Napomena za trosak"
            rows={3}
            value={form.description}
          />
        </label>

        <div className="button-row">
          <button className="btn btn-primary" type="submit">
            <SubmitIcon className="btn-icon" aria-hidden="true" />
            {editingExpenseId ? "Sacuvaj trosak" : "Dodaj trosak"}
          </button>
          {editingExpenseId ? (
            <button className="btn btn-secondary" onClick={onCancelEdit} type="button">
              <X className="btn-icon" aria-hidden="true" />
              Odustani
            </button>
          ) : null}
        </div>
      </form>

      <div className="item-list">
        {expenses.map((expense) => (
          <article className="list-item" key={expense.id}>
            <div className="list-item-main">
              <span className="list-item-title">{expense.title}</span>
              <p className="muted">
                {getExpenseCategoryLabel(expense.category)} - {formatDate(expense.expenseDate)}
              </p>
              <p className="muted">{formatMoney(expense.amount)}</p>
              {expense.description ? <p className="list-item-description">{expense.description}</p> : null}
            </div>
            <div className="list-item-actions">
              <button className="btn btn-secondary btn-small" onClick={() => onEdit(expense)} type="button">
                <Pencil className="btn-icon" aria-hidden="true" />
                Izmeni
              </button>
              <button className="btn btn-danger-soft btn-small" onClick={() => onDelete(expense.id)} type="button">
                <Trash2 className="btn-icon" aria-hidden="true" />
                Obrisi
              </button>
            </div>
          </article>
        ))}
        {expenses.length === 0 ? <EmptyState>Nema dodatih troskova.</EmptyState> : null}
      </div>
    </section>
  );
}
