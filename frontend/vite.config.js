import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Point VITE_API_BASE_URL at the Lamp_Factory ASP.NET Core API, e.g. https://localhost:7051/api
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173
  }
})
