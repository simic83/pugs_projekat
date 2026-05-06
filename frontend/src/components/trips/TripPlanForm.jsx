import { Plus, RotateCcw, Save } from "lucide-react";
import { FormFieldError } from "./FormFieldError.jsx";

export function TripPlanForm({
  errors = {},
  form,
  onChange,
  onReset,
  onSubmit,
  resetLabel = "Vrati vrednosti",
  showReset = false,
  submitLabel = "Dodaj plan",
}) {
  const SubmitIcon = submitLabel.startsWith("Dodaj") ? Plus : Save;

  return (
    <form className="form-grid" noValidate onSubmit={onSubmit}>
      <label className="field">
        <span className="field-label">Naziv plana</span>
        <input
          className={`input${errors.title ? " input-error" : ""}`}
          name="title"
          onChange={onChange}
          placeholder="Letovanje u Grckoj"
          required
          value={form.title}
        />
        <FormFieldError message={errors.title} />
      </label>

      <label className="field">
        <span className="field-label">Opis</span>
        <textarea
          className="textarea"
          name="description"
          onChange={onChange}
          placeholder="Kratak opis plana"
          rows={3}
          value={form.description}
        />
      </label>

      <div className="form-row">
        <label className="field">
          <span className="field-label">Pocetak</span>
          <input
            className={`input${errors.startDate ? " input-error" : ""}`}
            name="startDate"
            onChange={onChange}
            required
            type="date"
            value={form.startDate}
          />
          <FormFieldError message={errors.startDate} />
        </label>
        <label className="field">
          <span className="field-label">Kraj</span>
          <input
            className={`input${errors.endDate ? " input-error" : ""}`}
            name="endDate"
            onChange={onChange}
            required
            type="date"
            value={form.endDate}
          />
          <FormFieldError message={errors.endDate} />
        </label>
      </div>

      <label className="field">
        <span className="field-label">Planirani budzet</span>
        <input
          className={`input${errors.plannedBudget ? " input-error" : ""}`}
          min="0"
          name="plannedBudget"
          onChange={onChange}
          step="0.01"
          type="number"
          value={form.plannedBudget}
        />
        <FormFieldError message={errors.plannedBudget} />
      </label>

      <label className="field">
        <span className="field-label">Napomene</span>
        <textarea
          className="textarea"
          name="notes"
          onChange={onChange}
          placeholder="Smestaj, prevoz, vazne napomene"
          rows={3}
          value={form.notes}
        />
      </label>

      <div className="button-row">
        <button className="btn btn-primary" type="submit">
          <SubmitIcon className="btn-icon" aria-hidden="true" />
          {submitLabel}
        </button>
        {showReset ? (
          <button className="btn btn-secondary" onClick={onReset} type="button">
            <RotateCcw className="btn-icon" aria-hidden="true" />
            {resetLabel}
          </button>
        ) : null}
      </div>
    </form>
  );
}
