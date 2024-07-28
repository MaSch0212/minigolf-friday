import { createAction, on, props } from '@ngrx/store';
import { produce } from 'immer';

import { PlayerEventTimeslotRegistrationChangedRealtimeEvent } from '../../../models/realtime-events';
import { Reducers } from '../../utils';
import { EVENTS_ACTION_SCOPE } from '../consts';
import { eventEntityAdapter, EventsFeatureState } from '../events.state';

export const eventTimeslotRegistrationChangedAction = createAction(
  `[${EVENTS_ACTION_SCOPE}] Event Timeslot Registration Changed`,
  props<PlayerEventTimeslotRegistrationChangedRealtimeEvent>()
);

export const eventTimeslotRegistrationChangedReducers: Reducers<EventsFeatureState> = [
  on(eventTimeslotRegistrationChangedAction, (state, event) =>
    eventEntityAdapter.mapOne(
      {
        id: event.eventId,
        map: produce(draft => {
          const timeslot = draft.timeslots.find(t => t.id === event.eventTimeslotId);
          if (!timeslot) return;
          if (event.isRegistered) {
            if (!timeslot.playerIds.includes(event.userId)) {
              timeslot.playerIds.push(event.userId);
            }
          } else {
            timeslot.playerIds = timeslot.playerIds.filter(p => p !== event.userId);
          }
        }),
      },
      state
    )
  ),
];
