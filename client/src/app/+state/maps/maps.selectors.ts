import { createDistinctSelector } from '@ngneers/easy-ngrx-distinct-selector';
import { createFeatureSelector } from '@ngrx/store';

import { mapsFeatureName } from './maps.feature';
import { MapsFeatureState, mapsEntityAdapter } from './maps.reducer';

export const mapsFeatureSelector = createFeatureSelector<MapsFeatureState>(mapsFeatureName);

export const mapSelectors = mapsEntityAdapter.getSelectors(mapsFeatureSelector);

export const selectMapsLoadState = createDistinctSelector(
  mapsFeatureSelector,
  state => state.loadState
);

export const selectMapActionState = createDistinctSelector(
  mapsFeatureSelector,
  state => state.actionState
);
