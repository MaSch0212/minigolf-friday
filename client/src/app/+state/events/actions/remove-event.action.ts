import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { switchMap } from 'rxjs';

import { EventAdministrationService } from '../../../api/services';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
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
  removeEvent$: createFunctionalEffect.dispatching((api = inject(EventAdministrationService)) =>
    onHttpAction(removeEventAction, selectEventsActionState('remove')).pipe(
      switchMap(({ props }) => toHttpAction(deleteEvent(api, props), removeEventAction, props))
    )
  ),
};

async function deleteEvent(
  api: EventAdministrationService,
  props: ReturnType<typeof removeEventAction>['props']
) {
  const response = await api.deleteEvent({ eventId: props.eventId });
  return response.ok
    ? removeEventAction.success(props, undefined)
    : removeEventAction.error(props, response);
}
