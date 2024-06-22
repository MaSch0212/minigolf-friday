import { createAction, on, props } from '@ngrx/store';
import { produce } from 'immer';

import { initialActionState } from '../../action-state';
import { Reducers } from '../../utils';
import { USERS_ACTION_SCOPE } from '../consts';
import { UsersFeatureState } from '../users.state';

export const resetUsersActionStateAction = createAction(
  `[${USERS_ACTION_SCOPE}] Reset Action State`,
  props<{ scope: keyof UsersFeatureState['actionStates'] }>()
);

export const resetUsersActionStateReducers: Reducers<UsersFeatureState> = [
  on(
    resetUsersActionStateAction,
    produce((state, { scope }) => {
      state.actionStates[scope] = initialActionState;
    })
  ),
];
