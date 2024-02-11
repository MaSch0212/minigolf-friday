import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { switchMap } from 'rxjs';

import { GetEventResponse } from '../../../models/api/event';
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

export const loadEventAction = createHttpAction<
  { eventId: string; reload?: boolean },
  GetEventResponse
>()(EVENTS_ACTION_SCOPE, 'Load Event');

export const loadEventReducers: Reducers<EventsFeatureState> = [
  on(loadEventAction.success, (state, { response }) =>
    eventEntityAdapter.addOne(response.event, state)
  ),
  handleHttpAction(
    'loadOne',
    loadEventAction,
    (s, p) => !s.entities[p.eventId] || p.reload === true
  ),
];

export const loadEventEffects: Effects = {
  loadEvent$: createFunctionalEffect.dispatching((api = inject(EventsService)) =>
    onHttpAction(loadEventAction, selectEventsActionState('loadOne')).pipe(
      switchMap(({ props }) =>
        api.getEvent(props.eventId).pipe(mapToHttpAction(loadEventAction, props))
      )
    )
  ),
};
