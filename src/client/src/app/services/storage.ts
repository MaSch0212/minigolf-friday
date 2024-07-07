import { Logger } from './logger.service';
import { User } from '../models/parsed-models';

const TOKEN_KEY = 'access_token_info';
const LOGIN_TOKEN_KEY = 'login_token';
const HAS_CONFIGURED_PUSH = 'has_configured_push';

function getLocalStorage(key: string): string | null {
  return localStorage.getItem(key);
}
function setLocalStorage(key: string, value: string | null) {
  const oldValue = localStorage.getItem(key);
  if (oldValue === value) return;
  if (value) {
    Logger.logDebug('Storage', `Setting local storage key "${key}"`, {
      from: localStorage.getItem(key),
      to: value,
    });
    localStorage.setItem(key, value);
  } else {
    Logger.logDebug('Storage', `Removing local storage key "${key}"`, {
      from: localStorage.getItem(key),
    });
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

export function getHasConfiguredPush(): boolean {
  const hasRejected = getLocalStorage(HAS_CONFIGURED_PUSH);
  return hasRejected === 'true';
}
export function setHasConfiguredPush(hasConfigured: boolean) {
  setLocalStorage(HAS_CONFIGURED_PUSH, hasConfigured ? 'true' : 'false');
}

export type LogLevel = 'debug' | 'info' | 'warn' | 'error';
const logLevelDefaults: { [L in LogLevel]: 'true' | 'false' } = {
  debug: 'false',
  info: 'true',
  warn: 'true',
  error: 'true',
};
export function getLogLevelEnabled(level: LogLevel): boolean {
  return (getLocalStorage(`log_level_${level}`) ?? logLevelDefaults[level]) === 'true';
}
export function setLogLevelEnabled(level: LogLevel, enabled: boolean) {
  setLocalStorage(`log_level_${level}`, enabled ? 'true' : 'false');
}
