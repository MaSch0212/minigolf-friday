import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { switchMap } from 'rxjs';

import { GetPlayerEventResponse } from '../../../models/api/player-event';
import { PlayerEventsService } from '../../../services/player-events.service';
import {
  createHttpAction,
  handleHttpAction,
  mapToHttpAction,
  onHttpAction,
} from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { PLAYER_EVENTS_ACTION_SCOPE } from '../consts';
import { selectPlayerEventsActionState } from '../player-events.selectors';
import { PlayerEventsFeatureState, playerEventEntityAdapter } from '../player-events.state';

export const loadPlayerEventAction = createHttpAction<
  { eventId: string; reload?: boolean },
  GetPlayerEventResponse
>()(PLAYER_EVENTS_ACTION_SCOPE, 'Load Player Event');

export const loadPlayerEventReducers: Reducers<PlayerEventsFeatureState> = [
  on(loadPlayerEventAction.success, (state, { response }) =>
    playerEventEntityAdapter.upsertOne(response.event, state)
  ),
  handleHttpAction(
    'loadOne',
    loadPlayerEventAction,
    (s, p) => !s.entities[p.eventId] || p.reload === true
  ),
];

export const loadPlayerEventEffects: Effects = {
  loadPlayerEvent$: createFunctionalEffect.dispatching((api = inject(PlayerEventsService)) =>
    onHttpAction(loadPlayerEventAction, selectPlayerEventsActionState('loadOne')).pipe(
      switchMap(({ props }) =>
        api.getEvent(props.eventId).pipe(mapToHttpAction(loadPlayerEventAction, props))
      )
    )
  ),
};
