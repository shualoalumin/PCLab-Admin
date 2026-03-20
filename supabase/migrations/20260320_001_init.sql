create extension if not exists pgcrypto;

create table public.students (
  id uuid primary key default gen_random_uuid(),
  student_id text not null unique,
  name text not null,
  grade text,
  class_name text,
  is_active boolean not null default true
);

create table public.devices (
  id uuid primary key default gen_random_uuid(),
  hostname text not null unique,
  mac_address text not null unique,
  device_type text not null default 'windows_lab_pc',
  location text,
  is_active boolean not null default true
);

create table public.sessions (
  id uuid primary key default gen_random_uuid(),
  student_ref uuid not null references public.students(id),
  device_ref uuid not null references public.devices(id),
  start_at timestamptz not null default now(),
  end_at timestamptz,
  idle_minutes integer not null default 0 check (idle_minutes >= 0),
  ended_reason text,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  constraint sessions_end_after_start check (end_at is null or end_at >= start_at)
);

create index sessions_device_start_idx
  on public.sessions (device_ref, start_at desc);

create index sessions_student_start_idx
  on public.sessions (student_ref, start_at desc);

create index sessions_start_idx
  on public.sessions (start_at desc);

create unique index sessions_one_open_per_device_idx
  on public.sessions (device_ref)
  where end_at is null;

create unique index sessions_one_open_per_student_idx
  on public.sessions (student_ref)
  where end_at is null;

alter table public.students enable row level security;
alter table public.devices enable row level security;
alter table public.sessions enable row level security;
