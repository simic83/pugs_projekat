import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext.jsx";

export function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ name: "", email: "", password: "" });
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

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
      await register(form);
      navigate("/", { replace: true });
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
          <h1 className="auth-title">Register</h1>
          <p className="auth-subtitle">Napravi nalog za svoje planove putovanja.</p>
        </div>

        <label className="field">
          <span className="field-label">Ime</span>
          <input
            autoComplete="name"
            className="input"
            name="name"
            onChange={updateField}
            required
            type="text"
            value={form.name}
          />
        </label>

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
            autoComplete="new-password"
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
          {isSubmitting ? "Kreiranje..." : "Kreiraj nalog"}
        </button>

        <p className="auth-subtitle">
          Vec imas nalog? <Link to="/login">Login</Link>
        </p>
      </form>
    </main>
  );
}
