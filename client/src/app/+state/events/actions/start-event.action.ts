import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { produce } from 'immer';
import { switchMap } from 'rxjs';

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
          draft.isStarted = true;
        }),
      },
      state
    )
  ),
  handleHttpAction('start', startEventAction),
];

export const startEventEffects: Effects = {
  startEvent$: createFunctionalEffect.dispatching((api = inject(EventsService)) =>
    onHttpAction(startEventAction, selectEventsActionState('start')).pipe(
      switchMap(({ props }) =>
        api.startEvent(props.eventId).pipe(mapToHttpAction(startEventAction, props))
      )
    )
  ),
};
