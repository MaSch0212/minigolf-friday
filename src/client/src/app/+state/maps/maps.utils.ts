import { effect } from '@angular/core';
import { Store } from '@ngrx/store';

import { loadMapsAction } from './maps.actions';
import { selectMapsActionState } from './maps.selectors';
import { injectEx, OptionalInjector } from '../../utils/angular.utils';

export function keepMapsLoaded(options?: OptionalInjector & { reload?: boolean }) {
  const store = injectEx(Store, options);
  const actionState = store.selectSignal(selectMapsActionState('load'));

  if (actionState().state !== 'none' && options?.reload) {
    store.dispatch(loadMapsAction({ reload: true }));
  }

  effect(
    () => {
      if (actionState().state === 'none') {
        store.dispatch(loadMapsAction({ reload: false }));
      }
    },
    { ...options, allowSignalWrites: true }
  );
}
