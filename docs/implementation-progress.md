# School Lab Login MVP Implementation Progress

## Overview

This document summarizes the implementation work completed for the Windows desktop school lab MVP.

The implementation follows the approved MVP scope:

- WPF fullscreen lock/check-in overlay
- student login/check-in
- session start/end logging
- idle time tracking
- return to lock overlay after logout
- simple admin dashboard
- one-PC-first design that can scale to 16 shared lab PCs

## What Was Built

### 1. Repository structure

Created the minimal project structure described in the plan:

- `apps/client-wpf`
- `apps/admin-web`
- `supabase/migrations`
- `supabase/seed.sql`
- `docs`
- `README.md`

This keeps the repo limited to the two actual deployables and the database source of truth.

### 2. Supabase database layer

Added the MVP schema in `supabase/migrations/20260320_001_init.sql`.

Implemented:

- `students`
- `devices`
- `sessions`

Included:

- UUID primary keys
- foreign keys from `sessions` to `students` and `devices`
- partial unique indexes to prevent multiple open sessions per student or per device
- RLS enabled on all three public tables

Added demo seed data in `supabase/seed.sql` for:

- sample students
- one pilot lab PC device

### 3. Next.js admin app and thin API

Generated and adapted `apps/admin-web` as the single web deployable for:

- admin dashboard
- thin server API used by the WPF client

Implemented the three MVP client endpoints:

- `POST /api/client/check-in`
- `POST /api/client/logout`
- `GET /api/client/current`

Implemented server-side Supabase access only:

- `src/lib/supabase/server.ts`
- service-role key kept on the server side

Implemented simple internal admin auth:

- password login page
- cookie-based admin session
- protected dashboard layout

### 4. Admin dashboard

Implemented the requested MVP dashboard views in `apps/admin-web/src/app/(admin)/dashboard/page.tsx`.

Dashboard includes only:

- current active sessions
- today's session list
- total usage time by student
- total usage time by device

No extra analytics, network reporting, or Realtime dependencies were added.

### 5. WPF client vertical slice

Scaffolded the WPF client in `apps/client-wpf`.

Implemented:

- fullscreen topmost lock overlay
- student check-in form
- session bar with logout action
- local session state persistence
- startup recovery logic
- idle time tracking using `GetLastInputInfo`
- fail-closed startup behavior

Key files:

- `src/PCLab.Client/Views/ShellWindow.xaml`
- `src/PCLab.Client/Views/SessionBarWindow.xaml`
- `src/PCLab.Client/Services/SessionCoordinator.cs`
- `src/PCLab.Client/Services/IdleTracker.cs`
- `src/PCLab.Client/Services/LocalStateStore.cs`
- `src/PCLab.Client/Services/ApiClient.cs`

### 6. Windows startup automation

Added `apps/client-wpf/scripts/Register-PCLabClientTask.ps1`.

This script registers a Task Scheduler logon task for the shared lab account so the client auto-starts and can be restarted on failure.

## Session Lifecycle Implemented

### Startup

- client starts
- local state is loaded
- current open device session is checked from the server
- if recovery is uncertain, the app returns to locked mode
- if an orphaned open session exists, it is closed with `app_recovery`

### Check-in

- student enters student ID
- client sends student ID + device identity
- server validates active student and active device
- server ensures there is no conflicting open session
- server creates the session row

### Active session

- overlay is hidden
- session bar is shown
- idle time is tracked locally
- local session state is updated periodically

### Logout

- student ends session from the session bar
- client posts final idle minutes and `student_logout`
- server closes the session
- local state is cleared
- overlay returns

## Validation Completed

### Web app

Validated successfully:

- `npm run lint`
- `npm run build`

The web app builds with placeholder environment variables for:

- `NEXT_PUBLIC_SUPABASE_URL`
- `SUPABASE_SERVICE_ROLE_KEY`
- `ADMIN_PASSWORD`
- `ADMIN_SESSION_SALT`

### WPF client

Could not build in this environment because the `dotnet` SDK is not installed on the machine running the agent.

The WPF project files were still created and wired for a Windows machine with the .NET desktop workload installed.

## Known Limits

These remain intentionally within MVP constraints:

- no Windows service
- no shell replacement
- no hard kiosk enforcement
- no dashboard analytics beyond the required four views
- no direct client access to Supabase
- no advanced recovery heartbeat
- no teacher override workflow

## Recommended Next Steps For Demo

1. Apply `supabase/migrations/20260320_001_init.sql` to the target Supabase project.
2. Run `supabase/seed.sql`.
3. Create `apps/admin-web/.env.local` from `.env.example`.
4. Run the Next.js app and confirm the three API endpoints against real Supabase data.
5. Build the WPF client on a Windows machine with the .NET desktop workload installed.
6. Register the Task Scheduler startup task for the shared lab account.
7. Test the 1-PC pilot flow end to end.

## Workflow Update

Established a default project work routine for future tasks:

- think broadly before choosing the next implementation step
- manage progress through the work todo flow
- update a markdown progress document in `docs/` whenever a work todo is completed
- commit and push after each completed todo milestone when git state is safe

Artifacts added for this routine:

- `.cursor/rules/work-routine.mdc`
- `docs/workflow-routine.md`
