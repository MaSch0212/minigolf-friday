import { effect } from '@angular/core';
import { Store } from '@ngrx/store';

import { loadUsersAction } from './users.actions';
import { selectUsersActionState } from './users.selectors';
import { injectEx, OptionalInjector } from '../../utils/angular.utils';

export function keepUsersLoaded(options?: OptionalInjector & { reload?: boolean }) {
  const store = injectEx(Store, options);
  const actionState = store.selectSignal(selectUsersActionState('load'));

  if (actionState().state !== 'none' && options?.reload) {
    store.dispatch(loadUsersAction({ reload: true }));
  }

  effect(
    () => {
      if (actionState().state === 'none') {
        store.dispatch(loadUsersAction({ reload: false }));
      }
    },
    { ...options, allowSignalWrites: true }
  );
}
