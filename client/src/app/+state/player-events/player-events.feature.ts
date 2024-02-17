import { provideEffects } from '@ngrx/effects';
import { createFeature, provideState } from '@ngrx/store';

import { PLAYER_EVENTS_FEATURE_NAME } from './consts';
import { PlayerEventsFeatureEffects } from './player-events.effects';
import { playerEventsReducer } from './player-events.reducer';

export const playerEventsFeature = createFeature({
  name: PLAYER_EVENTS_FEATURE_NAME,
  reducer: playerEventsReducer,
});

export function providePlayerEventsState() {
  return [provideState(playerEventsFeature), provideEffects(PlayerEventsFeatureEffects)];
}
