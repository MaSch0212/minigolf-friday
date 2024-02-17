import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { switchMap } from 'rxjs';

import { AddEventRequest, AddEventResponse } from '../../../models/api/event';
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

export const addEventAction = createHttpAction<AddEventRequest, AddEventResponse>()(
  EVENTS_ACTION_SCOPE,
  'Add'
);

export const addEventReducers: Reducers<EventsFeatureState> = [
  on(addEventAction.success, (state, { response }) =>
    eventEntityAdapter.upsertOne(response.event, state)
  ),
  handleHttpAction('add', addEventAction),
];

export const addEventEffects: Effects = {
  addEvent$: createFunctionalEffect.dispatching((api = inject(EventsService)) =>
    onHttpAction(addEventAction, selectEventsActionState('add')).pipe(
      switchMap(({ props }) => api.addEvent(props).pipe(mapToHttpAction(addEventAction, props)))
    )
  ),
};
