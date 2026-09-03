// ========================================
// Auth Service & Session Management
// ========================================

const TOKEN_KEY = 'smartlib_token';
const USER_KEY = 'smartlib_user';

// Clean up any legacy items
localStorage.removeItem('biblioteca_user');

/**
 * Safely decodes base64 JWT payload handling UTF-8 characters.
 */
export function parseJwt(token) {
  try {
    const base64Url = token.split('.')[1];
    if (!base64Url) return null;
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
}

/**
 * Checks if a JWT token has expired.
 */
export function isTokenExpired(token) {
  if (!token) return true;
  const payload = parseJwt(token);
  if (!payload || !payload.exp) return false;
  // Exp is in seconds, convert to ms
  return payload.exp * 1000 <= Date.now();
}

/**
 * Returns the stored JWT token if valid and not expired.
 */
export function getToken() {
  const token = localStorage.getItem(TOKEN_KEY);
  if (!token) return null;

  if (isTokenExpired(token)) {
    logout();
    return null;
  }
  return token;
}

/**
 * Returns current authenticated user profile object.
 */
export function getUser() {
  const userJson = localStorage.getItem(USER_KEY);
  if (!userJson) return null;
  try {
    return JSON.parse(userJson);
  } catch {
    return null;
  }
}

/**
 * Checks if the user is currently authenticated with a valid token.
 */
export function isAuthenticated() {
  const token = localStorage.getItem(TOKEN_KEY);
  return !!token && !isTokenExpired(token);
}

/**
 * Checks if the authenticated user has a specific role (e.g. 'ADMIN', 'BIBLIOTECARIO', 'ALUNO').
 */
export function hasRole(role) {
  const user = getUser();
  if (!user || !user.perfil) return false;
  return user.perfil.toUpperCase() === role.toUpperCase();
}

/**
 * Checks if the user has any of the specified roles.
 */
export function hasAnyRole(roles = []) {
  if (!roles || roles.length === 0) return true;
  const user = getUser();
  if (!user || !user.perfil) return false;
  const userRole = user.perfil.toUpperCase();
  return roles.some(r => r.toUpperCase() === userRole);
}

export function isAdmin() {
  return hasRole('ADMIN');
}

export function isBibliotecario() {
  return hasRole('BIBLIOTECARIO');
}

export function isAluno() {
  return hasRole('ALUNO');
}

export function canManageAcervo() {
  return isAdmin() || isBibliotecario();
}

export function canManageEmprestimos() {
  return isAdmin() || isBibliotecario();
}

/**
 * Returns default landing route based on user profile.
 */
export function getDefaultRoute() {
  if (isAluno()) {
    return '/livros';
  }
  return '/dashboard';
}

/**
 * Stores token and user session into local storage.
 */
export function setSession(token, user) {
  localStorage.setItem(TOKEN_KEY, token);
  localStorage.setItem(USER_KEY, JSON.stringify(user));
}

/**
 * Clears authentication session and redirects to login page.
 */
export function logout() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
  localStorage.removeItem('biblioteca_user');

  if (window.location.hash !== '#/login') {
    window.location.hash = '/login';
    window.location.reload();
  }
}

/**
 * Real login request via POST /api/auth/login.
 */
export async function login(email, senha) {
  const response = await fetch('/api/auth/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ email: email.trim(), senha }),
  });

  if (!response.ok) {
    let message = 'Credenciais inválidas: e-mail ou senha incorretos.';
    try {
      const errorData = await response.json();
      message = errorData.detail || errorData.message || errorData.title || message;
    } catch {
      // fallback message
    }
    throw new Error(message);
  }

  const data = await response.json();
  if (!data.token) {
    throw new Error('Servidor não retornou um token de acesso válido.');
  }

  setSession(data.token, data.usuario);
  return data;
}

/**
 * Verifies current session token validity against /api/auth/me.
 */
export async function checkSession() {
  const token = getToken();
  if (!token) return null;

  try {
    const response = await fetch('/api/auth/me', {
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });

    if (response.status === 401) {
      logout();
      return null;
    }

    if (response.ok) {
      const user = await response.json();
      localStorage.setItem(USER_KEY, JSON.stringify(user));
      return user;
    }
  } catch {
    // Offline or server temporarily down, keep current storage
  }

  return getUser();
}
