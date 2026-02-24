import pg from "pg";
import fetch from "node-fetch";
import { API, DB, MANAGER_CREDS, RESIDENT_CREDS } from "./config.js";

const { Pool } = pg;
const pool = new Pool(DB);

const GOOGLE_TEST_EMAIL = "google.testuser@gmail.com";

function pass(msg) { console.log(`  [pass] ${msg}`); }
function fail(msg, detail) { console.error(`  [fail] ${msg}`, detail ?? ""); }
function section(msg) { console.log(`\n--- ${msg} ---`); }

async function run() {
  console.log("=== auth controller tests ===");

  // cleanup before suite
  await pool.query(
    `DELETE FROM "Users" WHERE "Email" IN ($1, $2, $3)`,
    [MANAGER_CREDS.email, RESIDENT_CREDS.email, GOOGLE_TEST_EMAIL]
  );

  // ── register manager ────────────────────────────────────────────────────────
  section("POST /auth/register (manager)");

  const dbBefore = await pool.query(
    `SELECT COUNT(*)::int AS count FROM "Users" WHERE "Email" = $1`,
    [MANAGER_CREDS.email]
  );
  console.log(`  db before: user count = ${dbBefore.rows[0].count}`);

  const regRes = await fetch(`${API}/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(MANAGER_CREDS),
  });
  const regBody = await regRes.json();

  const dbAfter = await pool.query(
    `SELECT "Id", "Email", "Role", "AuthProvider" FROM "Users" WHERE "Email" = $1`,
    [MANAGER_CREDS.email]
  );
  console.log(`  db after: user count = ${dbAfter.rows.length}`);

  if (regRes.status === 200 && regBody.token) pass("register returned 200 with token");
  else fail("register did not return token", regBody);

  if (dbAfter.rows.length === 1) pass("user row created in db");
  else fail("user row missing in db");

  if (dbAfter.rows[0]?.Role === "Manager") pass("role stored correctly");
  else fail("role mismatch", dbAfter.rows[0]);

  if (dbAfter.rows[0]?.AuthProvider === "local") pass("authprovider = local");
  else fail("authprovider wrong", dbAfter.rows[0]);

  // ── register duplicate ──────────────────────────────────────────────────────
  section("POST /auth/register (duplicate email)");

  const dupRes = await fetch(`${API}/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(MANAGER_CREDS),
  });
  const dupBody = await dupRes.json();

  const dbDup = await pool.query(
    `SELECT COUNT(*)::int AS count FROM "Users" WHERE "Email" = $1`,
    [MANAGER_CREDS.email]
  );
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

  const dbResResident = await pool.query(
    `SELECT "Role" FROM "Users" WHERE "Email" = $1`,
    [RESIDENT_CREDS.email]
  );

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

  const dbBadRole = await pool.query(
    `SELECT COUNT(*)::int AS count FROM "Users" WHERE "Email" = $1`,
    ["badrole@test.com"]
  );

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

  const dbShort = await pool.query(
    `SELECT COUNT(*)::int AS count FROM "Users" WHERE "Email" = $1`,
    ["shortpw@test.com"]
  );

  if (shortPwRes.status === 400) pass("short password returns 400");
  else fail("expected 400 for short password");

  if (dbShort.rows[0].count === 0) pass("no row inserted for short password");
  else fail("row inserted despite short password");

  // ── login valid ─────────────────────────────────────────────────────────────
  section("POST /auth/login (valid credentials)");

  const dbBeforeLogin = await pool.query(
    `SELECT "Id", "Email" FROM "Users" WHERE "Email" = $1`,
    [MANAGER_CREDS.email]
  );
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

  const dbAfterBadPw = await pool.query(
    `SELECT COUNT(*)::int AS count FROM "Users" WHERE "Email" = $1`,
    [MANAGER_CREDS.email]
  );
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

  // ── google oauth2 — new user (simulates angular client flow) ────────────────
  //
  // in production angular does this:
  //   1. user clicks "sign in with google"
  //   2. google sdk returns an id token (a signed jwt string)
  //   3. angular posts { "idToken": "<google jwt>" } to POST /api/auth/google
  //   4. backend calls GoogleJsonWebSignature.ValidateAsync(idToken) to verify
  //   5. backend extracts email from payload, does find-or-create, returns our jwt
  //
  // in tests we cannot get a real signed google id token without a browser,
  // so we use the development-only /api/auth/google/test endpoint which
  // accepts { "email": "..." } directly and skips the google signature check.
  // the find-or-create logic, db writes, and jwt issuance are identical.
  //
  section("POST /api/auth/google/test (new google user — first login creates account)");

  const dbBeforeGoogle = await pool.query(
    `SELECT COUNT(*)::int AS count FROM "Users" WHERE "Email" = $1`,
    [GOOGLE_TEST_EMAIL]
  );
  console.log(`  db before: google user exists = ${dbBeforeGoogle.rows[0].count > 0}`);

  // this is exactly the shape angular sends to /api/auth/google
  // except here we hit the test endpoint and pass email directly
  // instead of a real google id token
  const googleRes = await fetch(`${API}/auth/google/test`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email: GOOGLE_TEST_EMAIL }),
  });
  const googleBody = await googleRes.json();

  const dbAfterGoogle = await pool.query(
    `SELECT "Id", "Email", "Role", "AuthProvider", "PasswordHash" FROM "Users" WHERE "Email" = $1`,
    [GOOGLE_TEST_EMAIL]
  );
  console.log(`  db after: google user row count = ${dbAfterGoogle.rows.length}`);

  if (googleRes.status === 200 && googleBody.token) pass("google login returns 200 with jwt");
  else fail("google login failed", googleBody);

  if (googleBody.email === GOOGLE_TEST_EMAIL) pass("response email matches google email");
  else fail("email mismatch in response", googleBody);

  if (dbAfterGoogle.rows.length === 1) pass("user row created in db for google user");
  else fail("user row missing after google login");

  if (dbAfterGoogle.rows[0]?.AuthProvider === "google") pass("authprovider stored as google");
  else fail("authprovider not google", dbAfterGoogle.rows[0]);

  if (dbAfterGoogle.rows[0]?.PasswordHash === null) pass("passwordhash is null for google user");
  else fail("passwordhash should be null for google user");

  if (dbAfterGoogle.rows[0]?.Role === "Resident") pass("default role Resident assigned to new google user");
  else fail("default role wrong", dbAfterGoogle.rows[0]);

  // ── google oauth2 — second login (existing user, role preserved) ─────────────
  section("POST /api/auth/google/test (existing google user — second login preserves role)");

  const dbCountBefore = await pool.query(
    `SELECT COUNT(*)::int AS count FROM "Users" WHERE "Email" = $1`,
    [GOOGLE_TEST_EMAIL]
  );
  console.log(`  db before second login: row count = ${dbCountBefore.rows[0].count}`);

  const googleRes2 = await fetch(`${API}/auth/google/test`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email: GOOGLE_TEST_EMAIL }),
  });
  const googleBody2 = await googleRes2.json();

  const dbCountAfter = await pool.query(
    `SELECT COUNT(*)::int AS count FROM "Users" WHERE "Email" = $1`,
    [GOOGLE_TEST_EMAIL]
  );
  console.log(`  db after second login: row count = ${dbCountAfter.rows[0].count}`);

  if (googleRes2.status === 200 && googleBody2.token) pass("second google login returns 200 with jwt");
  else fail("second google login failed", googleBody2);

  if (dbCountAfter.rows[0].count === 1) pass("no duplicate user row created on second google login");
  else fail("duplicate row created", dbCountAfter.rows[0]);

  if (googleBody2.role === "Resident") pass("role preserved across google logins");
  else fail("role changed on second login", googleBody2);

  // ── google login endpoint rejects missing idToken ───────────────────────────
  section("POST /api/auth/google (missing idToken)");

  // this tests the real /api/auth/google endpoint (not the test shim)
  // angular would send { "idToken": "<token>" } — here we send empty
  const missingTokenRes = await fetch(`${API}/auth/google`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ idToken: "" }),
  });

  if (missingTokenRes.status === 400) pass("empty idToken returns 400");
  else fail("expected 400 for empty idToken", { status: missingTokenRes.status });

  // ── google login endpoint rejects invalid idToken ────────────────────────────
  section("POST /api/auth/google (invalid/fake idToken — not signed by google)");

  // simulate what happens if a client sends a garbage token
  // backend calls GoogleJsonWebSignature.ValidateAsync which throws,
  // validator returns null, service throws UnauthorizedAccessException, returns 401
  const fakeTokenRes = await fetch(`${API}/auth/google`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ idToken: "fake.token.notfromgoogle" }),
  });

  if (fakeTokenRes.status === 401) pass("fake idToken returns 401");
  else fail("expected 401 for fake idToken", { status: fakeTokenRes.status });

  // ── google test endpoint is blocked in non-development ──────────────────────
  // (we can only verify it works in dev — we cannot easily test that
  //  it returns 404 in production from here, but it is documented above)

  // cleanup
  await pool.query(
    `DELETE FROM "Users" WHERE "Email" IN ($1, $2, $3)`,
    [MANAGER_CREDS.email, RESIDENT_CREDS.email, GOOGLE_TEST_EMAIL]
  );
  await pool.end();
  console.log("\n=== auth tests done ===\n");
}

run().catch((e) => { console.error("unhandled error", e); process.exit(1); });
