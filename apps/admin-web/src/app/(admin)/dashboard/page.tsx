import { DashboardSection } from "@/components/dashboard-section";
import { MetricCard } from "@/components/metric-card";
import { getDashboardSnapshot } from "@/lib/queries/dashboard";

function formatMinutes(minutes: number): string {
  const hours = Math.floor(minutes / 60);
  const remainingMinutes = minutes % 60;

  if (hours === 0) {
    return `${remainingMinutes}m`;
  }

  return `${hours}h ${remainingMinutes}m`;
}

function formatDateTime(value: string): string {
  return new Intl.DateTimeFormat("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

export default async function DashboardPage() {
  const snapshot = await getDashboardSnapshot();

  return (
    <>
      <section className="card-grid">
        <MetricCard
          label="Active sessions"
          value={String(snapshot.metrics.activeSessions)}
          hint="Currently checked-in students"
        />
        <MetricCard
          label="Today's sessions"
          value={String(snapshot.metrics.todaySessions)}
          hint="Sessions started today"
        />
        <MetricCard
          label="Students today"
          value={String(snapshot.metrics.uniqueStudentsToday)}
          hint="Distinct students with activity"
        />
        <MetricCard
          label="Devices today"
          value={String(snapshot.metrics.uniqueDevicesToday)}
          hint="Distinct lab PCs used today"
        />
      </section>

      <DashboardSection
        title="Current Active Sessions"
        hint="Open sessions that have not been logged out yet."
      >
        {snapshot.activeSessions.length === 0 ? (
          <div className="empty-state">No students are currently checked in.</div>
        ) : (
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Student</th>
                  <th>Device</th>
                  <th>Started</th>
                  <th>Idle</th>
                </tr>
              </thead>
              <tbody>
                {snapshot.activeSessions.map((session) => (
                  <tr key={session.id}>
                    <td>
                      <strong>{session.studentName}</strong>
                      <div className="muted">{session.studentId}</div>
                    </td>
                    <td>
                      <strong>{session.deviceHostname}</strong>
                      <div className="muted">
                        {session.deviceLocation ?? "Lab location not set"}
                      </div>
                    </td>
                    <td>{formatDateTime(session.startAt)}</td>
                    <td>{formatMinutes(session.idleMinutes)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </DashboardSection>

      <DashboardSection
        title="Today's Session List"
        hint="All sessions that started today, including completed sessions."
      >
        {snapshot.todaySessions.length === 0 ? (
          <div className="empty-state">No sessions have started today yet.</div>
        ) : (
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Student</th>
                  <th>Device</th>
                  <th>Started</th>
                  <th>Ended</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {snapshot.todaySessions.map((session) => (
                  <tr key={session.id}>
                    <td>
                      <strong>{session.studentName}</strong>
                      <div className="muted">{session.studentId}</div>
                    </td>
                    <td>{session.deviceHostname}</td>
                    <td>{formatDateTime(session.startAt)}</td>
                    <td>{session.endAt ? formatDateTime(session.endAt) : "-"}</td>
                    <td>
                      <span
                        className={`badge ${session.endAt ? "success" : ""}`.trim()}
                      >
                        {session.endAt ? session.endedReason ?? "completed" : "active"}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </DashboardSection>

      <DashboardSection
        title="Total Usage By Student"
        hint="Completed-session time totals only."
      >
        {snapshot.usageByStudent.length === 0 ? (
          <div className="empty-state">No completed sessions yet.</div>
        ) : (
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Student</th>
                  <th>Total usage</th>
                </tr>
              </thead>
              <tbody>
                {snapshot.usageByStudent.map((entry) => (
                  <tr key={entry.label}>
                    <td>{entry.label}</td>
                    <td>{formatMinutes(entry.minutes)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </DashboardSection>

      <DashboardSection
        title="Total Usage By Device"
        hint="Completed-session time totals grouped by lab PC."
      >
        {snapshot.usageByDevice.length === 0 ? (
          <div className="empty-state">No device usage totals yet.</div>
        ) : (
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Device</th>
                  <th>Total usage</th>
                </tr>
              </thead>
              <tbody>
                {snapshot.usageByDevice.map((entry) => (
                  <tr key={entry.label}>
                    <td>{entry.label}</td>
                    <td>{formatMinutes(entry.minutes)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </DashboardSection>
    </>
  );
}
