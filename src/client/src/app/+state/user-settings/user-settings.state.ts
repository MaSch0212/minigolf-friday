import { UserSettings } from '../../models/parsed-models';
import { ActionState, initialActionState } from '../action-state';

export type UserSettingsFeatureState = {
  settings: UserSettings | undefined;
  actionStates: {
    load: ActionState;
    update: ActionState;
  };
};

export const initialUserSettingsFeatureState: UserSettingsFeatureState = {
  settings: undefined,
  actionStates: {
    load: initialActionState,
    update: initialActionState,
  },
};
