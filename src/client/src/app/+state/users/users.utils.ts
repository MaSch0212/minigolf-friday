import { DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Store } from '@ngrx/store';
import { filter } from 'rxjs';

import { loadUsersAction } from './users.actions';
import { selectUsersActionState } from './users.selectors';
import { injectEx, OptionalInjector } from '../../utils/angular.utils';

export function keepUsersLoaded(options?: OptionalInjector) {
  const store = injectEx(Store, options);
  store.dispatch(loadUsersAction({ reload: false }));
  store
    .select(selectUsersActionState('load'))
    .pipe(
      filter(x => x.state === 'none'),
      takeUntilDestroyed(injectEx(DestroyRef, options))
    )
    .subscribe(() => store.dispatch(loadUsersAction({ reload: true, silent: true })));
}
