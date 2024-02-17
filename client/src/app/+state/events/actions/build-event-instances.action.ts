import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { produce, castDraft } from 'immer';
import { switchMap } from 'rxjs';

import { BuildInstancesResponse } from '../../../models/api/event';
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

export const buildEventInstancesAction = createHttpAction<
  {
    eventId: string;
  },
  BuildInstancesResponse
>()(EVENTS_ACTION_SCOPE, 'Build Event Instances');

export const buildEventInstancesReducers: Reducers<EventsFeatureState> = [
  on(buildEventInstancesAction.success, (state, { props, response }) =>
    eventEntityAdapter.mapOne(
      {
        id: props.eventId,
        map: produce(draft => {
          for (const timeslot of draft.timeslots) {
            timeslot.instances = castDraft(response.instances[timeslot.id] || []);
          }
        }),
      },
      state
    )
  ),
  handleHttpAction('buildInstances', buildEventInstancesAction),
];

export const buildEventInstancesEffects: Effects = {
  buildEventInstances$: createFunctionalEffect.dispatching((api = inject(EventsService)) =>
    onHttpAction(buildEventInstancesAction, selectEventsActionState('buildInstances')).pipe(
      switchMap(({ props }) =>
        api.buildInstances(props.eventId).pipe(mapToHttpAction(buildEventInstancesAction, props))
      )
    )
  ),
};
