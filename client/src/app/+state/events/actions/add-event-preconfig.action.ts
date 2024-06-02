import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { castDraft, produce } from 'immer';
import { switchMap } from 'rxjs';

import { EventAdministrationService } from '../../../api/services';
import { EventInstancePreconfiguration } from '../../../models/parsed-models';
import { assertBody } from '../../../utils/http.utils';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { EVENTS_ACTION_SCOPE } from '../consts';
import { selectEventsActionState } from '../events.selectors';
import { EventsFeatureState, eventEntityAdapter } from '../events.state';

export const addEventPreconfigAction = createHttpAction<
  { eventId: string; timeslotId: string },
  EventInstancePreconfiguration
>()(EVENTS_ACTION_SCOPE, 'Add Preconfig');

export const addEventPreconfigReducers: Reducers<EventsFeatureState> = [
  on(addEventPreconfigAction.success, (state, { props, response }) => {
    const entity = state.entities[props.eventId];
    if (!entity) return state;
    return eventEntityAdapter.upsertOne(
      produce(entity, draft => {
        const timeslot = draft.timeslots.find(t => t.id === props.timeslotId);
        if (!timeslot) return;
        timeslot.preconfigurations.push(castDraft(response));
      }),
      state
    );
  }),
  handleHttpAction('addPreconfig', addEventPreconfigAction),
];

export const addEventPreconfigEffects: Effects = {
  addEventPreconfig$: createFunctionalEffect.dispatching(
    (api = inject(EventAdministrationService)) =>
      onHttpAction(addEventPreconfigAction, selectEventsActionState('addPreconfig')).pipe(
        switchMap(({ props }) =>
          toHttpAction(createPreconfiguration(api, props), addEventPreconfigAction, props)
        )
      )
  ),
};

async function createPreconfiguration(
  api: EventAdministrationService,
  props: ReturnType<typeof addEventPreconfigAction>['props']
) {
  const response = await api.createPreconfiguration({ timeslotId: props.timeslotId });
  return response.ok
    ? addEventPreconfigAction.success(props, assertBody(response).preconfiguration)
    : addEventPreconfigAction.error(props, response);
}
