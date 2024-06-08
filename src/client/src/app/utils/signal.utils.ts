import { Injector, Signal, effect, isSignal } from '@angular/core';

export function chainSignals<S1, S2>(
  s1: Signal<S1>,
  s2: (s1: Signal<S1>) => Signal<S2>
): Signal<S2>;
export function chainSignals<S1, S2, S3>(
  s1: Signal<S1>,
  s2: (s1: Signal<S1>) => Signal<S2>,
  s3: (s1: Signal<S1>, s2: Signal<S2>) => Signal<S3>
): Signal<S3>;
export function chainSignals<S1, S2, S3, S4>(
  s1: Signal<S1>,
  s2: (s1: Signal<S1>) => Signal<S2>,
  s3: (s1: Signal<S1>, s2: Signal<S2>) => Signal<S3>,
  s4: (s1: Signal<S1>, s2: Signal<S2>, s3: Signal<S3>) => Signal<S4>
): Signal<S4>;
export function chainSignals(
  ...signals: ((...args: Signal<unknown>[]) => Signal<unknown>)[]
): Signal<unknown> {
  const processedSignals: Signal<unknown>[] = [];
  for (const signal of signals) {
    if (isSignal(signal)) {
      processedSignals.push(signal);
    } else {
      processedSignals.push(signal(...processedSignals));
    }
  }
  const last = processedSignals.pop();
  if (!last) {
    throw new Error('No signals provided');
  }
  Object.defineProperty(last, Symbol(), { value: processedSignals });
  return last;
}

export async function ensureSignalValue<T>(
  signal: Signal<T | null | undefined>,
  options?: { injector?: Injector }
): Promise<T> {
  const value = signal();
  if (value !== null && value !== undefined) return value;
  return new Promise<T>(resolve => {
    const e = effect(
      () => {
        const value = signal();
        if (value !== null && value !== undefined) {
          e.destroy();
          resolve(value);
        }
      },
      { injector: options?.injector }
    );
  });
}
