import { provideEffects } from '@ngrx/effects';
import { createFeature, provideState } from '@ngrx/store';

import { PlayersFeatureEffects } from './players.effects';
import { PlayersFeatureState, playersReducer } from './players.reducer';

export const playersFeatureName = 'players';
export type PlayersFeatureSlice = {
  [playersFeatureName]: PlayersFeatureState;
};

export const playersFeature = createFeature({
  name: playersFeatureName,
  reducer: playersReducer,
});

export function providePlayersState() {
  return [provideState(playersFeature), provideEffects(PlayersFeatureEffects)];
}
