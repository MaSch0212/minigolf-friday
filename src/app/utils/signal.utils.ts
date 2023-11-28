import { Signal, isSignal } from '@angular/core';

export function chainSignals<S1, S2>(
  s1: (() => Signal<S1>) | Signal<S1>,
  s2: ((s1: Signal<S1>) => Signal<S2>) | Signal<S2>
): Signal<S2>;
export function chainSignals<S1, S2, S3>(
  s1: (() => Signal<S1>) | Signal<S1>,
  s2: ((s1: Signal<S1>) => Signal<S2>) | Signal<S2>,
  s3: ((s1: Signal<S1>, s2: Signal<S2>) => Signal<S3>) | Signal<S3>
): Signal<S3>;
export function chainSignals<S1, S2, S3, S4>(
  s1: (() => Signal<S1>) | Signal<S1>,
  s2: ((s1: Signal<S1>) => Signal<S2>) | Signal<S2>,
  s3: ((s1: Signal<S1>, s2: Signal<S2>) => Signal<S3>) | Signal<S3>,
  s4: ((s1: Signal<S1>, s2: Signal<S2>, s3: Signal<S3>) => Signal<S4>) | Signal<S4>
): Signal<S4>;
export function chainSignals(
  ...signals: (Signal<unknown> | ((...args: Signal<unknown>[]) => Signal<unknown>))[]
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
