import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { produce } from 'immer';
import { switchMap } from 'rxjs';

import { EventAdministrationService } from '../../../api/services';
import { AuthService } from '../../../services/auth.service';
import { createHttpAction, handleHttpAction, onHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { EVENTS_ACTION_SCOPE } from '../consts';
import { eventEntityAdapter, EventsFeatureState } from '../events.state';

export const setEditingEventInstancesAction = createHttpAction<
  { eventId: string; isEditing: boolean },
  { userIdEditingInstances: string | null }
>()(EVENTS_ACTION_SCOPE, 'Set Editing Event Instances');

export const setEditingEventInstancesReducers: Reducers<EventsFeatureState> = [
  on(setEditingEventInstancesAction.success, (state, { props, response }) =>
    eventEntityAdapter.mapOne(
      {
        id: props.eventId,
        map: produce(draft => {
          draft.userIdEditingInstances = response.userIdEditingInstances;
        }),
      },
      state
    )
  ),
  handleHttpAction('setInstancesEditing', setEditingEventInstancesAction),
];

export const setEditingEventInstancesEffects: Effects = {
  setEditingEventInstances$: createFunctionalEffect.dispatching(
    (api = inject(EventAdministrationService), authService = inject(AuthService)) =>
      onHttpAction(setEditingEventInstancesAction).pipe(
        switchMap(({ props }) => setEditingEventInstances(api, props, authService))
      )
  ),
};

async function setEditingEventInstances(
  api: EventAdministrationService,
  props: ReturnType<typeof setEditingEventInstancesAction>['props'],
  authService: AuthService
) {
  const response = await api.setEventInstancesEditing({
    eventId: props.eventId,
    body: { isEditing: props.isEditing },
  });
  return response.ok
    ? setEditingEventInstancesAction.success(props, {
        userIdEditingInstances: props.isEditing ? (authService.user()?.id ?? null) : null,
      })
    : setEditingEventInstancesAction.error(props, response);
}
