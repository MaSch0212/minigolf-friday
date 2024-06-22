import { createDistinctSelector } from '@ngneers/easy-ngrx-distinct-selector';
import { createFeatureSelector } from '@ngrx/store';

import { USER_SETTINGS_FEATURE_NAME } from './consts';
import { UserSettingsFeatureState } from './user-settings.state';

export const selectUserSettingsFeature = createFeatureSelector<UserSettingsFeatureState>(
  USER_SETTINGS_FEATURE_NAME
);

export const selectUserSettingsActionStates = createDistinctSelector(
  selectUserSettingsFeature,
  state => state.actionStates
);
export function selectUserSettingsActionState(
  action: keyof UserSettingsFeatureState['actionStates']
) {
  return createDistinctSelector(selectUserSettingsActionStates, state => state[action]);
}

export const selectUserSettings = createDistinctSelector(
  selectUserSettingsFeature,
  state => state.settings
);
