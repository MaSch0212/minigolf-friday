import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { produce } from 'immer';
import { switchMap } from 'rxjs';

import { EventAdministrationService } from '../../../api/services';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { EVENTS_ACTION_SCOPE } from '../consts';
import { selectEventsActionState } from '../events.selectors';
import { EventsFeatureState, eventEntityAdapter } from '../events.state';

export const startEventAction = createHttpAction<{ eventId: string }>()(
  EVENTS_ACTION_SCOPE,
  'Start Event'
);

export const startEventReducers: Reducers<EventsFeatureState> = [
  on(startEventAction.success, (state, { props }) =>
    eventEntityAdapter.mapOne(
      {
        id: props.eventId,
        map: produce(draft => {
          draft.startedAt = new Date();
        }),
      },
      state
    )
  ),
  handleHttpAction('start', startEventAction),
];

export const startEventEffects: Effects = {
  startEvent$: createFunctionalEffect.dispatching((api = inject(EventAdministrationService)) =>
    onHttpAction(startEventAction, selectEventsActionState('start')).pipe(
      switchMap(({ props }) => toHttpAction(startEvent(api, props), startEventAction, props))
    )
  ),
};

async function startEvent(
  api: EventAdministrationService,
  props: ReturnType<typeof startEventAction>['props']
) {
  const response = await api.startEvent({ eventId: props.eventId });
  return response.ok
    ? startEventAction.success(props, undefined)
    : startEventAction.error(props, response);
}
