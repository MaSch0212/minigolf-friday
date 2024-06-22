import { DestroyRef, Signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Store } from '@ngrx/store';
import { filter } from 'rxjs';

import { loadMapsAction } from './maps.actions';
import { selectMapsActionState } from './maps.selectors';
import { injectEx, OptionalInjector } from '../../utils/angular.utils';

export function keepMapsLoaded(options?: OptionalInjector & { enabled?: Signal<boolean> }) {
  const store = injectEx(Store, options);
  store.dispatch(loadMapsAction({ reload: false }));
  store
    .select(selectMapsActionState('load'))
    .pipe(
      filter(x => x.state === 'none'),
      takeUntilDestroyed(injectEx(DestroyRef, options))
    )
    .subscribe(() => store.dispatch(loadMapsAction({ reload: true, silent: true })));
}
