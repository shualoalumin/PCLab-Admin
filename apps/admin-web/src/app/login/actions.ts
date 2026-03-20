"use server";

import { redirect } from "next/navigation";

import {
  clearAdminSession,
  createAdminSession,
} from "@/lib/auth/admin-session";
import { getAdminPassword } from "@/lib/config";

export async function loginAction(formData: FormData): Promise<void> {
  const submittedPassword = String(formData.get("password") ?? "").trim();

  if (submittedPassword !== getAdminPassword()) {
    redirect("/login?error=invalid");
  }

  await createAdminSession();
  redirect("/dashboard");
}

export async function logoutAction(): Promise<void> {
  await clearAdminSession();
  redirect("/login");
}
