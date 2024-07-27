import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { produce, castDraft } from 'immer';
import { switchMap } from 'rxjs';

import { ApiEventTimeslotInstances } from '../../../api/models';
import { EventAdministrationService } from '../../../api/services';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { EVENTS_ACTION_SCOPE } from '../consts';
import { eventEntityAdapter, EventsFeatureState } from '../events.state';

export const setEventInstancesAction = createHttpAction<{
  eventId: string;
  instances: ApiEventTimeslotInstances[];
}>()(EVENTS_ACTION_SCOPE, 'Set Event Instances');

export const setEventInstancesReducers: Reducers<EventsFeatureState> = [
  on(setEventInstancesAction.success, (state, { props }) =>
    eventEntityAdapter.mapOne(
      {
        id: props.eventId,
        map: produce(draft => {
          for (const timeslot of draft.timeslots) {
            timeslot.instances = castDraft(
              props.instances.find(x => x.timeslotId === timeslot.id)?.instances || []
            );
          }
        }),
      },
      state
    )
  ),
  handleHttpAction('setInstances', setEventInstancesAction),
];

export const setEventInstancesEffects: Effects = {
  setEventInstances$: createFunctionalEffect.dispatching(
    (api = inject(EventAdministrationService)) =>
      onHttpAction(setEventInstancesAction).pipe(
        switchMap(({ props }) =>
          toHttpAction(setEventInstances(api, props), setEventInstancesAction, props)
        )
      )
  ),
};

async function setEventInstances(
  api: EventAdministrationService,
  props: ReturnType<typeof setEventInstancesAction>['props']
) {
  const response = await api.putEventInstances({
    eventId: props.eventId,
    body: { instances: props.instances },
  });
  return response.ok
    ? setEventInstancesAction.success(props, undefined)
    : setEventInstancesAction.error(props, response);
}
