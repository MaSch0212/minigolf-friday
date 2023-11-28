import { provideEffects } from '@ngrx/effects';
import { createFeature, provideState } from '@ngrx/store';

import { PlayersFeatureEffects } from './players.effects';
import { playersReducer } from './players.reducer';

export const playersFeatureName = 'players';

export const playersFeature = createFeature({
  name: playersFeatureName,
  reducer: playersReducer,
});

export function providePlayersState() {
  return [provideState(playersFeature), provideEffects(PlayersFeatureEffects)];
}
