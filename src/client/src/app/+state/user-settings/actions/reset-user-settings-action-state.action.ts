import { createAction, on, props } from '@ngrx/store';
import { produce } from 'immer';

import { initialActionState } from '../../action-state';
import { Reducers } from '../../utils';
import { USER_SETTINGS_ACTION_SCOPE } from '../consts';
import { UserSettingsFeatureState } from '../user-settings.state';

export const resetUserSettingsActionStateAction = createAction(
  `[${USER_SETTINGS_ACTION_SCOPE}] Reset Action State`,
  props<{ scope: keyof UserSettingsFeatureState['actionStates'] }>()
);

export const resetUserSettingsActionStateReducers: Reducers<UserSettingsFeatureState> = [
  on(
    resetUserSettingsActionStateAction,
    produce((state, { scope }) => {
      state.actionStates[scope] = initialActionState;
    })
  ),
];
