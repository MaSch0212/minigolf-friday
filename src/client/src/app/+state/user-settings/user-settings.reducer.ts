import { createReducer } from '@ngrx/store';

import { loadUserSettingsReducers } from './actions/load-user-settings.action';
import { resetUserSettingsReducers } from './actions/reset-user-settings.action';
import { updateUserSettingsReducers } from './actions/update-user-settings.action';
import { UserSettingsFeatureState, initialUserSettingsFeatureState } from './user-settings.state';

export const userSettingsReducer = createReducer<UserSettingsFeatureState>(
  initialUserSettingsFeatureState,

  ...loadUserSettingsReducers,
  ...resetUserSettingsReducers,
  ...updateUserSettingsReducers
);
