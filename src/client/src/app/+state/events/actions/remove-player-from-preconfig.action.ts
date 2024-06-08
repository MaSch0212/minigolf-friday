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
  removePlayerFromPreconfig$: createFunctionalEffect.dispatching(
    (api = inject(EventAdministrationService)) =>
      onHttpAction(
        removePlayerFromPreconfigAction,
        selectEventsActionState('removePlayerFromPreconfig')
      ).pipe(
        switchMap(({ props }) =>
          toHttpAction(
            removePlayerFromPreconfiguration(api, props),
            removePlayerFromPreconfigAction,
            props
          )
        )
      )
  ),
};

async function removePlayerFromPreconfiguration(
  api: EventAdministrationService,
  props: ReturnType<typeof removePlayerFromPreconfigAction>['props']
) {
  const response = await api.removePlayersFromPreconfiguration({
    preconfigurationId: props.preconfigId,
    body: { playerIds: [props.playerId] },
  });
  return response.ok
    ? removePlayerFromPreconfigAction.success(props, undefined)
    : removePlayerFromPreconfigAction.error(props, response);
}
