import { createAction, on } from '@ngrx/store';
import { produce } from 'immer';

import { initialActionState } from '../../action-state';
import { Reducers } from '../../utils';
import { USER_SETTINGS_ACTION_SCOPE } from '../consts';
import { UserSettingsFeatureState } from '../user-settings.state';

export const resetUserSettingsAction = createAction(`[${USER_SETTINGS_ACTION_SCOPE}] Reset`);

export const resetUserSettingsReducers: Reducers<UserSettingsFeatureState> = [
  on(
    resetUserSettingsAction,
    produce(state => {
      state.settings = undefined;
      state.actionStates.load = initialActionState;
    })
  ),
];
