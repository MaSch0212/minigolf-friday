import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { mergeMap } from 'rxjs';

import { EventsService } from '../../../api/services';
import { parsePlayerEvent, PlayerEvent } from '../../../models/parsed-models';
import { assertBody } from '../../../utils/http.utils';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { PLAYER_EVENTS_ACTION_SCOPE } from '../consts';
import { selectPlayerEventsActionState } from '../player-events.selectors';
import { PlayerEventsFeatureState, playerEventEntityAdapter } from '../player-events.state';

export const loadPlayerEventAction = createHttpAction<
  { eventId: string; reload?: boolean; silent?: boolean },
  PlayerEvent
>()(PLAYER_EVENTS_ACTION_SCOPE, 'Load Player Event');

export const loadPlayerEventReducers: Reducers<PlayerEventsFeatureState> = [
  on(loadPlayerEventAction.success, (state, { response }) =>
    playerEventEntityAdapter.upsertOne(response, state)
  ),
  handleHttpAction('loadOne', loadPlayerEventAction, {
    condition: (s, p) => p.silent !== true,
    startCondition: (s, p) => !s.entities[p.eventId] || p.reload === true,
  }),
];

export const loadPlayerEventEffects: Effects = {
  loadPlayerEvent$: createFunctionalEffect.dispatching((api = inject(EventsService)) =>
    onHttpAction(
      loadPlayerEventAction,
      selectPlayerEventsActionState('loadOne'),
      p => !!p.props.silent
    ).pipe(
      mergeMap(({ props }) =>
        toHttpAction(getPlayerEvent(api, props), loadPlayerEventAction, props)
      )
    )
  ),
};

async function getPlayerEvent(
  api: EventsService,
  props: ReturnType<typeof loadPlayerEventAction>['props']
) {
  const response = await api.getPlayerEvent({ eventId: props.eventId });
  return response.ok
    ? loadPlayerEventAction.success(props, parsePlayerEvent(assertBody(response).event))
    : loadPlayerEventAction.error(props, response);
}
