import { BellRing, Pencil, Plus, Save, Trash2, X } from "lucide-react";
import { EmptyState } from "./EmptyState.jsx";
import { FormFieldError } from "./FormFieldError.jsx";
import { formatDateTime } from "./tripDisplayUtils.js";

export function RemindersSection({
  canEdit = true,
  editingReminderId,
  errors = {},
  form,
  onCancelEdit = () => {},
  onChange = () => {},
  onDelete = () => {},
  onEdit = () => {},
  onSubmit = () => {},
  onToggle = () => {},
  reminders = [],
}) {
  const SubmitIcon = editingReminderId ? Save : Plus;

  return (
    <section className="section-card">
      <div className="section-header">
        <div>
          <h2 className="section-title section-title-row">
            <BellRing className="section-title-icon" aria-hidden="true" />
            Podsjetnici
          </h2>
          <p className="section-subtitle">Rokovi i zadaci prije ili tokom putovanja.</p>
        </div>
        <span className="badge">{reminders.length}</span>
      </div>

      <div className={`section-content-grid${canEdit ? "" : " section-content-grid-single"}`}>
        {canEdit ? (
          <form className="form-grid section-form" noValidate onSubmit={onSubmit}>
            <label className="field">
              <span className="field-label">Naziv</span>
              <input
                className={`input${errors.title ? " input-error" : ""}`}
                maxLength={150}
                name="title"
                onChange={onChange}
                placeholder="Proveriti dokumenta"
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
              placeholder="Detalji podsjetnika"
              rows={3}
              value={form.description}
            />
          </label>

          <label className="field">
            <span className="field-label">Datum i vreme</span>
            <input
              className={`input${errors.reminderAt ? " input-error" : ""}`}
              name="reminderAt"
              onChange={onChange}
              required
              type="datetime-local"
              value={form.reminderAt}
            />
            <FormFieldError message={errors.reminderAt} />
          </label>

          <label className="checklist-label">
            <input checked={form.isCompleted} name="isCompleted" onChange={onChange} type="checkbox" />
            <span>Zavrseno</span>
          </label>

            <div className="button-row">
              <button className="btn btn-primary" type="submit">
                <SubmitIcon className="btn-icon" aria-hidden="true" />
                {editingReminderId ? "Sacuvaj podsjetnik" : "Dodaj podsjetnik"}
              </button>
              {editingReminderId ? (
                <button className="btn btn-secondary" onClick={onCancelEdit} type="button">
                  <X className="btn-icon" aria-hidden="true" />
                  Odustani
                </button>
              ) : null}
            </div>
          </form>
        ) : null}

        <div className="item-list">
          {reminders.map((reminder) => (
            <article className="list-item" key={reminder.id}>
              <div className="list-item-main">
                {canEdit ? (
                  <label className="checklist-label">
                    <input
                      checked={Boolean(reminder.isCompleted)}
                      onChange={(event) => onToggle(reminder, event.target.checked)}
                      type="checkbox"
                    />
                    <span className={`list-item-title checklist-title${reminder.isCompleted ? " is-done" : ""}`}>
                      {reminder.title}
                    </span>
                  </label>
                ) : (
                  <span className={`list-item-title checklist-title${reminder.isCompleted ? " is-done" : ""}`}>
                    {reminder.title}
                  </span>
                )}
                <p className="muted">Vreme: {formatDateTime(reminder.reminderAt)}</p>
                <span className={`badge ${reminder.isCompleted ? "badge-success" : "badge-muted"}`}>
                  {reminder.isCompleted ? "Zavrseno" : "Nezavrseno"}
                </span>
                {reminder.description ? <p className="list-item-description">{reminder.description}</p> : null}
              </div>
              {canEdit ? (
                <div className="list-item-actions">
                  <button className="btn btn-secondary btn-small" onClick={() => onEdit(reminder)} type="button">
                    <Pencil className="btn-icon" aria-hidden="true" />
                    Izmeni
                  </button>
                  <button className="btn btn-danger-soft btn-small" onClick={() => onDelete(reminder.id)} type="button">
                    <Trash2 className="btn-icon" aria-hidden="true" />
                    Obrisi
                  </button>
                </div>
              ) : null}
            </article>
          ))}
          {reminders.length === 0 ? <EmptyState>Nema dodatih podsjetnika.</EmptyState> : null}
        </div>
      </div>
    </section>
  );
}
