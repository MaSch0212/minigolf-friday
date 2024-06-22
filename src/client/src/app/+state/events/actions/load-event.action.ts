import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { switchMap } from 'rxjs';

import { EventAdministrationService } from '../../../api/services';
import { Event, parseEvent } from '../../../models/parsed-models';
import { assertBody } from '../../../utils/http.utils';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { EVENTS_ACTION_SCOPE } from '../consts';
import { selectEventsActionState } from '../events.selectors';
import { EventsFeatureState, eventEntityAdapter } from '../events.state';

export const loadEventAction = createHttpAction<{ eventId: string; reload?: boolean }, Event>()(
  EVENTS_ACTION_SCOPE,
  'Load Event'
);

export const loadEventReducers: Reducers<EventsFeatureState> = [
  on(loadEventAction.success, (state, { response }) =>
    eventEntityAdapter.upsertOne(response, state)
  ),
  handleHttpAction('loadOne', loadEventAction, {
    startCondition: (s, p) => !s.entities[p.eventId] || p.reload === true,
  }),
];

export const loadEventEffects: Effects = {
  loadEvent$: createFunctionalEffect.dispatching((api = inject(EventAdministrationService)) =>
    onHttpAction(loadEventAction, selectEventsActionState('loadOne')).pipe(
      switchMap(({ props }) => toHttpAction(getEvent(api, props), loadEventAction, props))
    )
  ),
};

async function getEvent(
  api: EventAdministrationService,
  props: ReturnType<typeof loadEventAction>['props']
) {
  const response = await api.getEvent({ eventId: props.eventId });
  return response.ok
    ? loadEventAction.success(props, parseEvent(assertBody(response).event))
    : loadEventAction.error(props, response);
}
