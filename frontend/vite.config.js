import { defineConfig } from 'vite';

// Plugin to handle # character in file paths
function hashPathPlugin() {
  return {
    name: 'hash-path-fix',
    enforce: 'pre',
    configResolved(config) {
      // Override the root to use the junction path if available
    },
    resolveId(source, importer) {
      // Let Vite handle resolution normally
      return null;
    },
    load(id) {
      // If the id contains # and Vite can't load it, try without the URL fragment part
      if (id.includes('#') && !id.includes('node_modules')) {
        try {
          const fs = require('fs');
          if (fs.existsSync(id)) {
            return fs.readFileSync(id, 'utf-8');
          }
        } catch {
          // fallback
        }
      }
      return null;
    },
  };
}

export default defineConfig({
  plugins: [hashPathPlugin()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5084',
        changeOrigin: true,
      },
    },
  },
});
