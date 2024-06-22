import { createAction, on } from '@ngrx/store';
import { produce } from 'immer';

import { initialActionState } from '../../action-state';
import { Reducers } from '../../utils';
import { PLAYER_EVENTS_ACTION_SCOPE } from '../consts';
import { playerEventEntityAdapter, PlayerEventsFeatureState } from '../player-events.state';

export const resetPlayerEventsAction = createAction(`[${PLAYER_EVENTS_ACTION_SCOPE}] Reset`);

export const resetPlayerEventsReducers: Reducers<PlayerEventsFeatureState> = [
  on(resetPlayerEventsAction, state =>
    playerEventEntityAdapter.removeAll(
      produce(state, draft => {
        draft.actionStates.load = initialActionState;
      })
    )
  ),
];
