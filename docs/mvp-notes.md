# MVP Notes

## Product Boundaries

- This project manages shared Windows desktop lab PCs only.
- The first milestone targets a single proof-of-concept device.
- The same design should scale to 16 lab PCs without architecture changes.

## Required User Flow

1. The shared Windows lab account signs in.
2. The fullscreen lock/check-in overlay is already visible.
3. The student enters a valid student ID.
4. The server creates a session linked to the student and device.
5. The student uses the PC.
6. The client accumulates idle minutes locally.
7. The student ends the session from the client.
8. The server closes the session.
9. The lock/check-in overlay returns immediately.

## MVP Design Decisions

- Use one WPF desktop app, not a Windows service.
- Launch the app at user logon with Task Scheduler.
- Keep the client fail-closed: uncertain recovery should always return to the lock overlay.
- Keep the backend simple: Next.js route handlers plus Supabase Postgres.
- Keep dashboard auth simple with a server-side password gate for internal admin use.

## Postponed On Purpose

- Hard kiosk enforcement
- Device secrets and strong endpoint identity
- Heartbeats or realtime sync
- Teacher override workflows
- Rich analytics and process telemetry
