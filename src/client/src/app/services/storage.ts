import { User } from '../models/parsed-models';

const TOKEN_KEY = 'access_token_info';
const LOGIN_TOKEN_KEY = 'login_token';

function getLocalStorage(key: string): string | null {
  return localStorage.getItem(key);
}
function setLocalStorage(key: string, value: string | null) {
  if (value) {
    localStorage.setItem(key, value);
  } else {
    localStorage.removeItem(key);
  }
}

export type AuthTokenInfo = {
  token: string;
  expiresAt: Date;
  user: User;
};
export function getAuthTokenInfo(): AuthTokenInfo | null {
  const tokenInfo = getLocalStorage(TOKEN_KEY);
  if (!tokenInfo) return null;

  try {
    const info = JSON.parse(tokenInfo);
    info.expiresAt = new Date(info.expiresAt);
    return info;
  } catch {
    return null;
  }
}
export function setAuthTokenInfo(tokenInfo: AuthTokenInfo | null) {
  setLocalStorage(TOKEN_KEY, tokenInfo ? JSON.stringify(tokenInfo) : null);
}

export function getLoginToken(): string | null {
  return getLocalStorage(LOGIN_TOKEN_KEY);
}
export function setLoginToken(token: string | null) {
  setLocalStorage(LOGIN_TOKEN_KEY, token);
}

export function getHasRejectedPush(): boolean {
  const hasRejected = getLocalStorage('hasRejectedPush');
  return hasRejected === 'true';
}
export function setHasRejectedPush(hasRejected: boolean) {
  setLocalStorage('hasRejectedPush', hasRejected ? 'true' : 'false');
}
