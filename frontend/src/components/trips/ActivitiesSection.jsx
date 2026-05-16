import { Calendar, CalendarDays, List, Pencil, Plus, Save, Trash2, X } from "lucide-react";
import { EmptyState } from "./EmptyState.jsx";
import { ActivityCalendarView } from "./ActivityCalendarView.jsx";
import { FormFieldError } from "./FormFieldError.jsx";
import {
  ACTIVITY_STATUSES,
  formatDate,
  formatMoney,
  getStatusClass,
  getStatusLabel,
} from "./tripDisplayUtils.js";

export function ActivitiesSection({
  activities,
  activityGroups,
  activityViewMode,
  editingActivityId,
  errors = {},
  form,
  initialCalendarDate,
  onCancelEdit,
  onChange,
  onDelete,
  onEdit,
  onSubmit,
  onViewModeChange,
  tripPlan,
}) {
  const SubmitIcon = editingActivityId ? Save : Plus;
  const tripStartDate = tripPlan?.startDate ? String(tripPlan.startDate).slice(0, 10) : undefined;
  const tripEndDate = tripPlan?.endDate ? String(tripPlan.endDate).slice(0, 10) : undefined;

  return (
    <section className="section-card">
      <div className="section-header">
        <div>
          <h2 className="section-title section-title-row">
            <CalendarDays className="section-title-icon" aria-hidden="true" />
            Dnevni raspored
          </h2>
          <p className="section-subtitle">Aktivnosti rasporedjene po danima putovanja.</p>
        </div>
        <span className="badge">{activities.length}</span>
      </div>

      <div className="section-content-grid section-content-grid-wide">
        <form className="form-grid section-form" noValidate onSubmit={onSubmit}>
          <label className="field">
            <span className="field-label">Naziv aktivnosti</span>
            <input
              className={`input${errors.title ? " input-error" : ""}`}
              name="title"
              onChange={onChange}
              placeholder="Obilazak muzeja"
              required
              value={form.title}
            />
            <FormFieldError message={errors.title} />
          </label>

        <div className="form-row">
          <label className="field">
            <span className="field-label">Datum</span>
            <input
              className={`input${errors.activityDate ? " input-error" : ""}`}
              max={tripEndDate}
              min={tripStartDate}
              name="activityDate"
              onChange={onChange}
              required
              type="date"
              value={form.activityDate}
            />
            <FormFieldError message={errors.activityDate} />
          </label>
          <label className="field">
            <span className="field-label">Vreme</span>
            <input className="input" name="activityTime" onChange={onChange} type="time" value={form.activityTime} />
          </label>
        </div>

        <div className="form-row">
          <label className="field">
            <span className="field-label">Lokacija</span>
            <input
              className="input"
              name="location"
              onChange={onChange}
              placeholder="Centar grada"
              value={form.location}
            />
          </label>
          <label className="field">
            <span className="field-label">Status</span>
            <select
              className={`select${errors.status ? " input-error" : ""}`}
              name="status"
              onChange={onChange}
              value={form.status}
            >
              {ACTIVITY_STATUSES.map((status) => (
                <option key={status.value} value={status.value}>
                  {status.label}
                </option>
              ))}
            </select>
            <FormFieldError message={errors.status} />
          </label>
        </div>

        <label className="field">
          <span className="field-label">Procenjeni trosak</span>
          <input
            className={`input${errors.estimatedCost ? " input-error" : ""}`}
            min="0"
            name="estimatedCost"
            onChange={onChange}
            step="0.01"
            type="number"
            value={form.estimatedCost}
          />
          <FormFieldError message={errors.estimatedCost} />
        </label>

        <label className="field">
          <span className="field-label">Opis</span>
          <textarea
            className="textarea"
            name="description"
            onChange={onChange}
            placeholder="Kratka napomena za aktivnost"
            rows={3}
            value={form.description}
          />
        </label>

          <div className="button-row">
            <button className="btn btn-primary" type="submit">
              <SubmitIcon className="btn-icon" aria-hidden="true" />
              {editingActivityId ? "Sacuvaj aktivnost" : "Dodaj aktivnost"}
            </button>
            {editingActivityId ? (
              <button className="btn btn-secondary" onClick={onCancelEdit} type="button">
                <X className="btn-icon" aria-hidden="true" />
                Odustani
              </button>
            ) : null}
          </div>
        </form>

        <div className="section-list-panel">
          <div className="activity-view-toggle" aria-label="Prikaz aktivnosti" role="group">
            <button
              aria-pressed={activityViewMode === "list"}
              className={`btn btn-small btn-toggle${activityViewMode === "list" ? " is-active" : ""}`}
              onClick={() => onViewModeChange("list")}
              type="button"
            >
              <List className="btn-icon" aria-hidden="true" />
              Lista
            </button>
            <button
              aria-pressed={activityViewMode === "calendar"}
              className={`btn btn-small btn-toggle${activityViewMode === "calendar" ? " is-active" : ""}`}
              onClick={() => onViewModeChange("calendar")}
              type="button"
            >
              <Calendar className="btn-icon" aria-hidden="true" />
              Kalendar
            </button>
          </div>

          {activityViewMode === "list" ? (
            <div className="item-list">
          {activityGroups.map(([dateKey, groupActivities]) => (
            <div className="activity-day" key={dateKey}>
              <h3 className="activity-day-title">
                {dateKey === "no-date" ? "Bez datuma" : formatDate(groupActivities[0]?.activityDate)}
              </h3>
              {groupActivities.map((activity) => (
                <article className="list-item" key={activity.id}>
                  <div className="list-item-main">
                    <span className="list-item-title">{activity.title}</span>
                    <p className="muted">
                      {activity.activityTime ? `${String(activity.activityTime).slice(0, 5)} - ` : ""}
                      {activity.location || "Bez lokacije"}
                    </p>
                    <p className="muted">
                      <span className={`badge ${getStatusClass(activity.status)}`}>
                        {getStatusLabel(activity.status)}
                      </span>{" "}
                      {formatMoney(activity.estimatedCost)}
                    </p>
                    {activity.description ? <p className="list-item-description">{activity.description}</p> : null}
                  </div>
                  <div className="list-item-actions">
                    <button className="btn btn-secondary btn-small" onClick={() => onEdit(activity)} type="button">
                      <Pencil className="btn-icon" aria-hidden="true" />
                      Izmeni
                    </button>
                    <button className="btn btn-danger-soft btn-small" onClick={() => onDelete(activity.id)} type="button">
                      <Trash2 className="btn-icon" aria-hidden="true" />
                      Obrisi
                    </button>
                  </div>
                </article>
              ))}
            </div>
          ))}
              {activities.length === 0 ? <EmptyState>Nema dodatih aktivnosti.</EmptyState> : null}
            </div>
          ) : (
            <ActivityCalendarView activities={activities} initialDate={initialCalendarDate} />
          )}
        </div>
      </div>
    </section>
  );
}
