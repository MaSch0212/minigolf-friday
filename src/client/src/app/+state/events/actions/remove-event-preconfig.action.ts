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

export const removeEventPreconfigAction = createHttpAction<{
  eventId: string;
  timeslotId: string;
  preconfigId: string;
}>()(EVENTS_ACTION_SCOPE, 'Delete Preconfiguration');

export const removeEventPreconfigReducers: Reducers<EventsFeatureState> = [
  on(removeEventPreconfigAction.success, (state, { props }) =>
    eventEntityAdapter.mapOne(
      {
        id: props.eventId,
        map: produce(draft => {
          const timeslot = draft.timeslots.find(t => t.id === props.timeslotId);
          if (timeslot) {
            timeslot.preconfigurations = timeslot.preconfigurations.filter(
              p => p.id !== props.preconfigId
            );
          }
        }),
      },
      state
    )
  ),
  handleHttpAction('removePreconfig', removeEventPreconfigAction),
];

export const removeEventPreconfigEffects: Effects = {
  removeEventPreconfig$: createFunctionalEffect.dispatching(
    (api = inject(EventAdministrationService)) =>
      onHttpAction(removeEventPreconfigAction, selectEventsActionState('removePreconfig')).pipe(
        switchMap(({ props }) =>
          toHttpAction(deletePreconfiguration(api, props), removeEventPreconfigAction, props)
        )
      )
  ),
};

async function deletePreconfiguration(
  api: EventAdministrationService,
  props: ReturnType<typeof removeEventPreconfigAction>['props']
) {
  const response = await api.deletePreconfiguration({ preconfigurationId: props.preconfigId });
  return response.ok
    ? removeEventPreconfigAction.success(props, undefined)
    : removeEventPreconfigAction.error(props, response);
}
