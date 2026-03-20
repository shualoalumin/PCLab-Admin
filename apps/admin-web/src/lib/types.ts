export type StudentSummary = {
  id: string;
  studentId: string;
  name: string;
  grade: string | null;
  className: string | null;
};

export type DeviceSummary = {
  id: string;
  hostname: string;
  macAddress: string;
  location: string | null;
};

export type ActiveSession = {
  sessionId: string;
  studentId: string;
  studentName: string;
  startAt: string;
};

export type DashboardSessionRow = {
  id: string;
  startAt: string;
  endAt: string | null;
  idleMinutes: number;
  endedReason: string | null;
  studentId: string;
  studentName: string;
  deviceHostname: string;
  deviceLocation: string | null;
};

export type UsageSummary = {
  label: string;
  minutes: number;
};
