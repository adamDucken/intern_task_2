#!/usr/bin/env sh
set -e

cd /tests
npm install

echo "waiting for backend to be ready..."
i=1
while [ $i -le 30 ]; do
  if wget -qO- http://backend:5291/swagger/index.html >/dev/null 2>&1; then
    echo "backend is ready"
    break
  fi
  echo "attempt $i/30 - backend not ready yet, retrying in 5s..."
  i=$((i + 1))
  sleep 5
done

echo "running tests..."
node /tests/auth.test.js
node /tests/houses.test.js
node /tests/apartments.test.js
node /tests/residents.test.js
echo "all tests passed"
