import { registerLocaleData } from '@angular/common';
import { Injectable, Signal, computed, effect, inject, signal } from '@angular/core';
import { PrimeNGConfig } from 'primeng/api';

import { getLocalStorage, setLocalStorage } from '../utils/local-storage.utils';

import type en from '../i18n/en.json';
import type primengEn from '../i18n/primeng.en.json';

type LangType = {
  translations: typeof en;
  locale: unknown;
  localeExtra: unknown;
  primengTranslations: typeof primengEn;
};
const langs: Record<string, () => Promise<LangType>> = {
  en: () => import('../i18n/en').then(x => x.default),
  de: () => import('../i18n/de').then(x => x.default),
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
  private readonly _primengConfig = inject(PrimeNGConfig);

  private readonly _translations = signal<typeof en | undefined>(undefined);
  private readonly _language = signal(getLocalStorage(langLocalStorageKey));

  public readonly translations = toTranslationsSignal(this._translations);
  public readonly browserLanguage = computed(() => navigator.language ?? 'en');
  public readonly language = computed(() => this._language() ?? this.browserLanguage());

  constructor() {
    effect(() => {
      const lang = this.language();

      document.documentElement.lang = lang;

      let getTranslations = langs[lang];
      if (!getTranslations && lang.includes('-')) {
        getTranslations = langs[lang.split('-')[0]];
      }
      if (!getTranslations) {
        getTranslations = langs['en'];
      }

      getTranslations().then(({ translations, locale, localeExtra, primengTranslations }) => {
        this._translations.set(JSON.parse(JSON.stringify(translations)));
        registerLocaleData(locale, lang, localeExtra);
        this._primengConfig.setTranslation(primengTranslations);
      });
    });
  }

  public translate(key: string) {
    const path = key.split('_');
    return (getDeepValue(this.translations(), path) as string) ?? key;
  }

  public setLanguage(language: string | null) {
    setLocalStorage(langLocalStorageKey, language);
    this._language.set(language);
  }

  public isLanguage(language: string | null) {
    return this._language() === language;
  }

  public getLangDisplay(lang: string | null) {
    lang ??= this.browserLanguage();
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
