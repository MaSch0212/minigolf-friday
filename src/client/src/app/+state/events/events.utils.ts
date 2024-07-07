import { DestroyRef, effect, Signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Store } from '@ngrx/store';
import { filter } from 'rxjs';

import { loadEventsAction, loadEventAction } from './events.actions';
import { selectEventsActionState } from './events.selectors';
import { injectEx, OptionalInjector } from '../../utils/angular.utils';

export function keepEventsLoaded(options?: OptionalInjector) {
  const store = injectEx(Store, options);
  store.dispatch(loadEventsAction({ reload: false }));
  store
    .select(selectEventsActionState('load'))
    .pipe(
      filter(x => x.state === 'none'),
      takeUntilDestroyed(injectEx(DestroyRef, options))
    )
    .subscribe(() => store.dispatch(loadEventsAction({ reload: true, silent: true })));
}

export function keepEventLoaded(
  eventId: Signal<string | null | undefined>,
  options?: OptionalInjector
) {
  const store = injectEx(Store, options);

  effect(
    () => {
      const id = eventId();
      if (id) {
        store.dispatch(loadEventAction({ eventId: id, reload: false }));
      }
    },
    {
      ...options,
      allowSignalWrites: true,
    }
  );

  store
    .select(selectEventsActionState('loadOne'))
    .pipe(
      filter(x => x.state === 'none'),
      takeUntilDestroyed(injectEx(DestroyRef, options))
    )
    .subscribe(() => {
      const id = eventId();
      if (id) {
        store.dispatch(loadEventAction({ eventId: id, reload: true, silent: true }));
      }
    });
}
