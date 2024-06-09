import { createReducer } from '@ngrx/store';

import { loadPlayerEventReducers } from './actions/load-player-event.action';
import { loadPlayerEventsReducers } from './actions/load-player-events.action';
import { updateEventRegistrationReducers } from './actions/update-event-registration.action';
import { PlayerEventsFeatureState, initialPlayerEventsFeatureState } from './player-events.state';

export const playerEventsReducer = createReducer<PlayerEventsFeatureState>(
  initialPlayerEventsFeatureState,

  ...loadPlayerEventsReducers,
  ...loadPlayerEventReducers,
  ...updateEventRegistrationReducers
);
