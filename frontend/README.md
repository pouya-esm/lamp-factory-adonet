# Lamp Factory UI

React + Vite + Tailwind client for the Lamp Factory (SKD LED lamp) production
ERP. This file is a short overview; the detailed client documentation —
folder structure, authentication and token handling, API connection, page
routes, and commands — lives in [`../docs/frontend.md`](../docs/frontend.md).

## Setup

```bash
npm ci                    # install exactly per package-lock.json
cp .env.example .env      # PowerShell: Copy-Item .env.example .env — set VITE_API_BASE_URL
npm run dev               # dev server on http://localhost:5173
npm run build             # production build into dist/
```

- `VITE_API_BASE_URL` must point at the API root **including `/api`**
  (e.g. `https://localhost:7263/api`); without it the client falls back to
  that same value.
- Port `5173` is fixed in `vite.config.js` and matches the only origin the
  backend's CORS policy allows.

## What's here

- **`src/api/client.js`** — Axios instance that attaches the JWT to every
  request, unwraps the backend envelope (`isSuccess` / `result` /
  `errorMessage`, camelCase on the wire), performs a single-flight silent
  refresh on `401`, and normalizes error bodies into readable messages.
- **`src/api/tokenStorage.js`** — `localStorage` persistence for the access +
  refresh tokens (`lampFactory.token` / `lampFactory.refreshToken`), plus
  JWT-expiry helpers used for proactive renewal.
- **`src/context/AuthContext.jsx`** — session state. Decodes only the JWT's
  id and username claims (the token intentionally carries nothing else;
  permissions and roles stay server-side), subscribes to `tokenStorage` so
  silent renewals and forced logouts keep React in sync, and prefetches the
  user's accessible resources for navigation.
- **`src/components/Layout.jsx`, `Sidebar.jsx`, `Topbar.jsx`** — the app
  shell. The sidebar is generated from `src/components/navConfig.js`, grouped
  to mirror the SKD flow: Master Data → Planning & BOM → Production →
  Vouchers → Reports → Admin. `LineStatusStrip.jsx` shows a live status per
  production line, colored via `pages/production-lines/lineStatus.js`.
- **`src/pages/**` — real module screens (products, warehouses, operators,
  production lines, BOMs, production plans & documents, quality control, line
  stops, all four voucher types, users/roles/resources/actions/permissions,
  audit log, both report pages). `PlaceholderPage` remains only as the
  fallback for navigation entries whose screen has not been built yet.
