# School Lab Login MVP

This repository contains a tightly scoped MVP for a Windows desktop computer lab check-in system.

## Apps

- `apps/client-wpf`: Windows fullscreen lock/check-in client for the shared lab account.
- `apps/admin-web`: Next.js admin dashboard and thin server API for the WPF client.
- `supabase`: SQL migrations and seed data for the MVP schema.

## MVP Scope

- Fullscreen lock/check-in overlay on shared Windows lab PCs
- Student login/check-in
- Session start and end logging
- Idle time tracking
- Return to lock overlay after logout
- Simple admin dashboard for current sessions and usage history

## Out Of Scope

- BYOD, laptops, Chromebooks
- DNS or network analytics
- osquery or FleetDM
- Deep policy and remote-control systems
- Windows shell replacement or hard kiosk mode

## Local Setup

### Admin web app

1. `cd apps/admin-web`
2. Copy `.env.example` to `.env.local`
3. Fill in the Supabase and admin password values
4. `npm install`
5. `npm run dev`

### WPF client

The WPF project is scaffolded in `apps/client-wpf`. Build it on a Windows machine with the .NET desktop workload installed.

## Supabase

- Apply the SQL in `supabase/migrations`
- Seed initial demo data with `supabase/seed.sql`

## Pilot Workflow

1. Shared Windows lab account signs in
2. WPF overlay starts from Task Scheduler
3. Student checks in
4. Session is written to Supabase through the Next.js API
5. Student uses the PC while idle time is tracked locally
6. Student logs out from the client
7. Session closes in Supabase and the lock overlay returns
