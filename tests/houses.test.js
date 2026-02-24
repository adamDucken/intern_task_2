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
  console.log("=== houses controller tests ===");

  // setup users
  await pool.query(`DELETE FROM "Users" WHERE "Email" IN ($1, $2)`, [MANAGER_CREDS.email, RESIDENT_CREDS.email]);
  const managerToken = await getToken(MANAGER_CREDS);
  const residentToken = await getToken(RESIDENT_CREDS);

  // cleanup test houses
  await pool.query(`DELETE FROM "Houses" WHERE "Street" = 'test street'`);

  let createdHouseId;

  // ── GET /houses (authenticated) ─────────────────────────────────────────────
  section("GET /houses (manager)");

  const dbBefore = await pool.query(`SELECT COUNT(*)::int AS count FROM "Houses"`);
  console.log(`  db before: house count = ${dbBefore.rows[0].count}`);

  const getAllRes = await fetch(`${API}/houses`, {
    headers: { Authorization: `Bearer ${managerToken}` },
  });
  const getAllBody = await getAllRes.json();

  if (getAllRes.status === 200 && Array.isArray(getAllBody)) pass("get all returns 200 with array");
  else fail("get all failed", getAllBody);

  if (getAllBody.length === dbBefore.rows[0].count) pass("api count matches db count");
  else fail("count mismatch", { api: getAllBody.length, db: dbBefore.rows[0].count });

  // ── GET /houses (no token) ───────────────────────────────────────────────────
  section("GET /houses (unauthenticated)");

  const noAuthRes = await fetch(`${API}/houses`);

  if (noAuthRes.status === 401) pass("unauthenticated returns 401");
  else fail("expected 401 without token");

  // ── POST /houses (manager) ───────────────────────────────────────────────────
  section("POST /houses (manager creates house)");

  const housePayload = {
    number: "42",
    street: "test street",
    city: "riga",
    country: "latvia",
    postalCode: "LV-1001",
  };

  const dbBeforeCreate = await pool.query(`SELECT COUNT(*)::int AS count FROM "Houses" WHERE "Street" = 'test street'`);
  console.log(`  db before create: count = ${dbBeforeCreate.rows[0].count}`);

  const createRes = await fetch(`${API}/houses`, {
    method: "POST",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${managerToken}` },
    body: JSON.stringify(housePayload),
  });
  const createBody = await createRes.json();

  const dbAfterCreate = await pool.query(`SELECT "Id", "Number", "Street", "City", "Country", "PostalCode" FROM "Houses" WHERE "Street" = 'test street'`);
  console.log(`  db after create: count = ${dbAfterCreate.rows.length}`);

  if (createRes.status === 201 && createBody.id) pass("create returns 201 with id");
  else fail("create failed", createBody);

  if (dbAfterCreate.rows.length === 1) pass("house row exists in db");
  else fail("house row missing in db");

  if (dbAfterCreate.rows[0].Street === "test street") pass("street stored correctly");
  else fail("street mismatch", dbAfterCreate.rows[0]);

  if (dbAfterCreate.rows[0].City === "riga") pass("city stored correctly");
  else fail("city mismatch");

  createdHouseId = createBody.id;

  // ── POST /houses (resident - forbidden) ─────────────────────────────────────
  section("POST /houses (resident - forbidden)");

  const dbBeforeForbidden = await pool.query(`SELECT COUNT(*)::int AS count FROM "Houses" WHERE "Street" = 'test street'`);

  const forbidRes = await fetch(`${API}/houses`, {
    method: "POST",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${residentToken}` },
    body: JSON.stringify(housePayload),
  });

  const dbAfterForbidden = await pool.query(`SELECT COUNT(*)::int AS count FROM "Houses" WHERE "Street" = 'test street'`);

  if (forbidRes.status === 403) pass("resident create returns 403");
  else fail("expected 403 for resident create", { status: forbidRes.status });

  if (dbAfterForbidden.rows[0].count === dbBeforeForbidden.rows[0].count) pass("no extra row inserted");
  else fail("extra row was inserted despite 403");

  // ── GET /houses/:id ──────────────────────────────────────────────────────────
  section(`GET /houses/${createdHouseId}`);

  const dbById = await pool.query(`SELECT "Id", "Number", "Street" FROM "Houses" WHERE "Id" = $1`, [createdHouseId]);
  console.log(`  db check: house found = ${dbById.rows.length === 1}`);

  const getOneRes = await fetch(`${API}/houses/${createdHouseId}`, {
    headers: { Authorization: `Bearer ${managerToken}` },
  });
  const getOneBody = await getOneRes.json();

  if (getOneRes.status === 200) pass("get by id returns 200");
  else fail("get by id failed", getOneBody);

  if (getOneBody.id === createdHouseId) pass("returned correct house id");
  else fail("id mismatch");

  // ── GET /houses/99999 (not found) ────────────────────────────────────────────
  section("GET /houses/99999 (not found)");

  const dbNonExist = await pool.query(`SELECT COUNT(*)::int AS count FROM "Houses" WHERE "Id" = 99999`);
  console.log(`  db confirm: id 99999 exists = ${dbNonExist.rows[0].count > 0}`);

  const notFoundRes = await fetch(`${API}/houses/99999`, {
    headers: { Authorization: `Bearer ${managerToken}` },
  });

  if (notFoundRes.status === 404) pass("nonexistent house returns 404");
  else fail("expected 404", { status: notFoundRes.status });

  // ── PUT /houses/:id (manager) ────────────────────────────────────────────────
  section(`PUT /houses/${createdHouseId} (manager updates)`);

  const dbBeforeUpdate = await pool.query(`SELECT "City" FROM "Houses" WHERE "Id" = $1`, [createdHouseId]);
  console.log(`  db before update: city = ${dbBeforeUpdate.rows[0].City}`);

  const updateRes = await fetch(`${API}/houses/${createdHouseId}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${managerToken}` },
    body: JSON.stringify({ ...housePayload, city: "jurmala" }),
  });

  const dbAfterUpdate = await pool.query(`SELECT "City" FROM "Houses" WHERE "Id" = $1`, [createdHouseId]);
  console.log(`  db after update: city = ${dbAfterUpdate.rows[0].City}`);

  if (updateRes.status === 204) pass("update returns 204");
  else fail("update failed", { status: updateRes.status });

  if (dbAfterUpdate.rows[0].City === "jurmala") pass("city updated in db");
  else fail("city not updated in db", dbAfterUpdate.rows[0]);

  // ── PUT /houses/:id (resident - forbidden) ───────────────────────────────────
  section(`PUT /houses/${createdHouseId} (resident - forbidden)`);

  const dbBeforeResPut = await pool.query(`SELECT "City" FROM "Houses" WHERE "Id" = $1`, [createdHouseId]);

  const resPutRes = await fetch(`${API}/houses/${createdHouseId}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${residentToken}` },
    body: JSON.stringify({ ...housePayload, city: "daugavpils" }),
  });

  const dbAfterResPut = await pool.query(`SELECT "City" FROM "Houses" WHERE "Id" = $1`, [createdHouseId]);

  if (resPutRes.status === 403) pass("resident update returns 403");
  else fail("expected 403 for resident put");

  if (dbAfterResPut.rows[0].City === dbBeforeResPut.rows[0].City) pass("city unchanged after forbidden put");
  else fail("city was changed despite 403");

  // ── DELETE /houses/:id (resident - forbidden) ────────────────────────────────
  section(`DELETE /houses/${createdHouseId} (resident - forbidden)`);

  const dbBeforeResDel = await pool.query(`SELECT COUNT(*)::int AS count FROM "Houses" WHERE "Id" = $1`, [createdHouseId]);

  const resDelRes = await fetch(`${API}/houses/${createdHouseId}`, {
    method: "DELETE",
    headers: { Authorization: `Bearer ${residentToken}` },
  });

  const dbAfterResDel = await pool.query(`SELECT COUNT(*)::int AS count FROM "Houses" WHERE "Id" = $1`, [createdHouseId]);

  if (resDelRes.status === 403) pass("resident delete returns 403");
  else fail("expected 403 for resident delete");

  if (dbAfterResDel.rows[0].count === dbBeforeResDel.rows[0].count) pass("house still exists after forbidden delete");
  else fail("house was deleted despite 403");

  // ── DELETE /houses/:id (manager) ─────────────────────────────────────────────
  section(`DELETE /houses/${createdHouseId} (manager deletes)`);

  const dbBeforeDelete = await pool.query(`SELECT COUNT(*)::int AS count FROM "Houses" WHERE "Id" = $1`, [createdHouseId]);
  console.log(`  db before delete: count = ${dbBeforeDelete.rows[0].count}`);

  const deleteRes = await fetch(`${API}/houses/${createdHouseId}`, {
    method: "DELETE",
    headers: { Authorization: `Bearer ${managerToken}` },
  });

  const dbAfterDelete = await pool.query(`SELECT COUNT(*)::int AS count FROM "Houses" WHERE "Id" = $1`, [createdHouseId]);
  console.log(`  db after delete: count = ${dbAfterDelete.rows[0].count}`);

  if (deleteRes.status === 204) pass("delete returns 204");
  else fail("delete failed", { status: deleteRes.status });

  if (dbAfterDelete.rows[0].count === 0) pass("house row removed from db");
  else fail("house row still in db after delete");

  // cleanup
  await pool.query(`DELETE FROM "Users" WHERE "Email" IN ($1, $2)`, [MANAGER_CREDS.email, RESIDENT_CREDS.email]);
  await pool.query(`DELETE FROM "Houses" WHERE "Street" = 'test street'`);
  await pool.end();
  console.log("\n=== houses tests done ===\n");
}

run().catch((e) => { console.error("unhandled error", e); process.exit(1); });
