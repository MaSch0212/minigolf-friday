import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { switchMap } from 'rxjs';

import { EventAdministrationService } from '../../../api/services';
import { Event, parseEvent } from '../../../models/parsed-models';
import { toDateOnlyString } from '../../../utils/date.utils';
import { assertBody } from '../../../utils/http.utils';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { EVENTS_ACTION_SCOPE } from '../consts';
import { selectEventsActionState } from '../events.selectors';
import { EventsFeatureState, eventEntityAdapter } from '../events.state';

export const addEventAction = createHttpAction<
  {
    date: Date;
    registrationDeadline: Date;
  },
  Event
>()(EVENTS_ACTION_SCOPE, 'Add');

export const addEventReducers: Reducers<EventsFeatureState> = [
  on(addEventAction.success, (state, { response }) =>
    eventEntityAdapter.upsertOne(response, state)
  ),
  handleHttpAction('add', addEventAction),
];

export const addEventEffects: Effects = {
  addEvent$: createFunctionalEffect.dispatching((api = inject(EventAdministrationService)) =>
    onHttpAction(addEventAction, selectEventsActionState('add')).pipe(
      switchMap(({ props }) => toHttpAction(createEvent(api, props), addEventAction, props))
    )
  ),
};

async function createEvent(
  api: EventAdministrationService,
  props: ReturnType<typeof addEventAction>['props']
) {
  const response = await api.createEvent({
    body: {
      date: toDateOnlyString(props.date),
      registrationDeadline: props.registrationDeadline.toISOString(),
    },
  });
  return response.ok
    ? addEventAction.success(props, parseEvent(assertBody(response).event))
    : addEventAction.error(props, response);
}
