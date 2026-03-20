import { NextResponse } from "next/server";

import { createServiceClient } from "@/lib/supabase/server";
import {
  normalizeHostname,
  normalizeMacAddress,
} from "@/lib/validation/client";

export async function GET(request: Request) {
  try {
    const url = new URL(request.url);
    const hostname = normalizeHostname(url.searchParams.get("hostname") ?? "");
    const macAddress = normalizeMacAddress(
      url.searchParams.get("macAddress") ?? "",
    );

    if (!hostname || !macAddress) {
      return NextResponse.json(
        { error: "hostname and macAddress are required." },
        { status: 400 },
      );
    }

    const supabase = createServiceClient();

    const { data: device, error: deviceError } = await supabase
      .from("devices")
      .select("id")
      .eq("hostname", hostname)
      .eq("mac_address", macAddress)
      .eq("is_active", true)
      .maybeSingle();

    if (deviceError || !device) {
      return NextResponse.json(
        { activeSession: null, error: "Device was not found." },
        { status: 404 },
      );
    }

    const { data: activeSession, error: sessionError } = await supabase
      .from("sessions")
      .select(
        `
          id,
          start_at,
          student:students!sessions_student_ref_fkey(student_id, name)
        `,
      )
      .eq("device_ref", device.id)
      .is("end_at", null)
      .maybeSingle();

    if (sessionError) {
      return NextResponse.json(
        { error: "Unable to fetch the current session." },
        { status: 500 },
      );
    }

    if (!activeSession) {
      return NextResponse.json({ activeSession: null });
    }

    const student = Array.isArray(activeSession.student)
      ? activeSession.student[0]
      : activeSession.student;

    return NextResponse.json({
      activeSession: {
        sessionId: activeSession.id,
        studentId: student?.student_id ?? "unknown",
        studentName: student?.name ?? "Unknown student",
        startAt: activeSession.start_at,
      },
    });
  } catch (error) {
    return NextResponse.json(
      {
        error:
          error instanceof Error ? error.message : "Unexpected current-session error.",
      },
      { status: 400 },
    );
  }
}
