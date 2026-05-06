export function FormFieldError({ message }) {
  return message ? <span className="field-error">{message}</span> : null;
}
