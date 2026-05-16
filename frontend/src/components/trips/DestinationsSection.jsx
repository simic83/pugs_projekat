import { MapPinned, Pencil, Plus, Save, Trash2, X } from "lucide-react";
import { EmptyState } from "./EmptyState.jsx";
import { FormFieldError } from "./FormFieldError.jsx";
import { formatDateRange } from "./tripDisplayUtils.js";

export function DestinationsSection({
  destinations,
  editingDestinationId,
  errors = {},
  form,
  onCancelEdit,
  onChange,
  onDelete,
  onEdit,
  onSubmit,
  tripPlan,
}) {
  const SubmitIcon = editingDestinationId ? Save : Plus;
  const tripStartDate = tripPlan?.startDate ? String(tripPlan.startDate).slice(0, 10) : undefined;
  const tripEndDate = tripPlan?.endDate ? String(tripPlan.endDate).slice(0, 10) : undefined;

  return (
    <section className="section-card">
      <div className="section-header">
        <div>
          <h2 className="section-title section-title-row">
            <MapPinned className="section-title-icon" aria-hidden="true" />
            Destinacije
          </h2>
          <p className="section-subtitle">Gradovi i mesta koja ulaze u plan.</p>
        </div>
        <span className="badge">{destinations.length}</span>
      </div>

      <form className="form-grid" noValidate onSubmit={onSubmit}>
        <div className="form-row">
          <label className="field">
            <span className="field-label">Naziv</span>
            <input
              className={`input${errors.name ? " input-error" : ""}`}
              name="name"
              onChange={onChange}
              placeholder="Atina"
              required
              value={form.name}
            />
            <FormFieldError message={errors.name} />
          </label>
          <label className="field">
            <span className="field-label">Lokacija</span>
            <input className="input" name="location" onChange={onChange} placeholder="Grcka" value={form.location} />
          </label>
        </div>

        <div className="form-row">
          <label className="field">
            <span className="field-label">Datum dolaska</span>
            <input
              className={`input${errors.arrivalDate ? " input-error" : ""}`}
              max={tripEndDate}
              min={tripStartDate}
              name="arrivalDate"
              onChange={onChange}
              required
              type="date"
              value={form.arrivalDate}
            />
            <FormFieldError message={errors.arrivalDate} />
          </label>
          <label className="field">
            <span className="field-label">Datum odlaska</span>
            <input
              className={`input${errors.departureDate ? " input-error" : ""}`}
              max={tripEndDate}
              min={tripStartDate}
              name="departureDate"
              onChange={onChange}
              required
              type="date"
              value={form.departureDate}
            />
            <FormFieldError message={errors.departureDate} />
          </label>
        </div>

        <label className="field">
          <span className="field-label">Opis</span>
          <textarea
            className="textarea"
            name="description"
            onChange={onChange}
            placeholder="Smestaj, prevoz ili kratka napomena"
            rows={3}
            value={form.description}
          />
        </label>

        <div className="button-row">
          <button className="btn btn-primary" type="submit">
            <SubmitIcon className="btn-icon" aria-hidden="true" />
            {editingDestinationId ? "Sacuvaj destinaciju" : "Dodaj destinaciju"}
          </button>
          {editingDestinationId ? (
            <button className="btn btn-secondary" onClick={onCancelEdit} type="button">
              <X className="btn-icon" aria-hidden="true" />
              Odustani
            </button>
          ) : null}
        </div>
      </form>

      <div className="item-list">
        {destinations.map((destination) => (
          <article className="list-item" key={destination.id}>
            <div className="list-item-main">
              <span className="list-item-title">{destination.name}</span>
              <p className="muted">
                {destination.location ? `${destination.location} - ` : ""}
                {formatDateRange(destination.arrivalDate, destination.departureDate)}
              </p>
              {destination.description ? <p className="list-item-description">{destination.description}</p> : null}
            </div>
            <div className="list-item-actions">
              <button className="btn btn-secondary btn-small" onClick={() => onEdit(destination)} type="button">
                <Pencil className="btn-icon" aria-hidden="true" />
                Izmeni
              </button>
              <button className="btn btn-danger-soft btn-small" onClick={() => onDelete(destination.id)} type="button">
                <Trash2 className="btn-icon" aria-hidden="true" />
                Obrisi
              </button>
            </div>
          </article>
        ))}
        {destinations.length === 0 ? <EmptyState>Nema dodatih destinacija.</EmptyState> : null}
      </div>
    </section>
  );
}
