import { createAction, on, props } from '@ngrx/store';
import { produce } from 'immer';

import { initialActionState } from '../../action-state';
import { Reducers } from '../../utils';
import { PLAYER_EVENTS_ACTION_SCOPE } from '../consts';
import { PlayerEventsFeatureState } from '../player-events.state';

export const resetPlayerEventsActionStateAction = createAction(
  `[${PLAYER_EVENTS_ACTION_SCOPE}] Reset Action State`,
  props<{ scope: keyof PlayerEventsFeatureState['actionStates'] }>()
);

export const resetPlayerEventsActionStateReducers: Reducers<PlayerEventsFeatureState> = [
  on(
    resetPlayerEventsActionStateAction,
    produce((state, { scope }) => {
      state.actionStates[scope] = initialActionState;
    })
  ),
];
