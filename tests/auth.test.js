import pg from "pg";
import fetch from "node-fetch";
import { API, DB, MANAGER_CREDS, RESIDENT_CREDS } from "./config.js";

const { Pool } = pg;
const pool = new Pool(DB);

function pass(msg) { console.log(`  [pass] ${msg}`); }
function fail(msg, detail) { console.error(`  [fail] ${msg}`, detail ?? ""); }
function section(msg) { console.log(`\n--- ${msg} ---`); }

async function run() {
  console.log("=== auth controller tests ===");

  // cleanup before suite
  await pool.query(`DELETE FROM "Users" WHERE "Email" IN ($1, $2)`, [
    MANAGER_CREDS.email,
    RESIDENT_CREDS.email,
  ]);

  // ── register manager ────────────────────────────────────────────────────────
  section("POST /auth/register (manager)");

  const dbBefore = await pool.query(`SELECT COUNT(*)::int AS count FROM "Users" WHERE "Email" = $1`, [MANAGER_CREDS.email]);
  console.log(`  db before: user count = ${dbBefore.rows[0].count}`);

  const regRes = await fetch(`${API}/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(MANAGER_CREDS),
  });
  const regBody = await regRes.json();

  const dbAfter = await pool.query(`SELECT "Id", "Email", "Role", "AuthProvider" FROM "Users" WHERE "Email" = $1`, [MANAGER_CREDS.email]);
  console.log(`  db after:  user count = ${dbAfter.rows.length}`);

  if (regRes.status === 200 && regBody.token) pass("register returned 200 with token");
  else fail("register did not return token", regBody);

  if (dbAfter.rows.length === 1) pass("user row created in db");
  else fail("user row missing in db");

  if (dbAfter.rows[0]?.Role === "Manager") pass("role stored correctly");
  else fail("role mismatch", dbAfter.rows[0]);

  if (dbAfter.rows[0]?.AuthProvider === "local") pass("authprovider = local");
  else fail("authprovider wrong", dbAfter.rows[0]);

  const managerToken = regBody.token;

  // ── register duplicate ──────────────────────────────────────────────────────
  section("POST /auth/register (duplicate email)");

  const dupRes = await fetch(`${API}/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(MANAGER_CREDS),
  });
  const dupBody = await dupRes.json();

  const dbDup = await pool.query(`SELECT COUNT(*)::int AS count FROM "Users" WHERE "Email" = $1`, [MANAGER_CREDS.email]);
  console.log(`  db after dup attempt: user count = ${dbDup.rows[0].count}`);

  if (dupRes.status === 400) pass("duplicate returns 400");
  else fail("expected 400 for duplicate", dupBody);

  if (dbDup.rows[0].count === 1) pass("no duplicate row inserted");
  else fail("duplicate row found in db");

  // ── register resident ───────────────────────────────────────────────────────
  section("POST /auth/register (resident)");

  const resRegRes = await fetch(`${API}/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(RESIDENT_CREDS),
  });
  const resRegBody = await resRegRes.json();

  const dbResResident = await pool.query(`SELECT "Role" FROM "Users" WHERE "Email" = $1`, [RESIDENT_CREDS.email]);

  if (resRegRes.status === 200 && resRegBody.token) pass("resident registered");
  else fail("resident register failed", resRegBody);

  if (dbResResident.rows[0]?.Role === "Resident") pass("resident role stored correctly");
  else fail("resident role mismatch");

  // ── register invalid role ───────────────────────────────────────────────────
  section("POST /auth/register (invalid role)");

  const badRoleRes = await fetch(`${API}/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email: "badrole@test.com", password: "pass123", role: "Admin" }),
  });

  const dbBadRole = await pool.query(`SELECT COUNT(*)::int AS count FROM "Users" WHERE "Email" = $1`, ["badrole@test.com"]);

  if (badRoleRes.status === 400) pass("invalid role returns 400");
  else fail("expected 400 for invalid role");

  if (dbBadRole.rows[0].count === 0) pass("no row inserted for invalid role");
  else fail("row was inserted with invalid role");

  // ── register short password ─────────────────────────────────────────────────
  section("POST /auth/register (short password)");

  const shortPwRes = await fetch(`${API}/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email: "shortpw@test.com", password: "abc", role: "Resident" }),
  });

  const dbShort = await pool.query(`SELECT COUNT(*)::int AS count FROM "Users" WHERE "Email" = $1`, ["shortpw@test.com"]);

  if (shortPwRes.status === 400) pass("short password returns 400");
  else fail("expected 400 for short password");

  if (dbShort.rows[0].count === 0) pass("no row inserted for short password");
  else fail("row inserted despite short password");

  // ── login valid ─────────────────────────────────────────────────────────────
  section("POST /auth/login (valid credentials)");

  const dbBeforeLogin = await pool.query(`SELECT "Id", "Email" FROM "Users" WHERE "Email" = $1`, [MANAGER_CREDS.email]);
  console.log(`  db check: manager exists = ${dbBeforeLogin.rows.length === 1}`);

  const loginRes = await fetch(`${API}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email: MANAGER_CREDS.email, password: MANAGER_CREDS.password }),
  });
  const loginBody = await loginRes.json();

  if (loginRes.status === 200 && loginBody.token) pass("login returns 200 with token");
  else fail("login failed", loginBody);

  if (loginBody.role === "Manager") pass("login response contains correct role");
  else fail("login role mismatch", loginBody);

  if (loginBody.email === MANAGER_CREDS.email) pass("login response contains correct email");
  else fail("login email mismatch", loginBody);

  // ── login wrong password ────────────────────────────────────────────────────
  section("POST /auth/login (wrong password)");

  const badPwRes = await fetch(`${API}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email: MANAGER_CREDS.email, password: "wrongpassword" }),
  });

  const dbAfterBadPw = await pool.query(`SELECT COUNT(*)::int AS count FROM "Users" WHERE "Email" = $1`, [MANAGER_CREDS.email]);
  console.log(`  db check: user still exists = ${dbAfterBadPw.rows[0].count === 1}`);

  if (badPwRes.status === 401) pass("wrong password returns 401");
  else fail("expected 401 for wrong password");

  // ── login nonexistent user ──────────────────────────────────────────────────
  section("POST /auth/login (nonexistent user)");

  const noUserRes = await fetch(`${API}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email: "nobody@nowhere.com", password: "pass123" }),
  });

  if (noUserRes.status === 401) pass("nonexistent user returns 401");
  else fail("expected 401 for nonexistent user");

  // cleanup
  await pool.query(`DELETE FROM "Users" WHERE "Email" IN ($1, $2)`, [
    MANAGER_CREDS.email,
    RESIDENT_CREDS.email,
  ]);
  await pool.end();
  console.log("\n=== auth tests done ===\n");
}

run().catch((e) => { console.error("unhandled error", e); process.exit(1); });
