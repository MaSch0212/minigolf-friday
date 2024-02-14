import { inject } from '@angular/core';
import { on } from '@ngrx/store';
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

export const removeEventAction = createHttpAction<{ eventId: string }>()(
  EVENTS_ACTION_SCOPE,
  'Delete'
);

export const removeEventReducers: Reducers<EventsFeatureState> = [
  on(removeEventAction.success, (state, { props }) =>
    eventEntityAdapter.removeOne(props.eventId, state)
  ),
  handleHttpAction('remove', removeEventAction),
];

export const removeEventEffects: Effects = {
  removeEvent$: createFunctionalEffect.dispatching((api = inject(EventsService)) =>
    onHttpAction(removeEventAction, selectEventsActionState('remove')).pipe(
      switchMap(({ props }) =>
        api.removeEvent(props.eventId).pipe(mapToHttpAction(removeEventAction, props))
      )
    )
  ),
};
