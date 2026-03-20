import { logoutAction } from "@/app/login/actions";
import { requireAdminSession } from "@/lib/auth/admin-session";

export default async function AdminLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  await requireAdminSession();

  return (
    <>
      <div className="page-shell">
        <div className="page-header">
          <div className="title-stack">
            <span className="eyebrow">Admin Dashboard</span>
            <h1>School Lab Session Monitor</h1>
            <p className="muted">
              Track current check-ins and basic lab usage without extra analytics.
            </p>
          </div>

          <form action={logoutAction}>
            <button className="button ghost" type="submit">
              Sign out
            </button>
          </form>
        </div>

        {children}
      </div>
    </>
  );
}
