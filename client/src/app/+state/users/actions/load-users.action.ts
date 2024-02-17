import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { switchMap } from 'rxjs';

import { GetUsersResponse } from '../../../models/api/user';
import { UsersService } from '../../../services/users.service';
import {
  createHttpAction,
  handleHttpAction,
  mapToHttpAction,
  onHttpAction,
} from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { USERS_ACTION_SCOPE } from '../consts';
import { selectUsersActionState } from '../users.selectors';
import { UsersFeatureState, userEntityAdapter } from '../users.state';

export const loadUsersAction = createHttpAction<{ reload?: boolean }, GetUsersResponse>()(
  USERS_ACTION_SCOPE,
  'Load Users'
);

export const loadUsersReducers: Reducers<UsersFeatureState> = [
  on(loadUsersAction.success, (state, { response }) =>
    userEntityAdapter.upsertMany(response.users, state)
  ),
  handleHttpAction(
    'load',
    loadUsersAction,
    (s, p) => s.actionStates.load.state === 'none' || p.reload === true
  ),
];

export const loadUsersEffects: Effects = {
  loadUsers$: createFunctionalEffect.dispatching((api = inject(UsersService)) =>
    onHttpAction(loadUsersAction, selectUsersActionState('load')).pipe(
      switchMap(({ props }) => api.getUsers().pipe(mapToHttpAction(loadUsersAction, props)))
    )
  ),
};
