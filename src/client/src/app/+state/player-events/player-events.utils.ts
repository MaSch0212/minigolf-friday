import { DestroyRef, effect, Signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Store } from '@ngrx/store';
import { filter } from 'rxjs';

import { loadPlayerEventAction, loadPlayerEventsAction } from './player-events.actions';
import { selectPlayerEventsActionState } from './player-events.selectors';
import { injectEx, OptionalInjector } from '../../utils/angular.utils';

export function keepPlayerEventsLoaded(options?: OptionalInjector) {
  const store = injectEx(Store, options);
  store.dispatch(loadPlayerEventsAction({ reload: false }));
  store
    .select(selectPlayerEventsActionState('load'))
    .pipe(
      filter(x => x.state === 'none'),
      takeUntilDestroyed(injectEx(DestroyRef, options))
    )
    .subscribe(() => store.dispatch(loadPlayerEventsAction({ reload: true, silent: true })));
}

export function keepPlayerEventLoaded(eventId: Signal<string>, options?: OptionalInjector) {
  const store = injectEx(Store, options);

  effect(() => store.dispatch(loadPlayerEventAction({ eventId: eventId(), reload: false })), {
    ...options,
    allowSignalWrites: true,
  });

  store
    .select(selectPlayerEventsActionState('loadOne'))
    .pipe(
      filter(x => x.state === 'none'),
      takeUntilDestroyed(injectEx(DestroyRef, options))
    )
    .subscribe(() =>
      store.dispatch(loadPlayerEventAction({ eventId: eventId(), reload: true, silent: true }))
    );
}
