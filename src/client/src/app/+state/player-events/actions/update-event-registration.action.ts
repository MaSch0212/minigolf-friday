import { inject } from '@angular/core';
import { concatLatestFrom } from '@ngrx/operators';
import { on, Store } from '@ngrx/store';
import { produce } from 'immer';
import { switchMap } from 'rxjs';

import { ApiEventTimeslotRegistration } from '../../../api/models';
import { EventsService } from '../../../api/services';
import { PlayerEvent } from '../../../models/parsed-models';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { PLAYER_EVENTS_ACTION_SCOPE } from '../consts';
import { selectPlayerEvent, selectPlayerEventsActionState } from '../player-events.selectors';
import { PlayerEventsFeatureState, playerEventEntityAdapter } from '../player-events.state';

export const updateEventRegistrationAction = createHttpAction<{
  eventId: string;
  timeslotId: string;
  isRegistered?: boolean;
  fallbackTimeslotId?: string | null;
}>()(PLAYER_EVENTS_ACTION_SCOPE, 'Update Event Registration');

export const updateEventRegistrationReducers: Reducers<PlayerEventsFeatureState> = [
  on(updateEventRegistrationAction.success, (state, { props }) =>
    playerEventEntityAdapter.mapOne(
      {
        id: props.eventId,
        map: produce(draft => {
          const timeslot = draft.timeslots.find(t => t.id === props.timeslotId);
          if (timeslot) {
            if (props.isRegistered !== undefined) {
              timeslot.isRegistered = props.isRegistered;
            }
            if (props.fallbackTimeslotId !== undefined) {
              timeslot.chosenFallbackTimeslotId = props.fallbackTimeslotId;
            }
          }
        }),
      },
      state
    )
  ),
  handleHttpAction('register', updateEventRegistrationAction),
];

export const updateEventRegistrationEffects: Effects = {
  updateEventRegistration$: createFunctionalEffect.dispatching(
    (store = inject(Store), api = inject(EventsService)) =>
      onHttpAction(updateEventRegistrationAction, selectPlayerEventsActionState('register')).pipe(
        concatLatestFrom(({ props }) => store.select(selectPlayerEvent(props.eventId))),
        switchMap(([{ props }, currentEvent]) =>
          toHttpAction(
            updatePlayerEventRegistrations(api, props, currentEvent),
            updateEventRegistrationAction,
            props
          )
        )
      )
  ),
};

async function updatePlayerEventRegistrations(
  api: EventsService,
  props: ReturnType<typeof updateEventRegistrationAction>['props'],
  currentTimeslot: PlayerEvent | undefined
) {
  if (!currentTimeslot) return updateEventRegistrationAction.error(props, 'Event not found');

  let registrations: ApiEventTimeslotRegistration[] = currentTimeslot.timeslots
    .filter(t => t.isRegistered)
    .map(t => ({
      timeslotId: t.id,
      fallbackTimeslotId: t.chosenFallbackTimeslotId,
    }));

  let timeslotRegistration = registrations.find(r => r.timeslotId === props.timeslotId);
  if (props.isRegistered === true && !timeslotRegistration) {
    timeslotRegistration = {
      timeslotId: props.timeslotId,
      fallbackTimeslotId: props.fallbackTimeslotId,
    };
    registrations.push(timeslotRegistration);
  } else if (props.isRegistered === false && timeslotRegistration) {
    registrations = registrations.filter(r => r.timeslotId !== props.timeslotId);
    timeslotRegistration = undefined;
  }

  if (timeslotRegistration && props.fallbackTimeslotId !== undefined) {
    timeslotRegistration.fallbackTimeslotId = props.fallbackTimeslotId;
  }

  const response = await api.updatePlayerEventRegistrations({
    eventId: props.eventId,
    body: { timeslotRegistrations: registrations },
  });
  return response.ok
    ? updateEventRegistrationAction.success(props, undefined)
    : updateEventRegistrationAction.error(props, response);
}
