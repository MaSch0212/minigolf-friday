import { Injectable, Signal, computed, effect, signal } from '@angular/core';

import { getLocalStorage, setLocalStorage } from '../utils/local-storage.utils';

import type en from '../i18n/en.json';

const langs: Record<string, () => Promise<typeof en>> = {
  en: () => import('../i18n/en.json').then(x => x.default),
  de: () => import('../i18n/de.json').then(x => x.default),
};

type TranslateKeys<T> = T extends object
  ? T extends unknown[]
    ? never
    : T extends Set<unknown>
      ? never
      : T extends Map<unknown, unknown>
        ? never
        : T extends Function
          ? never
          : {
              [K in keyof T]: K extends string
                ? T[K] extends string
                  ? K
                  : `${K}_${TranslateKeys<T[K]>}`
                : never;
            }[keyof T]
  : never;
export type TranslationKey = TranslateKeys<typeof en>;

const langLocalStorageKey = 'lang';

@Injectable({ providedIn: 'root' })
export class TranslateService {
  private readonly _translations = signal<typeof en | undefined>(undefined);
  private readonly _language = signal(getLocalStorage(langLocalStorageKey));

  public readonly translations = toTranslationsSignal(this._translations);
  public readonly browserLanguage = computed(() => navigator.language ?? 'en');
  public readonly language = computed(() => this._language() ?? this.browserLanguage());

  public readonly languageItems = computed(() => [
    {
      label: `${this.translations.settings_useSystemLanguage()} (${this.getLangDisplay(
        this.browserLanguage()
      )})`,
      icon: this.isLanguage(null) ? 'i-[mdi--check]' : 'i-[mdi--translate]',
      command: () => this.setLanguage(null),
    },
    {
      separator: true,
    },
    {
      label: this.getLangDisplay('en'),
      icon: this.isLanguage('en') ? 'i-[mdi--check]' : 'i-[flag--us-1x1]',
      command: () => this.setLanguage('en'),
    },
    {
      label: this.getLangDisplay('de'),
      icon: this.isLanguage('de') ? 'i-[mdi--check]' : 'i-[flag--de-1x1]',
      command: () => this.setLanguage('de'),
    },
  ]);

  constructor() {
    effect(() => {
      let getTranslations = langs[this.language()];
      if (!getTranslations && this.language().includes('-')) {
        getTranslations = langs[this.language().split('-')[0]];
      }
      if (!getTranslations) {
        getTranslations = langs['en'];
      }

      getTranslations().then(x => this._translations.set(JSON.parse(JSON.stringify(x))));
    });
  }

  public setLanguage(language: string | null) {
    setLocalStorage(langLocalStorageKey, language);
    this._language.set(language);
  }

  public isLanguage(language: string | null) {
    return this._language() === language;
  }

  private getLangDisplay(lang: string) {
    switch (lang) {
      case 'en':
        return 'English';
      case 'de':
        return 'Deutsch';
      default:
        return lang;
    }
  }
}

export function isFunctionKey(key: string): key is Extract<keyof Function, string> {
  return (key in Function.prototype && key !== 'constructor') || key === 'prototype';
}
export type TranslationsSignal<T> = Signal<T> &
  Readonly<{
    [K in TranslateKeys<T>]: Signal<string> & { key: K };
  }>;
function toTranslationsSignal<T>(signal: Signal<T | undefined>): TranslationsSignal<T> {
  return new Proxy(signal, {
    get(target: any, prop) {
      if (typeof prop !== 'string' || isFunctionKey(prop)) {
        return target[prop];
      }
      const sig = computed(() => getDeepValue(target(), prop.split('_')));
      Object.defineProperty(sig, 'key', { value: prop });
      return sig;
    },
  }) as TranslationsSignal<T>;
}

function getDeepValue<T>(obj: T, path: string[]): unknown {
  return path.reduce((xs, x) => (xs && (xs as any)[x] ? (xs as any)[x] : null), obj);
}
