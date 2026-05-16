import { ListChecks, Pencil, Plus, Save, Trash2, X } from "lucide-react";
import { EmptyState } from "./EmptyState.jsx";
import { FormFieldError } from "./FormFieldError.jsx";

export function ChecklistSection({
  checklistItems,
  editingChecklistItemId,
  errors = {},
  form,
  onCancelEdit,
  onChange,
  onDelete,
  onEdit,
  onSubmit,
  onToggle,
}) {
  const SubmitIcon = editingChecklistItemId ? Save : Plus;

  return (
    <section className="section-card">
      <div className="section-header">
        <div>
          <h2 className="section-title section-title-row">
            <ListChecks className="section-title-icon" aria-hidden="true" />
            Lista za pripremu
          </h2>
          <p className="section-subtitle">Stavke za pakovanje i pripremu.</p>
        </div>
        <span className="badge">{checklistItems.length}</span>
      </div>

      <div className="section-content-grid">
        <form className="form-grid section-form" noValidate onSubmit={onSubmit}>
          <label className="field">
            <span className="field-label">Nova stavka</span>
            <input
              className={`input${errors.title ? " input-error" : ""}`}
              maxLength={150}
              name="title"
              onChange={onChange}
              placeholder="pasos, karta, punjac, putno osiguranje"
              required
              value={form.title}
            />
            <FormFieldError message={errors.title} />
          </label>

          <div className="button-row">
            <button className="btn btn-primary" type="submit">
              <SubmitIcon className="btn-icon" aria-hidden="true" />
              {editingChecklistItemId ? "Sacuvaj stavku" : "Dodaj stavku"}
            </button>
            {editingChecklistItemId ? (
              <button className="btn btn-secondary" onClick={onCancelEdit} type="button">
                <X className="btn-icon" aria-hidden="true" />
                Odustani
              </button>
            ) : null}
          </div>
        </form>

        <div className="item-list">
          {checklistItems.map((checklistItem) => (
            <article className="list-item checklist-item" key={checklistItem.id}>
              <label className="checklist-label">
                <input
                  checked={Boolean(checklistItem.isCompleted)}
                  onChange={(event) => onToggle(checklistItem, event.target.checked)}
                  type="checkbox"
                />
                <span className={`checklist-title${checklistItem.isCompleted ? " is-done" : ""}`}>
                  {checklistItem.title}
                </span>
              </label>
              <div className="list-item-actions">
                <button className="btn btn-secondary btn-small" onClick={() => onEdit(checklistItem)} type="button">
                  <Pencil className="btn-icon" aria-hidden="true" />
                  Izmeni
                </button>
                <button className="btn btn-danger-soft btn-small" onClick={() => onDelete(checklistItem.id)} type="button">
                  <Trash2 className="btn-icon" aria-hidden="true" />
                  Obrisi
                </button>
              </div>
            </article>
          ))}
          {checklistItems.length === 0 ? <EmptyState>Lista za pripremu je prazna.</EmptyState> : null}
        </div>
      </div>
    </section>
  );
}
