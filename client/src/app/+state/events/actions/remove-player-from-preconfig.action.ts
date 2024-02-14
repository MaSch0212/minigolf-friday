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

export const removePlayerFromPreconfigAction = createHttpAction<{
  eventId: string;
  timeslotId: string;
  preconfigId: string;
  playerId: string;
}>()(EVENTS_ACTION_SCOPE, 'Remove Player From Preconfig');

export const removePlayerFromPreconfigReducers: Reducers<EventsFeatureState> = [
  on(removePlayerFromPreconfigAction.success, (state, { props }) =>
    eventEntityAdapter.mapOne(
      {
        id: props.eventId,
        map: produce(draft => {
          const preconfig = draft.timeslots
            .find(t => t.id === props.timeslotId)
            ?.preconfigurations.find(p => p.id === props.preconfigId);
          if (preconfig) {
            preconfig.playerIds = preconfig.playerIds.filter(p => p !== props.playerId);
          }
        }),
      },
      state
    )
  ),
  handleHttpAction('removePlayerFromPreconfig', removePlayerFromPreconfigAction),
];

export const removePlayerFromPreconfigEffects: Effects = {
  removePlayerFromPreconfig$: createFunctionalEffect.dispatching((api = inject(EventsService)) =>
    onHttpAction(
      removePlayerFromPreconfigAction,
      selectEventsActionState('removePlayerFromPreconfig')
    ).pipe(
      switchMap(({ props }) =>
        api
          .removePlayerFromPreconfig(props.preconfigId, props.playerId)
          .pipe(mapToHttpAction(removePlayerFromPreconfigAction, props))
      )
    )
  ),
};
