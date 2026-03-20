import test from "node:test";
import assert from "node:assert/strict";

import {
  normalizeHostname,
  normalizeMacAddress,
  parseCheckInPayload,
  parseLogoutPayload,
} from "./client";

test("normalizeHostname uppercases and trims values", () => {
  assert.equal(normalizeHostname("  lab-pc-01 "), "LAB-PC-01");
});

test("normalizeMacAddress converts colon-delimited values", () => {
  assert.equal(
    normalizeMacAddress("aa:bb:cc:dd:ee:ff"),
    "AA-BB-CC-DD-EE-FF",
  );
});

test("parseCheckInPayload returns normalized payload", () => {
  assert.deepEqual(
    parseCheckInPayload({
      studentId: "20260017",
      hostname: "lab-pc-01",
      macAddress: "aa:bb:cc:dd:ee:ff",
    }),
    {
      studentId: "20260017",
      hostname: "LAB-PC-01",
      macAddress: "AA-BB-CC-DD-EE-FF",
    },
  );
});

test("parseCheckInPayload rejects empty student id", () => {
  assert.throws(
    () =>
      parseCheckInPayload({
        studentId: "",
        hostname: "LAB-PC-01",
        macAddress: "AA-BB-CC-DD-EE-FF",
      }),
    /Invalid studentId/,
  );
});

test("parseLogoutPayload normalizes idle minutes", () => {
  assert.deepEqual(
    parseLogoutPayload({
      sessionId: "session-1",
      idleMinutes: 14.9,
      endedReason: "student_logout",
    }),
    {
      sessionId: "session-1",
      idleMinutes: 14,
      endedReason: "student_logout",
    },
  );
});

test("parseLogoutPayload rejects negative idle minutes", () => {
  assert.throws(
    () =>
      parseLogoutPayload({
        sessionId: "session-1",
        idleMinutes: -1,
        endedReason: "student_logout",
      }),
    /Invalid idleMinutes/,
  );
});
