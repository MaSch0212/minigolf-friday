import { DestroyRef, Injectable, computed, effect, inject, signal } from '@angular/core';

import { getLocalStorage, setLocalStorage } from '../utils/local-storage.utils';

const localStorageKey = 'theme';

export type Theme = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly _linkElement = document.getElementById('app-theme') as HTMLLinkElement;
  private readonly _theme = signal<Theme | null>(this.getThemeFromLocalStorage());

  public readonly isDarkColorSchemePrefered = isDarkColorSchemePrefered();
  public readonly theme = computed(
    () => this._theme() ?? (this.isDarkColorSchemePrefered() ? 'dark' : 'light')
  );

  constructor() {
    effect(() => {
      this._linkElement.href = `${this.theme()}-theme.css`;
    });
  }

  public setTheme(theme: Theme | null) {
    setLocalStorage(localStorageKey, theme);
    this._theme.set(theme);
  }

  public isTheme(theme: Theme | null) {
    return this._theme() === theme;
  }

  private getThemeFromLocalStorage() {
    const theme = getLocalStorage(localStorageKey);
    return theme === 'light' || theme === 'dark' ? theme : null;
  }
}

function isDarkColorSchemePrefered() {
  const userMedia = window.matchMedia('(prefers-color-scheme: dark)');
  const result = signal(userMedia.matches);

  const listener = (e: MediaQueryListEvent) => {
    result.set(e.matches);
  };
  userMedia.addEventListener('change', listener);
  inject(DestroyRef).onDestroy(() => userMedia.removeEventListener('change', listener));

  return result;
}
