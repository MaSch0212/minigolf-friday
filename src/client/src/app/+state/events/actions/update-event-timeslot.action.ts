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

export const updateEventTimeslotAction = createHttpAction<{
  eventId: string;
  timeslotId: string;
  changes: {
    mapId?: string | null;
    isFallbackAllowed?: boolean;
  };
}>()(EVENTS_ACTION_SCOPE, 'Update Event Timeslot');

export const updateEventTimeslotReducers: Reducers<EventsFeatureState> = [
  on(updateEventTimeslotAction.success, (state, { props }) =>
    eventEntityAdapter.mapOne(
      {
        id: props.eventId,
        map: produce(draft => {
          const timeslot = draft.timeslots.find(x => x.id === props.timeslotId);
          if (timeslot) {
            if (props.changes.mapId !== undefined) {
              timeslot.mapId = props.changes.mapId;
            }
            if (props.changes.isFallbackAllowed !== undefined) {
              timeslot.isFallbackAllowed = props.changes.isFallbackAllowed;
            }
          }
        }),
      },
      state
    )
  ),
  handleHttpAction('updateTimeslot', updateEventTimeslotAction),
];

export const updateEventTimeslotEffects: Effects = {
  updateEventTimeslot$: createFunctionalEffect.dispatching(
    (api = inject(EventAdministrationService)) =>
      onHttpAction(updateEventTimeslotAction, selectEventsActionState('updateTimeslot')).pipe(
        switchMap(({ props }) =>
          toHttpAction(updateEventTimeslot(api, props), updateEventTimeslotAction, props)
        )
      )
  ),
};

async function updateEventTimeslot(
  api: EventAdministrationService,
  props: ReturnType<typeof updateEventTimeslotAction>['props']
) {
  const response = await api.updateEventTimeslot({
    timeslotId: props.timeslotId,
    body: props.changes,
  });
  return response.ok
    ? updateEventTimeslotAction.success(props, undefined)
    : updateEventTimeslotAction.error(props, response);
}
