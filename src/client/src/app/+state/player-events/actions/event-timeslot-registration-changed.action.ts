import { createAction, on, props } from '@ngrx/store';
import { produce } from 'immer';

import { PlayerEventTimeslotRegistrationChangedRealtimeEvent } from '../../../models/realtime-events';
import { Reducers } from '../../utils';
import { PLAYER_EVENTS_ACTION_SCOPE } from '../consts';
import { playerEventEntityAdapter, PlayerEventsFeatureState } from '../player-events.state';

export const eventTimeslotRegistrationChangedAction = createAction(
  `[${PLAYER_EVENTS_ACTION_SCOPE}] Event Timeslot Registration Changed`,
  props<PlayerEventTimeslotRegistrationChangedRealtimeEvent>()
);

export const eventTimeslotRegistrationChangedReducers: Reducers<PlayerEventsFeatureState> = [
  on(eventTimeslotRegistrationChangedAction, (state, event) =>
    playerEventEntityAdapter.mapOne(
      {
        id: event.eventId,
        map: produce(draft => {
          if (!draft.playerEventRegistrations) {
            draft.playerEventRegistrations = [];
          }
          let registration = draft.playerEventRegistrations.find(x => x.userId == event.userId);
          if (!registration) {
            registration = {
              userId: event.userId,
              userAlias: event.userAlias,
              registeredTimeslotIds: [],
            };
            draft.playerEventRegistrations.push(registration);
          }
          if (event.isRegistered) {
            registration.registeredTimeslotIds.push(event.eventTimeslotId);
          } else {
            registration.registeredTimeslotIds = registration.registeredTimeslotIds.filter(
              x => x !== event.eventTimeslotId
            );
          }
          if (registration.registeredTimeslotIds.length === 0) {
            draft.playerEventRegistrations = draft.playerEventRegistrations.filter(
              x => x.userId !== event.userId
            );
          }
        }),
      },
      state
    )
  ),
];
