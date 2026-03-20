insert into public.students (student_id, name, grade, class_name, is_active)
values
  ('20260001', 'Kim Student', '10', 'A', true),
  ('20260002', 'Lee Student', '10', 'A', true),
  ('20260003', 'Park Student', '11', 'B', true)
on conflict (student_id) do update
set
  name = excluded.name,
  grade = excluded.grade,
  class_name = excluded.class_name,
  is_active = excluded.is_active;

insert into public.devices (hostname, mac_address, device_type, location, is_active)
values
  ('LAB-PC-01', 'AA-BB-CC-DD-EE-FF', 'windows_lab_pc', 'Computer Lab A', true)
on conflict (hostname) do update
set
  mac_address = excluded.mac_address,
  device_type = excluded.device_type,
  location = excluded.location,
  is_active = excluded.is_active;
