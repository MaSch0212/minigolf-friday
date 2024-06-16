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

export const commitEventAction = createHttpAction<{ eventId: string; commit: boolean }>()(
  EVENTS_ACTION_SCOPE,
  'Commit Event'
);

export const commitEventReducers: Reducers<EventsFeatureState> = [
  on(commitEventAction.success, (state, { props }) =>
    eventEntityAdapter.mapOne(
      {
        id: props.eventId,
        map: produce(draft => {
          draft.staged = !props.commit;
        }),
      },
      state
    )
  ),
  handleHttpAction('commit', commitEventAction),
];

export const commitEventEffects: Effects = {
  commitEvent$: createFunctionalEffect.dispatching((api = inject(EventAdministrationService)) =>
    onHttpAction(commitEventAction, selectEventsActionState('commit')).pipe(
      switchMap(({ props }) => toHttpAction(commitEvent(api, props), commitEventAction, props))
    )
  ),
};

async function commitEvent(
  api: EventAdministrationService,
  props: ReturnType<typeof commitEventAction>['props']
) {
  const response = await api.updateEvent({
    eventId: props.eventId,
    body: { commit: true },
  });
  return response.ok
    ? commitEventAction.success(props, undefined)
    : commitEventAction.error(props, response);
}
