import { provideEffects } from '@ngrx/effects';
import { createFeature, provideState } from '@ngrx/store';

import { MapsFeatureEffects } from './maps.effects';
import { mapsReducer } from './maps.reducer';

export const mapsFeatureName = 'maps';

export const mapsFeature = createFeature({
  name: mapsFeatureName,
  reducer: mapsReducer,
});

export function provideMapsState() {
  return [provideState(mapsFeature), provideEffects(MapsFeatureEffects)];
}
