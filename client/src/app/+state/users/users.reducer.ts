import { createReducer } from '@ngrx/store';

import { loadUsersReducers } from './actions/load-users.action';
import { UsersFeatureState, initialUsersFeatureState } from './users.state';

export const usersReducer = createReducer<UsersFeatureState>(
  initialUsersFeatureState,

  ...loadUsersReducers
);
