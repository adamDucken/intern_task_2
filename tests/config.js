const apiHost = process.env.API_HOST ?? "localhost";
const dbHost = process.env.DB_HOST ?? "localhost";

export const API = `http://${apiHost}:5291/api`;

export const DB = {
  host: dbHost,
  port: 5432,
  database: "intern_task_2_db",
  user: "admin",
  password: "admin123",
};

export const MANAGER_CREDS = {
  email: "testmanager@test.com",
  password: "manager123",
  role: "Manager",
};

export const RESIDENT_CREDS = {
  email: "testresident@test.com",
  password: "resident123",
  role: "Resident",
};
