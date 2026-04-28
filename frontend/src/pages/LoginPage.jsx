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
    <main style={styles.page}>
      <form onSubmit={submit} style={styles.form}>
        <h1 style={styles.title}>Login</h1>

        <label style={styles.field}>
          <span>Email</span>
          <input
            autoComplete="email"
            name="email"
            onChange={updateField}
            required
            style={styles.input}
            type="email"
            value={form.email}
          />
        </label>

        <label style={styles.field}>
          <span>Password</span>
          <input
            autoComplete="current-password"
            minLength={8}
            name="password"
            onChange={updateField}
            required
            style={styles.input}
            type="password"
            value={form.password}
          />
        </label>

        {error ? <p style={styles.error}>{error}</p> : null}

        <button disabled={isSubmitting} style={styles.button} type="submit">
          {isSubmitting ? "Signing in..." : "Sign in"}
        </button>

        <p style={styles.footer}>
          Need an account? <Link to="/register">Register</Link>
        </p>
      </form>
    </main>
  );
}

const styles = {
  page: {
    minHeight: "100vh",
    display: "grid",
    placeItems: "center",
    padding: "24px",
    background: "#f7f7f8",
  },
  form: {
    width: "100%",
    maxWidth: "380px",
    display: "grid",
    gap: "16px",
    padding: "24px",
    border: "1px solid #dedee3",
    borderRadius: "8px",
    background: "#ffffff",
  },
  title: {
    margin: 0,
    fontSize: "28px",
    fontWeight: 700,
  },
  field: {
    display: "grid",
    gap: "6px",
    fontSize: "14px",
  },
  input: {
    minHeight: "40px",
    padding: "8px 10px",
    border: "1px solid #c9c9d1",
    borderRadius: "6px",
    font: "inherit",
  },
  button: {
    minHeight: "42px",
    border: 0,
    borderRadius: "6px",
    background: "#1769aa",
    color: "#ffffff",
    cursor: "pointer",
    font: "inherit",
    fontWeight: 700,
  },
  error: {
    margin: 0,
    color: "#b42318",
    fontSize: "14px",
  },
  footer: {
    margin: 0,
    fontSize: "14px",
  },
};
