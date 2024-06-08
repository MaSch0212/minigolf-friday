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

export const removeEventTimeslotAction = createHttpAction<{
  eventId: string;
  timeslotId: string;
}>()(EVENTS_ACTION_SCOPE, 'Remove Timeslot');

export const removeEventTimeslotReducers: Reducers<EventsFeatureState> = [
  on(removeEventTimeslotAction.success, (state, { props }) =>
    eventEntityAdapter.mapOne(
      {
        id: props.eventId,
        map: produce(draft => {
          draft.timeslots = draft.timeslots.filter(t => t.id !== props.timeslotId);
        }),
      },
      state
    )
  ),
  handleHttpAction('removeTimeslot', removeEventTimeslotAction),
];

export const removeEventTimeslotEffects: Effects = {
  removeEventTimeslot$: createFunctionalEffect.dispatching(
    (api = inject(EventAdministrationService)) =>
      onHttpAction(removeEventTimeslotAction, selectEventsActionState('removeTimeslot')).pipe(
        switchMap(({ props }) =>
          toHttpAction(deleteEventTimeslot(api, props), removeEventTimeslotAction, props)
        )
      )
  ),
};

async function deleteEventTimeslot(
  api: EventAdministrationService,
  props: ReturnType<typeof removeEventTimeslotAction>['props']
) {
  const response = await api.deleteEventTimeslot({ timeslotId: props.timeslotId });
  return response.ok
    ? removeEventTimeslotAction.success(props, undefined)
    : removeEventTimeslotAction.error(props, response);
}
