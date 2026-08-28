// ========================================
// SPA Router - Hash-based routing
// ========================================

const routes = {};
let currentCleanup = null;

export function registerRoute(path, handler) {
  routes[path] = handler;
}

export function navigateTo(path) {
  window.location.hash = path;
}

export function getCurrentRoute() {
  return window.location.hash.slice(1) || '/dashboard';
}

export async function handleRoute() {
  const path = getCurrentRoute();
  const handler = routes[path];

  // Cleanup previous page
  if (currentCleanup && typeof currentCleanup === 'function') {
    currentCleanup();
    currentCleanup = null;
  }

  if (handler) {
    const cleanup = await handler();
    if (typeof cleanup === 'function') {
      currentCleanup = cleanup;
    }
  } else {
    // Default to dashboard
    navigateTo('/dashboard');
  }
}

export function initRouter() {
  window.addEventListener('hashchange', handleRoute);

  // Set initial route
  if (!window.location.hash) {
    window.location.hash = '/dashboard';
  } else {
    handleRoute();
  }
}
