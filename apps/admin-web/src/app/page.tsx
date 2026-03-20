import { redirect } from "next/navigation";

import { hasAdminSession } from "@/lib/auth/admin-session";

export default async function Home() {
  if (await hasAdminSession()) {
    redirect("/dashboard");
  }

  redirect("/login");
}
