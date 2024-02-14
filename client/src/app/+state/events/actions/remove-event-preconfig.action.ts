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
  removeEventPreconfig$: createFunctionalEffect.dispatching((api = inject(EventsService)) =>
    onHttpAction(removeEventPreconfigAction, selectEventsActionState('removePreconfig')).pipe(
      switchMap(({ props }) =>
        api
          .removePreconfig(props.preconfigId)
          .pipe(mapToHttpAction(removeEventPreconfigAction, props))
      )
    )
  ),
};
