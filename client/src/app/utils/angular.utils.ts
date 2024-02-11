import { InjectOptions, Injector, ProviderToken, inject } from '@angular/core';

export type OptionalInjector = { injector?: Injector };
export type InjectExOptions = InjectOptions & OptionalInjector;

export function injectEx<T>(token: ProviderToken<T>, options?: OptionalInjector): T;
export function injectEx<T>(
  token: ProviderToken<T>,
  options: InjectExOptions & { optional?: false }
): T;
export function injectEx<T>(token: ProviderToken<T>, options: InjectExOptions): T | null;
export function injectEx<T>(token: ProviderToken<T>, options?: InjectExOptions): T | null {
  if (options?.injector) {
    return options.injector.get<T>(token, null, options);
  }
  return options ? inject(token, options) : inject(token);
}
