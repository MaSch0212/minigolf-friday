import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { produce } from 'immer';
import { switchMap } from 'rxjs';

import { UpdateTimeslotRequest } from '../../../models/api/event';
import { EventsService } from '../../../services/events.service';
import {
  createHttpAction,
  handleHttpAction,
  mapToHttpAction,
  onHttpAction,
} from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { EVENTS_ACTION_SCOPE } from '../consts';
import { selectEventsActionState } from '../events.selectors';
import { EventsFeatureState, eventEntityAdapter } from '../events.state';

export const updateEventTimeslotAction = createHttpAction<{
  eventId: string;
  timeslotId: string;
  changes: UpdateTimeslotRequest;
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
  updateEventTimeslot$: createFunctionalEffect.dispatching((api = inject(EventsService)) =>
    onHttpAction(updateEventTimeslotAction, selectEventsActionState('updateTimeslot')).pipe(
      switchMap(({ props }) =>
        api
          .updateTimeslot(props.timeslotId, props.changes)
          .pipe(mapToHttpAction(updateEventTimeslotAction, props))
      )
    )
  ),
};
