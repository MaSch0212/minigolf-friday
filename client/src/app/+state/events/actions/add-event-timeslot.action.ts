import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { Draft, produce } from 'immer';
import { switchMap } from 'rxjs';

import { AddTimeSlotRequest, AddTimeSlotResponse } from '../../../models/api/event';
import { MinigolfEventTimeslot } from '../../../models/event';
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

export const addEventTimeslotAction = createHttpAction<
  AddTimeSlotRequest & { eventId: string },
  AddTimeSlotResponse
>()(EVENTS_ACTION_SCOPE, 'Add Timeslot');

export const addEventTimeslotReducers: Reducers<EventsFeatureState> = [
  on(addEventTimeslotAction.success, (state, { props, response }) => {
    const entity = state.entities[props.eventId];
    if (!entity) return state;
    return eventEntityAdapter.upsertOne(
      produce(entity, draft => {
        draft.timeslots.push(response.timeslot as Draft<MinigolfEventTimeslot>);
      }),
      state
    );
  }),
  handleHttpAction('addTimeslot', addEventTimeslotAction, (s, p) => !!s.entities[p.eventId]),
];

export const addEventTimeslotEffects: Effects = {
  addEventTimeslot$: createFunctionalEffect.dispatching((api = inject(EventsService)) =>
    onHttpAction(addEventTimeslotAction, selectEventsActionState('addTimeslot')).pipe(
      switchMap(({ props }) =>
        api.addTimeSlot(props.eventId, props).pipe(mapToHttpAction(addEventTimeslotAction, props))
      )
    )
  ),
};
