import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,
    proxy: {
      // Dev'de /api istekleri backend'e yönlendirilir (CORS'suz, aynı origin)
      '/api': 'http://localhost:5072',
    },
  },
})
