import { createServiceClient } from "@/lib/supabase/server";
import type { DashboardSessionRow, UsageSummary } from "@/lib/types";

type RawSessionRow = {
  id: string;
  start_at: string;
  end_at: string | null;
  idle_minutes: number | null;
  ended_reason: string | null;
  student:
    | {
        student_id: string;
        name: string;
      }
    | {
        student_id: string;
        name: string;
      }[]
    | null;
  device:
    | {
        hostname: string;
        location: string | null;
      }
    | {
        hostname: string;
        location: string | null;
      }[]
    | null;
};

function takeFirst<T>(value: T | T[] | null): T | null {
  if (Array.isArray(value)) {
    return value[0] ?? null;
  }

  return value;
}

function mapSessionRow(row: RawSessionRow): DashboardSessionRow {
  const student = takeFirst(row.student);
  const device = takeFirst(row.device);

  return {
    id: row.id,
    startAt: row.start_at,
    endAt: row.end_at,
    idleMinutes: row.idle_minutes ?? 0,
    endedReason: row.ended_reason,
    studentId: student?.student_id ?? "unknown",
    studentName: student?.name ?? "Unknown student",
    deviceHostname: device?.hostname ?? "Unknown device",
    deviceLocation: device?.location ?? null,
  };
}

function minutesBetween(startAt: string, endAt: string | null): number {
  if (!endAt) {
    return 0;
  }

  const elapsedMs =
    new Date(endAt).getTime() - new Date(startAt).getTime();

  return Math.max(0, Math.floor(elapsedMs / 60000));
}

function sortUsageDescending(entries: Map<string, number>): UsageSummary[] {
  return [...entries.entries()]
    .map(([label, minutes]) => ({ label, minutes }))
    .sort((left, right) => right.minutes - left.minutes);
}

function getTodayBounds(): { startOfToday: string; endOfToday: string } {
  const start = new Date();
  start.setHours(0, 0, 0, 0);

  const end = new Date(start);
  end.setDate(end.getDate() + 1);

  return {
    startOfToday: start.toISOString(),
    endOfToday: end.toISOString(),
  };
}

export async function getDashboardSnapshot() {
  const supabase = createServiceClient();
  const { startOfToday, endOfToday } = getTodayBounds();

  const activeQuery = supabase
    .from("sessions")
    .select(
      `
        id,
        start_at,
        end_at,
        idle_minutes,
        ended_reason,
        student:students!sessions_student_ref_fkey(student_id, name),
        device:devices!sessions_device_ref_fkey(hostname, location)
      `,
    )
    .is("end_at", null)
    .order("start_at", { ascending: false });

  const todayQuery = supabase
    .from("sessions")
    .select(
      `
        id,
        start_at,
        end_at,
        idle_minutes,
        ended_reason,
        student:students!sessions_student_ref_fkey(student_id, name),
        device:devices!sessions_device_ref_fkey(hostname, location)
      `,
    )
    .gte("start_at", startOfToday)
    .lt("start_at", endOfToday)
    .order("start_at", { ascending: false });

  const totalsQuery = supabase
    .from("sessions")
    .select(
      `
        id,
        start_at,
        end_at,
        idle_minutes,
        ended_reason,
        student:students!sessions_student_ref_fkey(student_id, name),
        device:devices!sessions_device_ref_fkey(hostname, location)
      `,
    )
    .not("end_at", "is", null)
    .order("start_at", { ascending: false });

  const [
    { data: activeData, error: activeError },
    { data: todayData, error: todayError },
    { data: totalsData, error: totalsError },
  ] = await Promise.all([activeQuery, todayQuery, totalsQuery]);

  const firstError = activeError ?? todayError ?? totalsError;

  if (firstError) {
    throw new Error(firstError.message);
  }

  const activeSessions = (activeData ?? []).map((row) =>
    mapSessionRow(row as RawSessionRow),
  );
  const todaySessions = (todayData ?? []).map((row) =>
    mapSessionRow(row as RawSessionRow),
  );
  const closedSessions = (totalsData ?? []).map((row) =>
    mapSessionRow(row as RawSessionRow),
  );

  const usageByStudentMap = new Map<string, number>();
  const usageByDeviceMap = new Map<string, number>();

  for (const session of closedSessions) {
    const sessionMinutes = minutesBetween(session.startAt, session.endAt);

    usageByStudentMap.set(
      `${session.studentName} (${session.studentId})`,
      (usageByStudentMap.get(
        `${session.studentName} (${session.studentId})`,
      ) ?? 0) + sessionMinutes,
    );

    usageByDeviceMap.set(
      session.deviceHostname,
      (usageByDeviceMap.get(session.deviceHostname) ?? 0) + sessionMinutes,
    );
  }

  return {
    metrics: {
      activeSessions: activeSessions.length,
      todaySessions: todaySessions.length,
      uniqueStudentsToday: new Set(todaySessions.map((session) => session.studentId))
        .size,
      uniqueDevicesToday: new Set(
        todaySessions.map((session) => session.deviceHostname),
      ).size,
    },
    activeSessions,
    todaySessions,
    usageByStudent: sortUsageDescending(usageByStudentMap),
    usageByDevice: sortUsageDescending(usageByDeviceMap),
  };
}
