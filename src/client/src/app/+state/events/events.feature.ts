import { provideEffects } from '@ngrx/effects';
import { createFeature, provideState } from '@ngrx/store';

import { EVENTS_FEATURE_NAME } from './consts';
import { eventsFeatureEffects } from './events.effects';
import { eventsReducer } from './events.reducer';

export const eventsFeature = createFeature({
  name: EVENTS_FEATURE_NAME,
  reducer: eventsReducer,
});

export function provideEventsState() {
  return [provideState(eventsFeature), provideEffects(eventsFeatureEffects)];
}
