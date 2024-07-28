import { inject } from '@angular/core';
import { EMPTY, map, merge, mergeMap, of } from 'rxjs';

import { eventTimeslotRegistrationChangedAction } from './actions/event-timeslot-registration-changed.action';
import { loadPlayerEventAction, loadPlayerEventEffects } from './actions/load-player-event.action';
import { loadPlayerEventsEffects } from './actions/load-player-events.action';
import { updateEventRegistrationEffects } from './actions/update-event-registration.action';
import {
  playerEventRemovedAction,
  resetPlayerEventsActionStateAction,
} from './player-events.actions';
import { RealtimeEventsService } from '../../services/realtime-events.service';
import { createFunctionalEffect } from '../functional-effect';
import { Effects } from '../utils';

export const PlayerEventsFeatureEffects: Effects[] = [
  loadPlayerEventsEffects,
  loadPlayerEventEffects,
  updateEventRegistrationEffects,
  {
    playerEventUpdated$: createFunctionalEffect.dispatching(
      (events = inject(RealtimeEventsService)) =>
        merge(events.playerEventChanged, events.playerEventRegistrationChanged).pipe(
          mergeMap(event => {
            const eventId = event.eventId;
            const changeType = 'changeType' in event ? event.changeType : 'updated';
            if (changeType === 'updated' || changeType === 'created') {
              return of(loadPlayerEventAction({ eventId, reload: true, silent: true }));
            } else if (changeType === 'deleted') {
              return of(playerEventRemovedAction({ eventId }));
            }
            return EMPTY;
          })
        )
    ),

    eventTimeslotRegistrationChanged$: createFunctionalEffect.dispatching(() =>
      inject(RealtimeEventsService).playerEventTimeslotRegistrationChanged.pipe(
        map(event => eventTimeslotRegistrationChangedAction(event))
      )
    ),

    onServerReconnected$: createFunctionalEffect.dispatching(() =>
      inject(RealtimeEventsService).onReconnected$.pipe(
        mergeMap(() =>
          of(
            resetPlayerEventsActionStateAction({ scope: 'load' }),
            resetPlayerEventsActionStateAction({ scope: 'loadOne' })
          )
        )
      )
    ),
  },
];
