import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { switchMap } from 'rxjs';

import { UserAdministrationService } from '../../../api/services';
import { User } from '../../../models/parsed-models';
import { assertBody } from '../../../utils/http.utils';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { USERS_ACTION_SCOPE } from '../consts';
import { selectUsersActionState } from '../users.selectors';
import { UsersFeatureState, userEntityAdapter } from '../users.state';

export const loadUsersAction = createHttpAction<{ reload?: boolean }, User[]>()(
  USERS_ACTION_SCOPE,
  'Load Users'
);

export const loadUsersReducers: Reducers<UsersFeatureState> = [
  on(loadUsersAction.success, (state, { response }) =>
    userEntityAdapter.upsertMany(response, state)
  ),
  handleHttpAction('load', loadUsersAction, {
    startCondition: (s, p) => s.actionStates.load.state === 'none' || p.reload === true,
  }),
];

export const loadUsersEffects: Effects = {
  loadUsers$: createFunctionalEffect.dispatching((api = inject(UserAdministrationService)) =>
    onHttpAction(loadUsersAction, selectUsersActionState('load')).pipe(
      switchMap(({ props }) => toHttpAction(getUsers(api, props), loadUsersAction, props))
    )
  ),
};

async function getUsers(
  api: UserAdministrationService,
  props: ReturnType<typeof loadUsersAction>['props']
) {
  const response = await api.getUsers();
  return response.ok
    ? loadUsersAction.success(props, assertBody(response).users)
    : loadUsersAction.error(props, response);
}
