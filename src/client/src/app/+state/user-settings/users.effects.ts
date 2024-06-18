import { loadUserSettingsEffects } from './actions/load-user-settings.action';
import { updateUserSettingsEffects } from './actions/update-user-settings.action';
import { Effects } from '../utils';

export const userSettingsFeatureEffects: Effects[] = [
  loadUserSettingsEffects,
  updateUserSettingsEffects,
];
