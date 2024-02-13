import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { Draft, produce } from 'immer';
import { switchMap } from 'rxjs';

import { AddPreconfigResponse } from '../../../models/api/event';
import { MinigolfEventInstancePreconfiguration } from '../../../models/event';
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

export const addEventPreconfigAction = createHttpAction<
  { eventId: string; timeslotId: string },
  AddPreconfigResponse
>()(EVENTS_ACTION_SCOPE, 'Add Preconfig');

export const addEventPreconfigReducers: Reducers<EventsFeatureState> = [
  on(addEventPreconfigAction.success, (state, { props, response }) => {
    const entity = state.entities[props.eventId];
    if (!entity) return state;
    return eventEntityAdapter.upsertOne(
      produce(entity, draft => {
        const timeslot = draft.timeslots.find(t => t.id === props.timeslotId);
        if (!timeslot) return;
        timeslot.preconfigurations.push(
          response.preconfig as Draft<MinigolfEventInstancePreconfiguration>
        );
      }),
      state
    );
  }),
  handleHttpAction('addPreconfig', addEventPreconfigAction),
];

export const addEventPreconfigEffects: Effects = {
  addEventPreconfig$: createFunctionalEffect.dispatching((api = inject(EventsService)) =>
    onHttpAction(addEventPreconfigAction, selectEventsActionState('addPreconfig')).pipe(
      switchMap(({ props }) =>
        api.addPreconfig(props.timeslotId).pipe(mapToHttpAction(addEventPreconfigAction, props))
      )
    )
  ),
};
