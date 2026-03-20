function getRequiredEnv(name: string): string {
  const value = process.env[name];

  if (!value) {
    throw new Error(`Missing required environment variable: ${name}`);
  }

  return value;
}

export function getSupabaseUrl(): string {
  return getRequiredEnv("NEXT_PUBLIC_SUPABASE_URL");
}

export function getSupabaseServiceRoleKey(): string {
  return getRequiredEnv("SUPABASE_SERVICE_ROLE_KEY");
}

export function getAdminPassword(): string {
  return getRequiredEnv("ADMIN_PASSWORD");
}

export function getAdminSessionSalt(): string {
  return getRequiredEnv("ADMIN_SESSION_SALT");
}
