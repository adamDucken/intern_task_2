import pg from "pg";
import fetch from "node-fetch";
import { API, DB, MANAGER_CREDS, RESIDENT_CREDS } from "./config.js";

const { Pool } = pg;
const pool = new Pool(DB);

function pass(msg) { console.log(`  [pass] ${msg}`); }
function fail(msg, detail) { console.error(`  [fail] ${msg}`, detail ?? ""); }
function section(msg) { console.log(`\n--- ${msg} ---`); }

async function getToken(creds) {
  await fetch(`${API}/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(creds),
  });
  const res = await fetch(`${API}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email: creds.email, password: creds.password }),
  });
  const body = await res.json();
  return body.token;
}

async function run() {
  console.log("=== residents controller tests ===");

  // setup
  await pool.query(`DELETE FROM "Users" WHERE "Email" IN ($1, $2)`, [MANAGER_CREDS.email, RESIDENT_CREDS.email]);
  const managerToken = await getToken(MANAGER_CREDS);
  const residentToken = await getToken(RESIDENT_CREDS);

  // cleanup test residents
  await pool.query(`DELETE FROM "Residents" WHERE "Email" = 'testperson@test.com'`);

  let createdResidentId;

  const residentPayload = {
    firstName: "test",
    lastName: "person",
    personalCode: "123456-12345",
    dateOfBirth: "1990-05-15T00:00:00Z",
    phone: "+37120000000",
    email: "testperson@test.com",
  };

  // ── GET /residents (manager) ─────────────────────────────────────────────────
  section("GET /residents (manager)");

  const dbBefore = await pool.query(`SELECT COUNT(*)::int AS count FROM "Residents"`);
  console.log(`  db before: resident count = ${dbBefore.rows[0].count}`);

  const getAllRes = await fetch(`${API}/residents`, {
    headers: { Authorization: `Bearer ${managerToken}` },
  });
  const getAllBody = await getAllRes.json();

  if (getAllRes.status === 200 && Array.isArray(getAllBody)) pass("get all returns 200 with array");
  else fail("get all failed", getAllBody);

  if (getAllBody.length === dbBefore.rows[0].count) pass("api count matches db count");
  else fail("count mismatch", { api: getAllBody.length, db: dbBefore.rows[0].count });

  // ── GET /residents (resident role can read) ──────────────────────────────────
  section("GET /residents (resident role)");

  const resGetAll = await fetch(`${API}/residents`, {
    headers: { Authorization: `Bearer ${residentToken}` },
  });

  if (resGetAll.status === 200) pass("resident role can read residents");
  else fail("resident role get all failed", { status: resGetAll.status });

  // ── GET /residents (unauthenticated) ─────────────────────────────────────────
  section("GET /residents (unauthenticated)");

  const noAuthRes = await fetch(`${API}/residents`);

  if (noAuthRes.status === 401) pass("unauthenticated returns 401");
  else fail("expected 401");

  // ── POST /residents (manager) ────────────────────────────────────────────────
  section("POST /residents (manager creates)");

  const dbBeforeCreate = await pool.query(`SELECT COUNT(*)::int AS count FROM "Residents" WHERE "Email" = 'testperson@test.com'`);
  console.log(`  db before create: count = ${dbBeforeCreate.rows[0].count}`);

  const createRes = await fetch(`${API}/residents`, {
    method: "POST",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${managerToken}` },
    body: JSON.stringify(residentPayload),
  });
  const createBody = await createRes.json();

  const dbAfterCreate = await pool.query(`SELECT "Id", "FirstName", "LastName", "PersonalCode", "Email", "Phone" FROM "Residents" WHERE "Email" = 'testperson@test.com'`);
  console.log(`  db after create: count = ${dbAfterCreate.rows.length}`);

  if (createRes.status === 201 && createBody.id) pass("create returns 201 with id");
  else fail("create failed", createBody);

  if (dbAfterCreate.rows.length === 1) pass("resident row in db");
  else fail("resident row missing");

  if (dbAfterCreate.rows[0].FirstName === "test") pass("firstname stored correctly");
  else fail("firstname mismatch", dbAfterCreate.rows[0]);

  if (dbAfterCreate.rows[0].PersonalCode === "123456-12345") pass("personalcode stored correctly");
  else fail("personalcode mismatch");

  createdResidentId = createBody.id;

  // ── POST /residents (resident - forbidden) ───────────────────────────────────
  section("POST /residents (resident role - forbidden)");

  const dbBeforeForbid = await pool.query(`SELECT COUNT(*)::int AS count FROM "Residents" WHERE "Email" = 'testperson@test.com'`);

  const forbidRes = await fetch(`${API}/residents`, {
    method: "POST",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${residentToken}` },
    body: JSON.stringify({ ...residentPayload, email: "another@test.com" }),
  });

  const dbAfterForbid = await pool.query(`SELECT COUNT(*)::int AS count FROM "Residents" WHERE "Email" IN ('testperson@test.com', 'another@test.com')`);

  if (forbidRes.status === 403) pass("resident role create returns 403");
  else fail("expected 403", { status: forbidRes.status });

  if (dbAfterForbid.rows[0].count === dbBeforeForbid.rows[0].count) pass("no extra row inserted");
  else fail("extra row inserted despite 403");

  // ── GET /residents/:id ───────────────────────────────────────────────────────
  section(`GET /residents/${createdResidentId}`);

  const dbById = await pool.query(`SELECT "Id", "FirstName" FROM "Residents" WHERE "Id" = $1`, [createdResidentId]);
  console.log(`  db check: resident found = ${dbById.rows.length === 1}`);

  const getOneRes = await fetch(`${API}/residents/${createdResidentId}`, {
    headers: { Authorization: `Bearer ${managerToken}` },
  });
  const getOneBody = await getOneRes.json();

  if (getOneRes.status === 200) pass("get by id returns 200");
  else fail("get by id failed", getOneBody);

  if (getOneBody.id === createdResidentId) pass("correct resident returned");
  else fail("id mismatch");

  // ── GET /residents/99999 (not found) ─────────────────────────────────────────
  section("GET /residents/99999 (not found)");

  const dbNone = await pool.query(`SELECT COUNT(*)::int AS count FROM "Residents" WHERE "Id" = 99999`);
  console.log(`  db confirm: id 99999 exists = ${dbNone.rows[0].count > 0}`);

  const notFoundRes = await fetch(`${API}/residents/99999`, {
    headers: { Authorization: `Bearer ${managerToken}` },
  });

  if (notFoundRes.status === 404) pass("nonexistent resident returns 404");
  else fail("expected 404");

  // ── PUT /residents/:id (manager) ─────────────────────────────────────────────
  section(`PUT /residents/${createdResidentId} (manager updates)`);

  const dbBeforeUpdate = await pool.query(`SELECT "Phone" FROM "Residents" WHERE "Id" = $1`, [createdResidentId]);
  console.log(`  db before update: phone = ${dbBeforeUpdate.rows[0].Phone}`);

  const updateRes = await fetch(`${API}/residents/${createdResidentId}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${managerToken}` },
    body: JSON.stringify({ ...residentPayload, phone: "+37129999999" }),
  });

  const dbAfterUpdate = await pool.query(`SELECT "Phone" FROM "Residents" WHERE "Id" = $1`, [createdResidentId]);
  console.log(`  db after update: phone = ${dbAfterUpdate.rows[0].Phone}`);

  if (updateRes.status === 204) pass("update returns 204");
  else fail("update failed", { status: updateRes.status });

  if (dbAfterUpdate.rows[0].Phone === "+37129999999") pass("phone updated in db");
  else fail("phone not updated", dbAfterUpdate.rows[0]);

  // ── PUT /residents/:id (resident role - allowed per controller) ───────────────
  section(`PUT /residents/${createdResidentId} (resident role - allowed)`);

  const dbBeforeResPut = await pool.query(`SELECT "Phone" FROM "Residents" WHERE "Id" = $1`, [createdResidentId]);

  const resPutRes = await fetch(`${API}/residents/${createdResidentId}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${residentToken}` },
    body: JSON.stringify({ ...residentPayload, phone: "+37100000001" }),
  });

  const dbAfterResPut = await pool.query(`SELECT "Phone" FROM "Residents" WHERE "Id" = $1`, [createdResidentId]);

  if (resPutRes.status === 204) pass("resident role update returns 204 (allowed by role policy)");
  else fail("resident role update unexpected status", { status: resPutRes.status });

  if (dbAfterResPut.rows[0].Phone === "+37100000001") pass("phone updated by resident role");
  else fail("phone not updated by resident", dbAfterResPut.rows[0]);

  // ── DELETE /residents/:id (resident - forbidden) ─────────────────────────────
  section(`DELETE /residents/${createdResidentId} (resident - forbidden)`);

  const dbBeforeResDel = await pool.query(`SELECT COUNT(*)::int AS count FROM "Residents" WHERE "Id" = $1`, [createdResidentId]);

  const resDelRes = await fetch(`${API}/residents/${createdResidentId}`, {
    method: "DELETE",
    headers: { Authorization: `Bearer ${residentToken}` },
  });

  const dbAfterResDel = await pool.query(`SELECT COUNT(*)::int AS count FROM "Residents" WHERE "Id" = $1`, [createdResidentId]);

  if (resDelRes.status === 403) pass("resident delete returns 403");
  else fail("expected 403 for resident delete");

  if (dbAfterResDel.rows[0].count === dbBeforeResDel.rows[0].count) pass("resident still in db after forbidden delete");
  else fail("resident deleted despite 403");

  // ── DELETE /residents/:id (manager) ──────────────────────────────────────────
  section(`DELETE /residents/${createdResidentId} (manager deletes)`);

  const dbBeforeDelete = await pool.query(`SELECT COUNT(*)::int AS count FROM "Residents" WHERE "Id" = $1`, [createdResidentId]);
  console.log(`  db before delete: count = ${dbBeforeDelete.rows[0].count}`);

  const deleteRes = await fetch(`${API}/residents/${createdResidentId}`, {
    method: "DELETE",
    headers: { Authorization: `Bearer ${managerToken}` },
  });

  const dbAfterDelete = await pool.query(`SELECT COUNT(*)::int AS count FROM "Residents" WHERE "Id" = $1`, [createdResidentId]);
  console.log(`  db after delete: count = ${dbAfterDelete.rows[0].count}`);

  if (deleteRes.status === 204) pass("delete returns 204");
  else fail("delete failed", { status: deleteRes.status });

  if (dbAfterDelete.rows[0].count === 0) pass("resident removed from db");
  else fail("resident still in db after delete");

  // cleanup
  await pool.query(`DELETE FROM "Residents" WHERE "Email" = 'testperson@test.com'`);
  await pool.query(`DELETE FROM "Users" WHERE "Email" IN ($1, $2)`, [MANAGER_CREDS.email, RESIDENT_CREDS.email]);
  await pool.end();
  console.log("\n=== residents tests done ===\n");
}

run().catch((e) => { console.error("unhandled error", e); process.exit(1); });
