import { NotebookText, Pencil, Plus, Save, Trash2, X } from "lucide-react";
import { EmptyState } from "./EmptyState.jsx";
import { FormFieldError } from "./FormFieldError.jsx";
import { formatDateTime } from "./tripDisplayUtils.js";

export function NotesSection({ editingNoteId, errors = {}, form, notes, onCancelEdit, onChange, onDelete, onEdit, onSubmit }) {
  const SubmitIcon = editingNoteId ? Save : Plus;

  return (
    <section className="section-card">
      <div className="section-header">
        <div>
          <h2 className="section-title section-title-row">
            <NotebookText className="section-title-icon" aria-hidden="true" />
            Beleske
          </h2>
          <p className="section-subtitle">Kratke napomene vezane za izabrani plan.</p>
        </div>
        <span className="badge">{notes.length}</span>
      </div>

      <form className="form-grid" noValidate onSubmit={onSubmit}>
        <label className="field">
          <span className="field-label">Naslov</span>
          <input
            className={`input${errors.title ? " input-error" : ""}`}
            maxLength={150}
            name="title"
            onChange={onChange}
            placeholder="Rezervacije, ideje, bitni kontakti"
            required
            value={form.title}
          />
          <FormFieldError message={errors.title} />
        </label>

        <label className="field">
          <span className="field-label">Sadrzaj</span>
          <textarea
            className="textarea"
            name="content"
            onChange={onChange}
            placeholder="Tekst beleske"
            rows={3}
            value={form.content}
          />
        </label>

        <div className="button-row">
          <button className="btn btn-primary" type="submit">
            <SubmitIcon className="btn-icon" aria-hidden="true" />
            {editingNoteId ? "Sacuvaj belesku" : "Dodaj belesku"}
          </button>
          {editingNoteId ? (
            <button className="btn btn-secondary" onClick={onCancelEdit} type="button">
              <X className="btn-icon" aria-hidden="true" />
              Odustani
            </button>
          ) : null}
        </div>
      </form>

      <div className="item-list">
        {notes.map((note) => (
          <article className="list-item" key={note.id}>
            <div className="list-item-main">
              <span className="list-item-title">{note.title}</span>
              <p className="muted">
                {note.updatedAt ? `Izmenjeno: ${formatDateTime(note.updatedAt)}` : `Kreirano: ${formatDateTime(note.createdAt)}`}
              </p>
              {note.content ? <p className="list-item-description">{note.content}</p> : null}
            </div>
            <div className="list-item-actions">
              <button className="btn btn-secondary btn-small" onClick={() => onEdit(note)} type="button">
                <Pencil className="btn-icon" aria-hidden="true" />
                Izmeni
              </button>
              <button className="btn btn-danger-soft btn-small" onClick={() => onDelete(note.id)} type="button">
                <Trash2 className="btn-icon" aria-hidden="true" />
                Obrisi
              </button>
            </div>
          </article>
        ))}
        {notes.length === 0 ? <EmptyState>Nema dodatih beleski.</EmptyState> : null}
      </div>
    </section>
  );
}
