// ========================================
// SPA Router - Hash-based routing with RBAC Guards
// ========================================

import { isAuthenticated, hasAnyRole, getDefaultRoute } from './auth.js';
import { showToast } from './components/toast.js';

const routes = {};
let currentCleanup = null;

export function registerRoute(path, handler, meta = { roles: [], requiresAuth: true }) {
  routes[path] = { handler, meta };
}

export function navigateTo(path) {
  window.location.hash = path;
}

export function getCurrentRoute() {
  return window.location.hash.slice(1) || getDefaultRoute();
}

export async function handleRoute() {
  const path = getCurrentRoute();
  const route = routes[path];

  // 1. Guard: Check Authentication
  if (route && route.meta?.requiresAuth && !isAuthenticated()) {
    if (window.location.hash !== '#/login') {
      window.location.hash = '/login';
    }
    return;
  }

  // 2. Guard: If authenticated and on login page, redirect to home
  if (path === '/login' && isAuthenticated()) {
    navigateTo(getDefaultRoute());
    return;
  }

  // 3. Guard: Check Role-Based Access Control (RBAC)
  if (route && route.meta?.roles && route.meta.roles.length > 0) {
    if (!hasAnyRole(route.meta.roles)) {
      showToast('Acesso negado: seu perfil não tem permissão para acessar esta página.', 'error');
      navigateTo(getDefaultRoute());
      return;
    }
  }

  // Cleanup previous page
  if (currentCleanup && typeof currentCleanup === 'function') {
    try {
      currentCleanup();
    } catch (err) {
      console.error('Error during route cleanup:', err);
    }
    currentCleanup = null;
  }

  if (route && route.handler) {
    const cleanup = await route.handler();
    if (typeof cleanup === 'function') {
      currentCleanup = cleanup;
    }
  } else {
    // Default to user's authorized home route
    navigateTo(getDefaultRoute());
  }
}

export function initRouter() {
  window.addEventListener('hashchange', handleRoute);

  // Set initial route
  if (!window.location.hash) {
    window.location.hash = isAuthenticated() ? getDefaultRoute() : '/login';
  } else {
    handleRoute();
  }
}

