import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { produce, castDraft } from 'immer';
import { switchMap } from 'rxjs';

import { EventAdministrationService } from '../../../api/services';
import { EventInstance } from '../../../models/parsed-models';
import { assertBody } from '../../../utils/http.utils';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { EVENTS_ACTION_SCOPE } from '../consts';
import { selectEventsActionState } from '../events.selectors';
import { EventsFeatureState, eventEntityAdapter } from '../events.state';

export const buildEventInstancesAction = createHttpAction<
  {
    eventId: string;
  },
  Record<string, EventInstance[]>
>()(EVENTS_ACTION_SCOPE, 'Build Event Instances');

export const buildEventInstancesReducers: Reducers<EventsFeatureState> = [
  on(buildEventInstancesAction.success, (state, { props, response }) =>
    eventEntityAdapter.mapOne(
      {
        id: props.eventId,
        map: produce(draft => {
          for (const timeslot of draft.timeslots) {
            timeslot.instances = castDraft(response[timeslot.id] || []);
          }
        }),
      },
      state
    )
  ),
  handleHttpAction('buildInstances', buildEventInstancesAction),
];

export const buildEventInstancesEffects: Effects = {
  buildEventInstances$: createFunctionalEffect.dispatching(
    (api = inject(EventAdministrationService)) =>
      onHttpAction(buildEventInstancesAction, selectEventsActionState('buildInstances')).pipe(
        switchMap(({ props }) =>
          toHttpAction(buildEventInstances(api, props), buildEventInstancesAction, props)
        )
      )
  ),
};

async function buildEventInstances(
  api: EventAdministrationService,
  props: ReturnType<typeof buildEventInstancesAction>['props']
) {
  const response = await api.buildEventInstances({ eventId: props.eventId });
  return response.ok
    ? buildEventInstancesAction.success(
        props,
        Object.fromEntries(assertBody(response).instances.map(x => [x.timeslotId, x.instances]))
      )
    : buildEventInstancesAction.error(props, response);
}
