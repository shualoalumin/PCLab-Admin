import { NextResponse } from "next/server";

import { createServiceClient } from "@/lib/supabase/server";
import { parseLogoutPayload } from "@/lib/validation/client";

export async function POST(request: Request) {
  try {
    const payload = parseLogoutPayload(await request.json());
    const supabase = createServiceClient();

    const { data: existingSession, error: sessionError } = await supabase
      .from("sessions")
      .select("id")
      .eq("id", payload.sessionId)
      .is("end_at", null)
      .maybeSingle();

    if (sessionError) {
      return NextResponse.json(
        { error: "Unable to validate the active session." },
        { status: 500 },
      );
    }

    if (!existingSession) {
      return NextResponse.json(
        { error: "Active session was not found." },
        { status: 404 },
      );
    }

    const endAt = new Date().toISOString();

    const { data: updatedSession, error: updateError } = await supabase
      .from("sessions")
      .update({
        end_at: endAt,
        idle_minutes: payload.idleMinutes,
        ended_reason: payload.endedReason,
        updated_at: endAt,
      })
      .eq("id", payload.sessionId)
      .is("end_at", null)
      .select("id, end_at")
      .single();

    if (updateError || !updatedSession) {
      return NextResponse.json(
        { error: updateError?.message ?? "Failed to close the session." },
        { status: 500 },
      );
    }

    return NextResponse.json({
      sessionId: updatedSession.id,
      endAt: updatedSession.end_at,
    });
  } catch (error) {
    return NextResponse.json(
      {
        error:
          error instanceof Error ? error.message : "Unexpected logout error.",
      },
      { status: 400 },
    );
  }
}
