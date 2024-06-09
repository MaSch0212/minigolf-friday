import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { produce } from 'immer';
import { switchMap } from 'rxjs';

import { ApiEventTimeslotRegistration } from '../../../api/models';
import { EventsService } from '../../../api/services';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { PLAYER_EVENTS_ACTION_SCOPE } from '../consts';
import { selectPlayerEventsActionState } from '../player-events.selectors';
import { PlayerEventsFeatureState, playerEventEntityAdapter } from '../player-events.state';

export const registerForEventAction = createHttpAction<{
  eventId: string;
  registrations: ApiEventTimeslotRegistration[];
}>()(PLAYER_EVENTS_ACTION_SCOPE, 'Register For Event');

export const registerForEventReducers: Reducers<PlayerEventsFeatureState> = [
  on(registerForEventAction.success, (state, { props }) =>
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
  registerForEvent$: createFunctionalEffect.dispatching((api = inject(EventsService)) =>
    onHttpAction(registerForEventAction, selectPlayerEventsActionState('register')).pipe(
      switchMap(({ props }) =>
        toHttpAction(updatePlayerEventRegistrations(api, props), registerForEventAction, props)
      )
    )
  ),
};

async function updatePlayerEventRegistrations(
  api: EventsService,
  props: ReturnType<typeof registerForEventAction>['props']
) {
  const response = await api.updatePlayerEventRegistrations({
    eventId: props.eventId,
    body: { timeslotRegistrations: props.registrations },
  });
  return response.ok
    ? registerForEventAction.success(props, undefined)
    : registerForEventAction.error(props, response);
}
