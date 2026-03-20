import { createHash } from "node:crypto";

import { cookies } from "next/headers";
import { redirect } from "next/navigation";

import { getAdminPassword, getAdminSessionSalt } from "@/lib/config";

const ADMIN_COOKIE_NAME = "pc_lab_admin";

function buildSessionToken(): string {
  return createHash("sha256")
    .update(`${getAdminPassword()}:${getAdminSessionSalt()}`)
    .digest("hex");
}

export async function hasAdminSession(): Promise<boolean> {
  const cookieStore = await cookies();
  const cookieValue = cookieStore.get(ADMIN_COOKIE_NAME)?.value;
  return cookieValue === buildSessionToken();
}

export async function requireAdminSession(): Promise<void> {
  const authenticated = await hasAdminSession();

  if (!authenticated) {
    redirect("/login");
  }
}

export async function createAdminSession(): Promise<void> {
  const cookieStore = await cookies();

  cookieStore.set(ADMIN_COOKIE_NAME, buildSessionToken(), {
    httpOnly: true,
    sameSite: "lax",
    secure: process.env.NODE_ENV === "production",
    path: "/",
    maxAge: 60 * 60 * 8,
  });
}

export async function clearAdminSession(): Promise<void> {
  const cookieStore = await cookies();
  cookieStore.delete(ADMIN_COOKIE_NAME);
}
