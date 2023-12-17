import { createDistinctSelector } from '@ngneers/easy-ngrx-distinct-selector';
import { createFeatureSelector } from '@ngrx/store';

import { playersFeatureName } from './players.feature';
import { PlayersFeatureState as State, playersEntityAdapter } from './players.reducer';

export const playersFeatureSelector = createFeatureSelector<State>(playersFeatureName);

export const playerSelectors = playersEntityAdapter.getSelectors(playersFeatureSelector);

export const selectPlayersLoadState = createDistinctSelector(
  playersFeatureSelector,
  state => state.loadState
);

export const selectPlayerActionState = createDistinctSelector(
  playersFeatureSelector,
  state => state.actionState
);
