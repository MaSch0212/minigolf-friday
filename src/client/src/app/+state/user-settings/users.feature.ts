import { provideEffects } from '@ngrx/effects';
import { createFeature, provideState } from '@ngrx/store';

import { USER_SETTINGS_FEATURE_NAME } from './consts';
import { userSettingsFeatureEffects } from './users.effects';
import { userSettingsReducer } from './users.reducer';

export const userSettingsFeature = createFeature({
  name: USER_SETTINGS_FEATURE_NAME,
  reducer: userSettingsReducer,
});

export function provideUserSettingsState() {
  return [provideState(userSettingsFeature), provideEffects(userSettingsFeatureEffects)];
}
