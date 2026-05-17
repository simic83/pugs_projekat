import { Plane, UserPlus } from "lucide-react";
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { FormFieldError } from "../components/trips/FormFieldError.jsx";
import { useAuth } from "../context/AuthContext.jsx";
import { createRegisterFormModel } from "../models/auth.js";
import { MINIMUM_PASSWORD_LENGTH, hasValidationErrors, validateRegister } from "../utils/validation.js";

export function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState(createRegisterFormModel);
  const [error, setError] = useState("");
  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  const updateField = (event) => {
    const { name, value } = event.target;
    setForm((currentForm) => ({
      ...currentForm,
      [name]: value,
    }));
    setErrors((currentErrors) => ({
      ...currentErrors,
      [name]: "",
    }));
  };

  const submit = async (event) => {
    event.preventDefault();
    setError("");

    const validationErrors = validateRegister(form);
    if (hasValidationErrors(validationErrors)) {
      setErrors(validationErrors);
      return;
    }

    setErrors({});
    setIsSubmitting(true);

    try {
      await register(form);
      navigate("/login", { replace: true, state: { registered: true, email: form.email } });
    } catch (requestError) {
      setError(requestError.message || "Doslo je do greske.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <main className="auth-page">
      <form className="auth-card" noValidate onSubmit={submit}>
        <div className="auth-heading">
          <span className="brand-mark" aria-hidden="true">
            <Plane />
          </span>
          <h1 className="auth-title">Register</h1>
          <p className="auth-subtitle">Napravi nalog za svoje planove putovanja.</p>
        </div>

        <label className="field">
          <span className="field-label">Ime</span>
          <input
            autoComplete="name"
            className={`input${errors.name ? " input-error" : ""}`}
            name="name"
            onChange={updateField}
            required
            type="text"
            value={form.name}
          />
          <FormFieldError message={errors.name} />
        </label>

        <label className="field">
          <span className="field-label">Email</span>
          <input
            autoComplete="email"
            className={`input${errors.email ? " input-error" : ""}`}
            name="email"
            onChange={updateField}
            required
            type="email"
            value={form.email}
          />
          <FormFieldError message={errors.email} />
        </label>

        <label className="field">
          <span className="field-label">Lozinka</span>
          <input
            autoComplete="new-password"
            className={`input${errors.password ? " input-error" : ""}`}
            minLength={MINIMUM_PASSWORD_LENGTH}
            name="password"
            onChange={updateField}
            required
            type="password"
            value={form.password}
          />
          <FormFieldError message={errors.password} />
        </label>

        {error ? <p className="alert alert-error">{error}</p> : null}

        <button className="btn btn-primary" disabled={isSubmitting} type="submit">
          <UserPlus className="btn-icon" aria-hidden="true" />
          {isSubmitting ? "Kreiranje..." : "Kreiraj nalog"}
        </button>

        <p className="auth-subtitle">
          Vec imas nalog? <Link to="/login">Login</Link>
        </p>
      </form>
    </main>
  );
}
