# PC Lab Student Check-in Management System

## Project Logic Overview

**Project Type:** Capstone / Internal School Operations Tool  
**Current Scope:** Windows desktop PC lab only  
**Target Environment:** 16 shared Windows PCs in the computer lab  
**Current Focus:** MVP for 1-PC demo, then scale to 16 PCs

---

# 1. Project Purpose

The school computer lab is currently managed with a paper-based usage log.

Students manually write down their usage records, which creates several problems:

- records are inconsistent
- actual usage time is hard to verify
- it is difficult to know who used which PC and when
- there is no usable dashboard or summary for teachers/admins

The goal of this project is to replace the paper log with a **student check-in based PC usage management system**.

This is **not** a full device management platform at this stage.

---

# 2. Core Product Definition

This system adds a **second-layer student login/check-in flow** on top of the existing Windows login.

### Current lab environment

- Each lab PC is already logged into Windows using a shared/common account
- Students do **not** log into Windows individually
- Therefore, student identity must be captured by a separate application

### Core concept

When a student sits down at a PC:

1. A fullscreen lock/check-in overlay is already visible
2. The student must enter their own information
3. Only after successful check-in can they use the PC
4. When finished, they log out from the system
5. The PC returns to the lock/check-in screen for the next student

This creates a clear session-based workflow for shared lab computers.

---

# 3. MVP Scope

## In Scope

The MVP includes only the minimum necessary features for a working demo in the PC lab.

### Student-side
- fullscreen lock/check-in overlay on Windows
- student check-in using:
  - student ID
  - student name
  - class selection (optional but preferred)
- unlock flow after successful login
- logout flow after use
- automatic return to lock screen after logout

### Session tracking
- session start timestamp
- session end timestamp
- linked student
- linked device
- idle time accumulation during usage
- simple ended reason tracking

### Admin-side
- dashboard showing active sessions
- today's usage log
- total usage time by student
- total usage time by device

---

## Out of Scope for MVP

The following are intentionally postponed:

- laptop / BYOD management
- Chromebook integration
- Pi-hole
- DNS logging
- `dns_logs` table
- osquery / FleetDM
- advanced process analytics
- `app_summaries`
- top-domain analytics
- complex policy engine
- advanced teacher override system
- enterprise-scale remote device control

These can be added later if the MVP proves valuable.

---

# 4. Product Goal

The product goal is not to build a complex monitoring platform first.

The immediate goal is to prove this workflow:

```text
Shared Windows login active
↓
Fullscreen student check-in overlay appears
↓
Student enters information
↓
Session starts
↓
Student uses PC
↓
Idle time is tracked
↓
Student logs out
↓
Session ends
↓
Overlay returns for next student
```

If this works reliably on 1 PC, the core product logic is validated.

---

# 5. High-Level System Architecture

The system has three main parts:

```text
[ WPF Client on Lab PC ]
        ↓
[ Supabase Backend / Database ]
        ↓
[ Admin Dashboard ]
```

## 5.1 WPF Client
Runs on each lab PC.

Responsibilities:
- show fullscreen lock/check-in overlay
- collect student info
- start session
- track idle time
- end session on logout
- return to lock screen

## 5.2 Supabase Backend
Stores core data.

Responsibilities:
- store student records
- store device records
- store session records
- provide API/database access for client and dashboard

## 5.3 Admin Dashboard
Simple web dashboard for teachers/admins.

Responsibilities:
- show current active sessions
- show today's sessions
- show total usage by student
- show total usage by device

---

# 6. Core User Flow

## 6.1 PC Startup / Ready State

The lab PC is already logged into a shared Windows account.

The check-in overlay launches automatically.

### Expected state
- fullscreen
- topmost
- visible immediately
- normal PC usage blocked until check-in

---

## 6.2 Student Check-in

The student sees the check-in screen and enters:

- student ID
- student name
- class (optional)

The system validates the student record.

If valid:
- create a new session record
- save current device identity
- save start time
- unlock usage state

---

## 6.3 Active Session

While the student is using the PC:

- the session remains active
- idle time is measured in the background
- the client keeps track of session state locally

At MVP stage, deep monitoring is not required.

Only basic session state and idle accumulation are needed.

---

## 6.4 Logout

When the student finishes:

- they click logout inside the solution
- session end time is recorded
- ended reason is stored
- overlay returns immediately

This prepares the PC for the next student.

---

## 6.5 Crash / Unexpected Close

If the app is forced closed or the PC shuts down unexpectedly:

- session may remain open temporarily
- fallback logic should detect abnormal termination later
- MVP can mark this as a simple abnormal end case

This should be handled simply, not overengineered.

---

# 7. Data Model

The MVP data model should stay minimal.

## 7.1 students

Stores student identity records.

```sql
students (
  id uuid primary key,
  student_id text unique not null,
  name text not null,
  grade integer,
  class_name text,
  is_active boolean default true,
  created_at timestamptz default now(),
  updated_at timestamptz default now()
)
```

## 7.2 devices

Stores lab PC identity records.

```sql
devices (
  id uuid primary key,
  hostname text unique not null,
  mac_address text unique,
  device_type text default 'lab_pc',
  location text,
  is_active boolean default true,
  created_at timestamptz default now(),
  updated_at timestamptz default now()
)
```

## 7.3 sessions

Stores session history.

```sql
sessions (
  id uuid primary key,
  student_ref uuid not null references students(id),
  device_ref uuid not null references devices(id),
  start_at timestamptz not null,
  end_at timestamptz,
  idle_minutes integer default 0,
  ended_reason text,
  created_at timestamptz default now(),
  updated_at timestamptz default now()
)
```

---

# 8. Session Logic

## 8.1 Session Start
Triggered when student successfully checks in.

Actions:
- resolve student record
- resolve device record
- create new session row
- set session as active on client
- unlock UI

## 8.2 Session Active State
While session is active:

- idle detection loop runs
- usage time continues
- logout button remains available

## 8.3 Session End
Triggered when student clicks logout.

Actions:
- compute final idle minutes
- update `end_at`
- update `ended_reason`
- return to lock screen

## 8.4 Abnormal End
Triggered by:
- app crash
- forced close
- reboot
- shutdown without logout

MVP handling:
- session remains without `end_at`
- later cleanup can mark abnormal closure
- detailed recovery can be added later

---

# 9. Idle Detection Logic

Idle tracking is part of the MVP because actual usage time matters.

## Basic principle
- if no keyboard/mouse input occurs for a threshold period, mark that span as idle
- accumulate total idle minutes during the session

## Technical direction
Use Windows idle detection such as:

- `GetLastInputInfo`
- periodic polling every few seconds

## MVP simplification
- use a simple threshold, such as 3 to 5 minutes
- accumulate rounded idle minutes
- do not overbuild activity classification yet

---

# 10. Lock Screen Logic

The lock/check-in overlay is the key product behavior.

## Required behavior
- fullscreen
- always on top
- visible at startup
- blocks ordinary interaction before student login
- returns after logout

## MVP design goals
- reliable enough for demo
- simple enough to build quickly
- future extensible

## MVP simplifications
At first, the overlay does **not** need to implement extreme kiosk-grade lockdown.

It only needs to demonstrate:
- strong fullscreen visibility
- obvious blocked state
- clean unlock / relock workflow

More aggressive blocking can be added later.

---

# 11. Repository Structure

A practical repository structure for MVP:

```text
pc-lab-checkin-system/
  README.md
  docs/
    project-logic-overview.md
    sprint-plan.md
  client/
    PcLabCheckin.App/
  dashboard/
    admin-dashboard/
  database/
    schema.sql
    seed.sql
```

## Why this is enough
- `docs/` keeps planning and logic organized
- `client/` holds the Windows WPF application
- `dashboard/` holds the admin web dashboard
- `database/` stores SQL schema and seed files

This keeps the MVP understandable and simple.

---

# 12. Implementation Phases

## Phase 0. Planning and Setup
Goal:
- finalize scope
- choose stack
- define data model
- create Supabase project
- create device naming rules

Deliverables:
- architecture document
- schema draft
- repo structure

---

## Phase 1. UI-only WPF Prototype
Goal:
- prove the lock/check-in flow visually on 1 PC

Build:
- fullscreen check-in overlay
- student ID input
- student name input
- login button
- unlocked state
- logout button
- return to lock screen

No real backend yet.
Use mock data only.

Deliverable:
- 1-PC visual demo

---

## Phase 2. Real Session Persistence
Goal:
- connect the client to Supabase

Build:
- students table
- devices table
- sessions table
- login creates session
- logout ends session

Deliverable:
- real session records stored in DB

---

## Phase 3. Idle Tracking and Session Reliability
Goal:
- make the session lifecycle realistic

Build:
- idle detection
- active session state handling
- abnormal end fallback strategy
- startup behavior cleanup

Deliverable:
- session history with idle minutes

---

## Phase 4. Admin Dashboard
Goal:
- allow teachers/admins to see usage data

Build:
- active sessions list
- today's sessions table
- total usage by student
- total usage by device

Deliverable:
- basic dashboard with real data

---

# 13. Success Criteria

## 1-PC Demo Success
The MVP is successful if one lab PC can do the following reliably:

```text
PC ready
↓
check-in overlay shown
↓
student enters info
↓
session starts
↓
student uses PC
↓
student logs out
↓
session ends
↓
overlay returns
```

## 16-PC Rollout Success
After the 1-PC demo works, rollout is successful if:

- the same app can be installed on all 16 lab PCs
- each device identifies itself correctly
- sessions are stored consistently
- dashboard can summarize usage across devices

---

# 14. Future Expansion Path

These are possible future extensions after MVP succeeds.

## Device Expansion
- laptop support
- BYOD workflows
- Chromebook support if school eligibility changes

## Monitoring Expansion
- DNS logging
- Pi-hole integration
- app/process summaries
- richer analytics

## Admin Expansion
- teacher override
- remote lock/unlock
- abnormal session cleanup tools
- deeper reporting

Important:
These should remain future scope, not current implementation scope.

---

# 15. Strategic Principle

This project should not begin as a giant school infrastructure platform.

It should begin as a **focused, reliable shared-PC check-in system**.

The right build order is:

1. make the student check-in lock flow work
2. make session logging real
3. make dashboard useful
4. expand only after real usage proves value

---

# 16. One-Line Project Definition

> A Windows PC lab management system where students must check in before using a shared computer, and each usage session is automatically recorded.

---

# 17. Recommended Next Step

The immediate next step is:

**Build the UI-only WPF prototype for one PC first.**

That milestone should include:
- fullscreen overlay
- student ID / name inputs
- fake login
- unlocked state
- logout
- relock

That is the smallest buildable proof of the entire product concept.
