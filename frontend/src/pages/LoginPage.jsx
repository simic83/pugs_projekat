import { LogIn, Plane } from "lucide-react";
import { useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { FormFieldError } from "../components/trips/FormFieldError.jsx";
import { useAuth } from "../context/AuthContext.jsx";
import { createLoginFormModel } from "../models/auth.js";
import { hasValidationErrors, validateLogin } from "../utils/validation.js";

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [form, setForm] = useState(() => createLoginFormModel({ email: location.state?.email }));
  const [error, setError] = useState("");
  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  const redirectTo = location.state?.from?.pathname ?? "/";
  const registrationMessage = location.state?.registered
    ? "Nalog je kreiran. Prijavi se svojim emailom i lozinkom."
    : "";

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

    const validationErrors = validateLogin(form);
    if (hasValidationErrors(validationErrors)) {
      setErrors(validationErrors);
      return;
    }

    setErrors({});
    setIsSubmitting(true);

    try {
      await login(form);
      navigate(redirectTo, { replace: true });
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
          <h1 className="auth-title">Login</h1>
          <p className="auth-subtitle">Prijavi se i nastavi sa planiranjem putovanja.</p>
        </div>

        {registrationMessage ? <p className="alert alert-success">{registrationMessage}</p> : null}

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
            autoComplete="current-password"
            className={`input${errors.password ? " input-error" : ""}`}
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
          <LogIn className="btn-icon" aria-hidden="true" />
          {isSubmitting ? "Prijava..." : "Prijavi se"}
        </button>

        <p className="auth-subtitle">
          Nemas nalog? <Link to="/register">Registruj se</Link>
        </p>
      </form>
    </main>
  );
}
