import { provideEffects } from '@ngrx/effects';
import { createFeature, provideState } from '@ngrx/store';

import { MAPS_FEATURE_NAME } from './consts';
import { mapsFeatureEffects } from './maps.effects';
import { mapsReducer } from './maps.reducer';

export const mapsFeature = createFeature({
  name: MAPS_FEATURE_NAME,
  reducer: mapsReducer,
});

export function provideMapsState() {
  return [provideState(mapsFeature), provideEffects(mapsFeatureEffects)];
}
