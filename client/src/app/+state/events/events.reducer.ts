import { createReducer } from '@ngrx/store';

import { addEventReducers } from './actions/add-event.action';
import { addPlayerToEventPreconfigurationReducers } from './actions/add-player-to-preconfig.action';
import { loadEventReducers } from './actions/load-event.action';
import { loadEventsReducers } from './actions/load-events.action';
import { EventsFeatureState, initialEventsFeatureState } from './events.state';

export const eventsReducer = createReducer<EventsFeatureState>(
  initialEventsFeatureState,

  ...addEventReducers,
  ...addPlayerToEventPreconfigurationReducers,
  ...loadEventReducers,
  ...loadEventsReducers
);
