import { createAction, on } from '@ngrx/store';
import { produce } from 'immer';

import { initialActionState } from '../../action-state';
import { Reducers } from '../../utils';
import { USERS_ACTION_SCOPE } from '../consts';
import { userEntityAdapter, UsersFeatureState } from '../users.state';

export const resetUsersAction = createAction(`[${USERS_ACTION_SCOPE}] Reset`);

export const resetUsersReducers: Reducers<UsersFeatureState> = [
  on(resetUsersAction, state =>
    userEntityAdapter.removeAll(
      produce(state, draft => {
        draft.actionStates.load = initialActionState;
      })
    )
  ),
];
