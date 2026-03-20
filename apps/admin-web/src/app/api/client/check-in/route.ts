import { NextResponse } from "next/server";

import { createServiceClient } from "@/lib/supabase/server";
import { parseCheckInPayload } from "@/lib/validation/client";

export async function POST(request: Request) {
  try {
    const payload = parseCheckInPayload(await request.json());
    const supabase = createServiceClient();

    const [{ data: student, error: studentError }, { data: device, error: deviceError }] =
      await Promise.all([
        supabase
          .from("students")
          .select("id, student_id, name")
          .eq("student_id", payload.studentId)
          .eq("is_active", true)
          .maybeSingle(),
        supabase
          .from("devices")
          .select("id, hostname")
          .eq("hostname", payload.hostname)
          .eq("mac_address", payload.macAddress)
          .eq("is_active", true)
          .maybeSingle(),
      ]);

    if (studentError || !student) {
      return NextResponse.json(
        { error: "Student was not found or is inactive." },
        { status: 404 },
      );
    }

    if (deviceError || !device) {
      return NextResponse.json(
        { error: "Device was not found or is inactive." },
        { status: 404 },
      );
    }

    const [
      { data: deviceSession, error: deviceSessionError },
      { data: studentSession, error: studentSessionError },
    ] = await Promise.all([
      supabase
        .from("sessions")
        .select("id")
        .eq("device_ref", device.id)
        .is("end_at", null)
        .maybeSingle(),
      supabase
        .from("sessions")
        .select("id")
        .eq("student_ref", student.id)
        .is("end_at", null)
        .maybeSingle(),
    ]);

    if (deviceSessionError || studentSessionError) {
      return NextResponse.json(
        { error: "Unable to validate existing sessions." },
        { status: 500 },
      );
    }

    if (deviceSession) {
      return NextResponse.json(
        { error: "This device already has an active session." },
        { status: 409 },
      );
    }

    if (studentSession) {
      return NextResponse.json(
        { error: "This student already has an active session." },
        { status: 409 },
      );
    }

    const { data: newSession, error: insertError } = await supabase
      .from("sessions")
      .insert({
        student_ref: student.id,
        device_ref: device.id,
      })
      .select("id, start_at")
      .single();

    if (insertError || !newSession) {
      const status = insertError?.code === "23505" ? 409 : 500;

      return NextResponse.json(
        { error: insertError?.message ?? "Failed to create session." },
        { status },
      );
    }

    return NextResponse.json({
      sessionId: newSession.id,
      student: {
        id: student.id,
        studentId: student.student_id,
        name: student.name,
      },
      device: {
        id: device.id,
        hostname: device.hostname,
      },
      startAt: newSession.start_at,
    });
  } catch (error) {
    return NextResponse.json(
      {
        error:
          error instanceof Error ? error.message : "Unexpected check-in error.",
      },
      { status: 400 },
    );
  }
}
