import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { produce } from 'immer';
import { switchMap } from 'rxjs';

import { RegisterForEventRequest } from '../../../models/api/player-event';
import { PlayerEventsService } from '../../../services/player-events.service';
import {
  createHttpAction,
  handleHttpAction,
  mapToHttpAction,
  onHttpAction,
} from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { PLAYER_EVENTS_ACTION_SCOPE } from '../consts';
import { selectPlayerEventsActionState } from '../player-events.selectors';
import { PlayerEventsFeatureState, playerEventEntityAdapter } from '../player-events.state';

export const registerForEventAction = createHttpAction<{
  eventId: string;
  registrations: RegisterForEventRequest['timeslotRegistrations'];
}>()(PLAYER_EVENTS_ACTION_SCOPE, 'Register For Event');

export const registerForEventReducers: Reducers<PlayerEventsFeatureState> = [
  on(registerForEventAction, (state, { props }) =>
    playerEventEntityAdapter.mapOne(
      {
        id: props.eventId,
        map: produce(draft => {
          for (const timeslot of draft.timeslots) {
            const reg = props.registrations.find(r => r.timeslotId === timeslot.id);
            timeslot.isRegistered = reg !== undefined;
            timeslot.chosenFallbackTimeslotId = reg?.fallbackTimeslotId;
          }
        }),
      },
      state
    )
  ),
  handleHttpAction('register', registerForEventAction),
];

export const registerForEventEffects: Effects = {
  registerForEvent$: createFunctionalEffect.dispatching((api = inject(PlayerEventsService)) =>
    onHttpAction(registerForEventAction, selectPlayerEventsActionState('register')).pipe(
      switchMap(({ props }) =>
        api
          .registerForEvent(props.eventId, { timeslotRegistrations: props.registrations })
          .pipe(mapToHttpAction(registerForEventAction, props))
      )
    )
  ),
};
