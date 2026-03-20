export type CheckInPayload = {
  studentId: string;
  hostname: string;
  macAddress: string;
};

export type LogoutPayload = {
  sessionId: string;
  idleMinutes: number;
  endedReason: string;
};

function requireString(value: unknown, fieldName: string): string {
  if (typeof value !== "string" || value.trim() === "") {
    throw new Error(`Invalid ${fieldName}`);
  }

  return value.trim();
}

export function normalizeHostname(hostname: string): string {
  return hostname.trim().toUpperCase();
}

export function normalizeMacAddress(macAddress: string): string {
  return macAddress.trim().replaceAll(":", "-").toUpperCase();
}

export function parseCheckInPayload(payload: unknown): CheckInPayload {
  if (!payload || typeof payload !== "object") {
    throw new Error("Invalid request body");
  }

  const record = payload as Record<string, unknown>;

  return {
    studentId: requireString(record.studentId, "studentId"),
    hostname: normalizeHostname(requireString(record.hostname, "hostname")),
    macAddress: normalizeMacAddress(
      requireString(record.macAddress, "macAddress"),
    ),
  };
}

export function parseLogoutPayload(payload: unknown): LogoutPayload {
  if (!payload || typeof payload !== "object") {
    throw new Error("Invalid request body");
  }

  const record = payload as Record<string, unknown>;
  const idleMinutes = Number(record.idleMinutes ?? 0);

  if (!Number.isFinite(idleMinutes) || idleMinutes < 0) {
    throw new Error("Invalid idleMinutes");
  }

  return {
    sessionId: requireString(record.sessionId, "sessionId"),
    idleMinutes: Math.floor(idleMinutes),
    endedReason: requireString(record.endedReason, "endedReason"),
  };
}
