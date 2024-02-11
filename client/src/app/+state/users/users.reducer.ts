import { createReducer } from '@ngrx/store';

import { loadUsersByIdReducers } from './actions/load-users-by-id.action';
import { loadUsersReducers } from './actions/load-users.action';
import { UsersFeatureState, initialUsersFeatureState } from './users.state';

export const usersReducer = createReducer<UsersFeatureState>(
  initialUsersFeatureState,

  ...loadUsersByIdReducers,
  ...loadUsersReducers
);
