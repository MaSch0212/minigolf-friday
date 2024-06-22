import { createAction, on, props } from '@ngrx/store';

import { Reducers } from '../../utils';
import { PLAYER_EVENTS_ACTION_SCOPE } from '../consts';
import { playerEventEntityAdapter, PlayerEventsFeatureState } from '../player-events.state';

export const playerEventRemovedAction = createAction(
  `[${PLAYER_EVENTS_ACTION_SCOPE}] Player Event Removed`,
  props<{ eventId: string }>()
);

export const playerEventRemovedReducers: Reducers<PlayerEventsFeatureState> = [
  on(playerEventRemovedAction, (state, { eventId }) =>
    playerEventEntityAdapter.removeOne(eventId, state)
  ),
];
