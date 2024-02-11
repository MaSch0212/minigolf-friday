import { provideEffects } from '@ngrx/effects';
import { createFeature, provideState } from '@ngrx/store';

import { USERS_FEATURE_NAME } from './consts';
import { usersFeatureEffects } from './users.effects';
import { usersReducer } from './users.reducer';

export const usersFeature = createFeature({
  name: USERS_FEATURE_NAME,
  reducer: usersReducer,
});

export function provideUsersState() {
  return [provideState(usersFeature), provideEffects(usersFeatureEffects)];
}
