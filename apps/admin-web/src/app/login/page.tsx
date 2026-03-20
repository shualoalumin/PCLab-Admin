import { hasAdminSession } from "@/lib/auth/admin-session";
import { loginAction } from "@/app/login/actions";

import { redirect } from "next/navigation";

type LoginPageProps = {
  searchParams: Promise<{
    error?: string;
  }>;
};

export default async function LoginPage({ searchParams }: LoginPageProps) {
  if (await hasAdminSession()) {
    redirect("/dashboard");
  }

  const { error } = await searchParams;

  return (
    <main className="auth-shell">
      <section className="auth-card">
        <div className="title-stack">
          <span className="eyebrow">Internal Admin</span>
          <h1>School Lab Admin</h1>
          <p className="muted">
            Sign in to view active sessions, today&apos;s usage, and device totals.
          </p>
        </div>

        <form action={loginAction}>
          <div className="field">
            <label htmlFor="password">Admin password</label>
            <input
              id="password"
              name="password"
              type="password"
              autoComplete="current-password"
              placeholder="Enter admin password"
              required
            />
          </div>

          {error === "invalid" ? (
            <p className="error-text">The password was not correct.</p>
          ) : null}

          <div className="button-row">
            <button className="button" type="submit">
              Sign in
            </button>
          </div>
        </form>
      </section>
    </main>
  );
}
