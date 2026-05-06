import { useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext.jsx";

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [form, setForm] = useState({ email: "", password: "" });
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const redirectTo = location.state?.from?.pathname ?? "/";

  const updateField = (event) => {
    setForm((currentForm) => ({
      ...currentForm,
      [event.target.name]: event.target.value,
    }));
  };

  const submit = async (event) => {
    event.preventDefault();
    setError("");
    setIsSubmitting(true);

    try {
      await login(form);
      navigate(redirectTo, { replace: true });
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <main className="auth-page">
      <form className="auth-card" onSubmit={submit}>
        <div className="auth-heading">
          <span className="brand-mark">TP</span>
          <h1 className="auth-title">Login</h1>
          <p className="auth-subtitle">Prijavi se i nastavi sa planiranjem putovanja.</p>
        </div>

        <label className="field">
          <span className="field-label">Email</span>
          <input
            autoComplete="email"
            className="input"
            name="email"
            onChange={updateField}
            required
            type="email"
            value={form.email}
          />
        </label>

        <label className="field">
          <span className="field-label">Lozinka</span>
          <input
            autoComplete="current-password"
            className="input"
            minLength={8}
            name="password"
            onChange={updateField}
            required
            type="password"
            value={form.password}
          />
        </label>

        {error ? <p className="alert alert-error">{error}</p> : null}

        <button className="btn btn-primary" disabled={isSubmitting} type="submit">
          {isSubmitting ? "Prijava..." : "Prijavi se"}
        </button>

        <p className="auth-subtitle">
          Nemas nalog? <Link to="/register">Registruj se</Link>
        </p>
      </form>
    </main>
  );
}
