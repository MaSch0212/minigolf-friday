const keyPrefix = 'minigolf-friday-manager-';

export function getLocalStorage(key: string) {
  return localStorage.getItem(keyPrefix + key);
}

export function setLocalStorage(key: string, value: string | null | undefined) {
  if (value === null || value === undefined) {
    localStorage.removeItem(keyPrefix + key);
  } else {
    localStorage.setItem(keyPrefix + key, value);
  }
}
