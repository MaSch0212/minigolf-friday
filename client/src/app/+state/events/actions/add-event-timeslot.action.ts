import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { castDraft, produce } from 'immer';
import { switchMap } from 'rxjs';

import { EventAdministrationService } from '../../../api/services';
import { EventTimeslot, parseEventTimeslot } from '../../../models/parsed-models';
import { Time, timeToString } from '../../../utils/date.utils';
import { assertBody } from '../../../utils/http.utils';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { EVENTS_ACTION_SCOPE } from '../consts';
import { selectEventsActionState } from '../events.selectors';
import { EventsFeatureState, eventEntityAdapter } from '../events.state';

export const addEventTimeslotAction = createHttpAction<
  {
    eventId: string;
    time: Time;
    mapId: string;
    isFallbackAllowed: boolean;
  },
  EventTimeslot
>()(EVENTS_ACTION_SCOPE, 'Add Timeslot');

export const addEventTimeslotReducers: Reducers<EventsFeatureState> = [
  on(addEventTimeslotAction.success, (state, { props, response }) => {
    const entity = state.entities[props.eventId];
    if (!entity) return state;
    return eventEntityAdapter.upsertOne(
      produce(entity, draft => {
        draft.timeslots.push(castDraft(response));
      }),
      state
    );
  }),
  handleHttpAction('addTimeslot', addEventTimeslotAction, (s, p) => !!s.entities[p.eventId]),
];

export const addEventTimeslotEffects: Effects = {
  addEventTimeslot$: createFunctionalEffect.dispatching(
    (api = inject(EventAdministrationService)) =>
      onHttpAction(addEventTimeslotAction, selectEventsActionState('addTimeslot')).pipe(
        switchMap(({ props }) =>
          toHttpAction(createEventTimeslot(api, props), addEventTimeslotAction, props)
        )
      )
  ),
};

async function createEventTimeslot(
  api: EventAdministrationService,
  props: ReturnType<typeof addEventTimeslotAction>['props']
) {
  const response = await api.createEventTimeslot({
    eventId: props.eventId,
    body: {
      time: timeToString(props.time, 'minutes'),
      mapId: props.mapId,
      isFallbackAllowed: props.isFallbackAllowed,
    },
  });
  return response.ok
    ? addEventTimeslotAction.success(props, parseEventTimeslot(assertBody(response).timeslot))
    : addEventTimeslotAction.error(props, response);
}
