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
  console.log("=== apartments controller tests ===");

  // setup
  await pool.query(`DELETE FROM "Users" WHERE "Email" IN ($1, $2)`, [MANAGER_CREDS.email, RESIDENT_CREDS.email]);
  const managerToken = await getToken(MANAGER_CREDS);
  const residentToken = await getToken(RESIDENT_CREDS);

  // create a house to attach apartments to
  await pool.query(`DELETE FROM "Houses" WHERE "Street" = 'apt test street'`);
  const houseRes = await fetch(`${API}/houses`, {
    method: "POST",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${managerToken}` },
    body: JSON.stringify({ number: "1", street: "apt test street", city: "riga", country: "latvia", postalCode: "LV-1000" }),
  });
  const houseBody = await houseRes.json();
  const houseId = houseBody.id;

  let createdApartmentId;

  // ── GET /apartments (authenticated) ─────────────────────────────────────────
  section("GET /apartments (manager)");

  const dbBefore = await pool.query(`SELECT COUNT(*)::int AS count FROM "Apartments"`);
  console.log(`  db before: apartment count = ${dbBefore.rows[0].count}`);

  const getAllRes = await fetch(`${API}/apartments`, {
    headers: { Authorization: `Bearer ${managerToken}` },
  });
  const getAllBody = await getAllRes.json();

  if (getAllRes.status === 200 && Array.isArray(getAllBody)) pass("get all returns 200 with array");
  else fail("get all failed", getAllBody);

  if (getAllBody.length === dbBefore.rows[0].count) pass("api count matches db count");
  else fail("count mismatch", { api: getAllBody.length, db: dbBefore.rows[0].count });

  // ── GET /apartments (unauthenticated) ────────────────────────────────────────
  section("GET /apartments (unauthenticated)");

  const noAuthRes = await fetch(`${API}/apartments`);

  if (noAuthRes.status === 401) pass("unauthenticated returns 401");
  else fail("expected 401");

  // ── POST /apartments (manager) ───────────────────────────────────────────────
  section("POST /apartments (manager creates)");

  const aptPayload = {
    number: "101",
    floor: 1,
    roomCount: 3,
    residentCount: 2,
    totalArea: 75.5,
    livingArea: 60.0,
    houseId: houseId,
  };

  const dbBeforeCreate = await pool.query(`SELECT COUNT(*)::int AS count FROM "Apartments" WHERE "HouseId" = $1`, [houseId]);
  console.log(`  db before create: apt count for house = ${dbBeforeCreate.rows[0].count}`);

  const createRes = await fetch(`${API}/apartments`, {
    method: "POST",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${managerToken}` },
    body: JSON.stringify(aptPayload),
  });
  const createBody = await createRes.json();

  const dbAfterCreate = await pool.query(`SELECT "Id", "Number", "Floor", "RoomCount", "TotalArea", "LivingArea", "HouseId" FROM "Apartments" WHERE "HouseId" = $1`, [houseId]);
  console.log(`  db after create: apt count = ${dbAfterCreate.rows.length}`);

  if (createRes.status === 201 && createBody.id) pass("create returns 201 with id");
  else fail("create failed", createBody);

  if (dbAfterCreate.rows.length === 1) pass("apartment row in db");
  else fail("apartment row missing");

  if (dbAfterCreate.rows[0].Number === "101") pass("number stored correctly");
  else fail("number mismatch");

  if (parseFloat(dbAfterCreate.rows[0].TotalArea) === 75.5) pass("totalarea stored correctly");
  else fail("totalarea mismatch", dbAfterCreate.rows[0].TotalArea);

  if (dbAfterCreate.rows[0].HouseId === houseId) pass("houseId foreign key correct");
  else fail("houseId mismatch");

  createdApartmentId = createBody.id;

  // ── POST /apartments (resident - forbidden) ──────────────────────────────────
  section("POST /apartments (resident - forbidden)");

  const dbBeforeForbid = await pool.query(`SELECT COUNT(*)::int AS count FROM "Apartments" WHERE "HouseId" = $1`, [houseId]);

  const forbidRes = await fetch(`${API}/apartments`, {
    method: "POST",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${residentToken}` },
    body: JSON.stringify(aptPayload),
  });

  const dbAfterForbid = await pool.query(`SELECT COUNT(*)::int AS count FROM "Apartments" WHERE "HouseId" = $1`, [houseId]);

  if (forbidRes.status === 403) pass("resident create returns 403");
  else fail("expected 403", { status: forbidRes.status });

  if (dbAfterForbid.rows[0].count === dbBeforeForbid.rows[0].count) pass("no extra row inserted");
  else fail("row was inserted despite 403");

  // ── GET /apartments/:id ──────────────────────────────────────────────────────
  section(`GET /apartments/${createdApartmentId}`);

  const dbById = await pool.query(`SELECT "Id", "Number" FROM "Apartments" WHERE "Id" = $1`, [createdApartmentId]);
  console.log(`  db check: apartment found = ${dbById.rows.length === 1}`);

  const getOneRes = await fetch(`${API}/apartments/${createdApartmentId}`, {
    headers: { Authorization: `Bearer ${managerToken}` },
  });
  const getOneBody = await getOneRes.json();

  if (getOneRes.status === 200) pass("get by id returns 200");
  else fail("get by id failed", getOneBody);

  if (getOneBody.id === createdApartmentId) pass("correct apartment returned");
  else fail("id mismatch");

  // ── GET /apartments/99999 (not found) ────────────────────────────────────────
  section("GET /apartments/99999 (not found)");

  const dbNone = await pool.query(`SELECT COUNT(*)::int AS count FROM "Apartments" WHERE "Id" = 99999`);
  console.log(`  db confirm: id 99999 exists = ${dbNone.rows[0].count > 0}`);

  const notFoundRes = await fetch(`${API}/apartments/99999`, {
    headers: { Authorization: `Bearer ${managerToken}` },
  });

  if (notFoundRes.status === 404) pass("nonexistent apartment returns 404");
  else fail("expected 404");

  // ── PUT /apartments/:id (manager) ────────────────────────────────────────────
  section(`PUT /apartments/${createdApartmentId} (manager updates)`);

  const dbBeforeUpdate = await pool.query(`SELECT "RoomCount" FROM "Apartments" WHERE "Id" = $1`, [createdApartmentId]);
  console.log(`  db before update: roomcount = ${dbBeforeUpdate.rows[0].RoomCount}`);

  const updateRes = await fetch(`${API}/apartments/${createdApartmentId}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${managerToken}` },
    body: JSON.stringify({ ...aptPayload, roomCount: 5 }),
  });

  const dbAfterUpdate = await pool.query(`SELECT "RoomCount" FROM "Apartments" WHERE "Id" = $1`, [createdApartmentId]);
  console.log(`  db after update: roomcount = ${dbAfterUpdate.rows[0].RoomCount}`);

  if (updateRes.status === 204) pass("update returns 204");
  else fail("update failed", { status: updateRes.status });

  if (dbAfterUpdate.rows[0].RoomCount === 5) pass("roomcount updated in db");
  else fail("roomcount not updated", dbAfterUpdate.rows[0]);

  // ── PUT /apartments/:id (resident - forbidden) ───────────────────────────────
  section(`PUT /apartments/${createdApartmentId} (resident - forbidden)`);

  const dbBeforeResPut = await pool.query(`SELECT "RoomCount" FROM "Apartments" WHERE "Id" = $1`, [createdApartmentId]);

  const resPutRes = await fetch(`${API}/apartments/${createdApartmentId}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${residentToken}` },
    body: JSON.stringify({ ...aptPayload, roomCount: 99 }),
  });

  const dbAfterResPut = await pool.query(`SELECT "RoomCount" FROM "Apartments" WHERE "Id" = $1`, [createdApartmentId]);

  if (resPutRes.status === 403) pass("resident update returns 403");
  else fail("expected 403 for resident put");

  if (dbAfterResPut.rows[0].RoomCount === dbBeforeResPut.rows[0].RoomCount) pass("roomcount unchanged after forbidden put");
  else fail("roomcount changed despite 403");

  // ── DELETE /apartments/:id (resident - forbidden) ────────────────────────────
  section(`DELETE /apartments/${createdApartmentId} (resident - forbidden)`);

  const dbBeforeResDel = await pool.query(`SELECT COUNT(*)::int AS count FROM "Apartments" WHERE "Id" = $1`, [createdApartmentId]);

  const resDelRes = await fetch(`${API}/apartments/${createdApartmentId}`, {
    method: "DELETE",
    headers: { Authorization: `Bearer ${residentToken}` },
  });

  const dbAfterResDel = await pool.query(`SELECT COUNT(*)::int AS count FROM "Apartments" WHERE "Id" = $1`, [createdApartmentId]);

  if (resDelRes.status === 403) pass("resident delete returns 403");
  else fail("expected 403 for resident delete");

  if (dbAfterResDel.rows[0].count === dbBeforeResDel.rows[0].count) pass("apartment still in db after forbidden delete");
  else fail("apartment deleted despite 403");

  // ── DELETE /apartments/:id (manager) ─────────────────────────────────────────
  section(`DELETE /apartments/${createdApartmentId} (manager deletes)`);

  const dbBeforeDelete = await pool.query(`SELECT COUNT(*)::int AS count FROM "Apartments" WHERE "Id" = $1`, [createdApartmentId]);
  console.log(`  db before delete: count = ${dbBeforeDelete.rows[0].count}`);

  const deleteRes = await fetch(`${API}/apartments/${createdApartmentId}`, {
    method: "DELETE",
    headers: { Authorization: `Bearer ${managerToken}` },
  });

  const dbAfterDelete = await pool.query(`SELECT COUNT(*)::int AS count FROM "Apartments" WHERE "Id" = $1`, [createdApartmentId]);
  console.log(`  db after delete: count = ${dbAfterDelete.rows[0].count}`);

  if (deleteRes.status === 204) pass("delete returns 204");
  else fail("delete failed", { status: deleteRes.status });

  if (dbAfterDelete.rows[0].count === 0) pass("apartment removed from db");
  else fail("apartment still in db after delete");

  // cleanup
  await pool.query(`DELETE FROM "Houses" WHERE "Id" = $1`, [houseId]);
  await pool.query(`DELETE FROM "Users" WHERE "Email" IN ($1, $2)`, [MANAGER_CREDS.email, RESIDENT_CREDS.email]);
  await pool.end();
  console.log("\n=== apartments tests done ===\n");
}

run().catch((e) => { console.error("unhandled error", e); process.exit(1); });
