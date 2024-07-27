import { inject } from '@angular/core';
import { mergeMap, of, EMPTY, map, merge } from 'rxjs';

import { addEventPreconfigEffects } from './actions/add-event-preconfig.action';
import { addEventTimeslotEffects } from './actions/add-event-timeslot.action';
import { addEventEffects } from './actions/add-event.action';
import { addPlayerToEventPreconfigurationEffects } from './actions/add-player-to-preconfig.action';
import { buildEventInstancesEffects } from './actions/build-event-instances.action';
import { loadEventAction, loadEventEffects } from './actions/load-event.action';
import { loadEventsEffects } from './actions/load-events.action';
import { removeEventPreconfigEffects } from './actions/remove-event-preconfig.action';
import { removeEventTimeslotEffects } from './actions/remove-event-timeslot.action';
import { removeEventAction, removeEventEffects } from './actions/remove-event.action';
import { removePlayerFromPreconfigEffects } from './actions/remove-player-from-preconfig.action';
import { setEventInstancesEffects } from './actions/set-event-instances.action';
import { startEventEffects } from './actions/start-event.action';
import { updateEventTimeslotEffects } from './actions/update-event-timeslot.action';
import { updateEventEffects } from './actions/update-event.action';
import {
  eventTimeslotRegistrationChangedAction,
  resetEventsActionStateAction,
} from './events.actions';
import { RealtimeEventsService } from '../../services/realtime-events.service';
import { createFunctionalEffect } from '../functional-effect';
import { Effects } from '../utils';

export const eventsFeatureEffects: Effects[] = [
  addEventPreconfigEffects,
  addEventTimeslotEffects,
  addEventEffects,
  addPlayerToEventPreconfigurationEffects,
  buildEventInstancesEffects,
  loadEventEffects,
  loadEventsEffects,
  removeEventPreconfigEffects,
  removeEventTimeslotEffects,
  removeEventEffects,
  removePlayerFromPreconfigEffects,
  setEventInstancesEffects,
  startEventEffects,
  updateEventEffects,
  updateEventTimeslotEffects,
  {
    eventUpdated$: createFunctionalEffect.dispatching((events = inject(RealtimeEventsService)) =>
      merge(
        events.eventChanged,
        events.eventTimeslotChanged,
        events.eventPreconfigurationChanged,
        events.eventInstancesChanged
      ).pipe(
        mergeMap(event => {
          const eventId = event.eventId;
          let changeType = 'changeType' in event ? event.changeType : 'updated';
          if (changeType === 'deleted' && 'eventTimeslotId' in event) {
            changeType = 'updated';
          }
          if (changeType === 'updated' || changeType === 'created') {
            return of(loadEventAction({ eventId, reload: true, silent: true }));
          } else if (changeType === 'deleted') {
            return of(removeEventAction.success({ eventId }, undefined));
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
            resetEventsActionStateAction({ scope: 'load' }),
            resetEventsActionStateAction({ scope: 'loadOne' })
          )
        )
      )
    ),
  },
];
