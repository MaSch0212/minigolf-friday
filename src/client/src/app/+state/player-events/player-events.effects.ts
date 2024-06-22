import { inject } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { Store } from '@ngrx/store';
import { EMPTY, filter, map, mergeMap, of, skip, withLatestFrom } from 'rxjs';

import { loadPlayerEventAction, loadPlayerEventEffects } from './actions/load-player-event.action';
import { loadPlayerEventsEffects } from './actions/load-player-events.action';
import { updateEventRegistrationEffects } from './actions/update-event-registration.action';
import { playerEventRemovedAction, resetPlayerEventsAction } from './player-events.actions';
import { playerEventSelectors } from './player-events.selectors';
import { RealtimeEventsService } from '../../services/realtime-events.service';
import { createFunctionalEffect } from '../functional-effect';
import { Effects } from '../utils';

export const PlayerEventsFeatureEffects: Effects[] = [
  loadPlayerEventsEffects,
  loadPlayerEventEffects,
  updateEventRegistrationEffects,
  {
    playerEventUpdated$: createFunctionalEffect.dispatching((store = inject(Store)) =>
      inject(RealtimeEventsService).playerEventChanged.pipe(
        withLatestFrom(store.select(playerEventSelectors.selectEntities)),
        mergeMap(([{ eventId, changeType }, entities]) => {
          if (changeType === 'updated') {
            return eventId in entities
              ? of(loadPlayerEventAction({ eventId, reload: true, silent: true }))
              : EMPTY;
          } else if (changeType === 'created') {
            return of(loadPlayerEventAction({ eventId, reload: true, silent: true }));
          } else if (changeType === 'deleted') {
            return of(playerEventRemovedAction({ eventId }));
          }
          return EMPTY;
        })
      )
    ),
    playerEventRegistrationUpdated$: createFunctionalEffect.dispatching((store = inject(Store)) =>
      inject(RealtimeEventsService).playerEventRegistrationChanged.pipe(
        withLatestFrom(store.select(playerEventSelectors.selectEntities)),
        mergeMap(([{ eventId }, entities]) =>
          eventId in entities
            ? of(loadPlayerEventAction({ eventId, reload: true, silent: true }))
            : EMPTY
        )
      )
    ),

    onServerReconnect$: createFunctionalEffect.dispatching(() =>
      toObservable(inject(RealtimeEventsService).isConnected).pipe(
        skip(1),
        filter(x => x),
        map(() => resetPlayerEventsAction())
      )
    ),
  },
];
