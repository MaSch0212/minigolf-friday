import { createReducer } from '@ngrx/store';

import { loadUserSettingsReducers } from './actions/load-user-settings.action';
import { updateUserSettingsReducers } from './actions/update-user-settings.action';
import { UserSettingsFeatureState, initialUserSettingsFeatureState } from './users.state';

export const userSettingsReducer = createReducer<UserSettingsFeatureState>(
  initialUserSettingsFeatureState,

  ...loadUserSettingsReducers,
  ...updateUserSettingsReducers
);
