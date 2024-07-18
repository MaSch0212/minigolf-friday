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

export const updateEventAction = createHttpAction<{
  eventId: string;
  commit?: boolean;
  externalUri?: string;
}>()(EVENTS_ACTION_SCOPE, 'Update Event');

export const updateEventReducers: Reducers<EventsFeatureState> = [
  on(updateEventAction.success, (state, { props }) =>
    eventEntityAdapter.mapOne(
      {
        id: props.eventId,
        map: produce(draft => {
          draft.staged = props.commit ? props.commit : undefined;
          draft.externalUri = props.externalUri != undefined ? props.externalUri : undefined;
        }),
      },
      state
    )
  ),
  handleHttpAction('update', updateEventAction),
];

export const updateEventEffects: Effects = {
  updateEvent$: createFunctionalEffect.dispatching((api = inject(EventAdministrationService)) =>
    onHttpAction(updateEventAction, selectEventsActionState('update')).pipe(
      switchMap(({ props }) => toHttpAction(updateEvent(api, props), updateEventAction, props))
    )
  ),
};

async function updateEvent(
  api: EventAdministrationService,
  props: ReturnType<typeof updateEventAction>['props']
) {
  const response = await api.updateEvent({
    eventId: props.eventId,
    body: {
      commit: props.commit ? props.commit : undefined,
      externalUri: props.externalUri ?? undefined,
    },
  });
  return response.ok
    ? updateEventAction.success(props, undefined)
    : updateEventAction.error(props, response);
}
