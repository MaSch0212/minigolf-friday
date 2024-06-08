import { createReducer } from '@ngrx/store';

import { loadPlayerEventReducers } from './actions/load-player-event.action';
import { loadPlayerEventsReducers } from './actions/load-player-events.action';
import { registerForEventReducers } from './actions/register-for-event.action';
import { PlayerEventsFeatureState, initialPlayerEventsFeatureState } from './player-events.state';

export const playerEventsReducer = createReducer<PlayerEventsFeatureState>(
  initialPlayerEventsFeatureState,

  ...loadPlayerEventsReducers,
  ...loadPlayerEventReducers,
  ...registerForEventReducers
);
