import { createReducer } from '@ngrx/store';

import { addUserReducers } from './actions/add-user.action';
import { loadUserLoginTokenReducers } from './actions/load-user-login-token.action';
import { loadUserReducers } from './actions/load-user.action';
import { loadUsersReducers } from './actions/load-users.action';
import { removeUserReducers } from './actions/remove-user.action';
import { resetUsersActionStateReducers } from './actions/reset-users-action-state.action';
import { updateUserReducers } from './actions/update-user.action';
import { UsersFeatureState, initialUsersFeatureState } from './users.state';

export const usersReducer = createReducer<UsersFeatureState>(
  initialUsersFeatureState,

  ...addUserReducers,
  ...loadUserLoginTokenReducers,
  ...loadUserReducers,
  ...loadUsersReducers,
  ...removeUserReducers,
  ...resetUsersActionStateReducers,
  ...updateUserReducers
);
