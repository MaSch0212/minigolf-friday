import { DestroyRef, Injectable, computed, effect, inject, signal } from '@angular/core';

import { TranslateService } from './translate.service';
import { getLocalStorage, setLocalStorage } from '../utils/local-storage.utils';

const localStorageKey = 'theme';

export type Theme = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly _linkElement = document.getElementById('app-theme') as HTMLLinkElement;
  private readonly _theme = signal<Theme | null>(this.getThemeFromLocalStorage());
  private readonly _translations = inject(TranslateService).translations;

  public readonly isDarkColorSchemePrefered = isDarkColorSchemePrefered();
  public readonly theme = computed(
    () => this._theme() ?? (this.isDarkColorSchemePrefered() ? 'dark' : 'light')
  );

  public readonly themeItems = computed(() => [
    {
      label: `${this._translations.settings_useSystemTheme()} (${this.getThemeDisplay(
        this.isDarkColorSchemePrefered() ? 'dark' : 'light'
      )})`,
      icon: this.isTheme(null) ? 'i-[mdi--check]' : 'i-[mdi--theme-light-dark]',
      command: () => this.setTheme(null),
    },
    {
      separator: true,
    },
    {
      label: this._translations.settings_darkTheme(),
      icon: this.isTheme('dark') ? 'i-[mdi--check]' : 'i-[mdi--weather-night]',
      command: () => this.setTheme('dark'),
    },
    {
      label: this._translations.settings_lightTheme(),
      icon: this.isTheme('light') ? 'i-[mdi--check]' : 'i-[mdi--weather-sunny]',
      command: () => this.setTheme('light'),
    },
  ]);

  constructor() {
    effect(() => {
      const theme = this.theme();
      const css = `${theme}-theme.css`;
      if (!this._linkElement.href.endsWith(css)) {
        this._linkElement.href = css;
      }
      document.body.classList.toggle('dark', theme === 'dark');
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

  private getThemeDisplay(theme: string) {
    switch (theme) {
      case 'dark':
        return this._translations.settings_darkTheme();
      case 'light':
        return this._translations.settings_lightTheme();
      default:
        throw new Error(`Unknown theme: ${theme}`);
    }
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
