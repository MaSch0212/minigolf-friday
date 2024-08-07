import { DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Store } from '@ngrx/store';
import { filter } from 'rxjs';

import { loadUserSettingsAction } from './user-settings.actions';
import { selectUserSettingsActionState } from './user-settings.selectors';
import { injectEx, OptionalInjector } from '../../utils/angular.utils';

export function keepUserSettingsLoaded(options?: OptionalInjector) {
  const store = injectEx(Store, options);
  store.dispatch(loadUserSettingsAction({ reload: false }));
  store
    .select(selectUserSettingsActionState('load'))
    .pipe(
      filter(x => x.state === 'none'),
      takeUntilDestroyed(injectEx(DestroyRef, options))
    )
    .subscribe(() => store.dispatch(loadUserSettingsAction({ reload: true, silent: true })));
}
