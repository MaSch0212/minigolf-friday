import { effect } from '@angular/core';
import { Store } from '@ngrx/store';

import { loadUserSettingsAction } from './user-settings.actions';
import { selectUserSettingsActionState } from './user-settings.selectors';
import { injectEx, OptionalInjector } from '../../utils/angular.utils';

export function keepUserSettingsLoaded(options?: OptionalInjector & { reload?: boolean }) {
  const store = injectEx(Store, options);
  const actionState = store.selectSignal(selectUserSettingsActionState('load'));

  if (actionState().state !== 'none' && options?.reload) {
    store.dispatch(loadUserSettingsAction({ reload: true }));
  }

  effect(
    () => {
      if (actionState().state === 'none') {
        store.dispatch(loadUserSettingsAction({ reload: false }));
      }
    },
    { ...options, allowSignalWrites: true }
  );
}
