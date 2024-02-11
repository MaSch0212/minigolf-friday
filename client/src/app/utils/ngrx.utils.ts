import { Signal, computed, isSignal } from '@angular/core';
import { Selector, Store } from '@ngrx/store';

import { OptionalInjector, injectEx } from './angular.utils';
import { chainSignals } from './signal.utils';

export function selectSignal<T>(
  selector: Signal<Selector<object, T>> | Selector<object, T>,
  options?: OptionalInjector
): Signal<T> {
  const store = injectEx(Store, options);
  if (isSignal(selector)) {
    return chainSignals(
      selector,
      s => computed(() => store.selectSignal(s())),
      (_, s) => computed(() => s()())
    );
  } else {
    return store.selectSignal(selector);
  }
}
