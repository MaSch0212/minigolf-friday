import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { switchMap } from 'rxjs';

import { ApiCreateUserResponse } from '../../../api/models';
import { UserAdministrationService } from '../../../api/services';
import { User } from '../../../models/parsed-models';
import { assertBody } from '../../../utils/http.utils';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { USERS_ACTION_SCOPE } from '../consts';
import { selectUsersActionState } from '../users.selectors';
import { userEntityAdapter, UsersFeatureState } from '../users.state';

export const addUserAction = createHttpAction<Omit<User, 'id' | 'loginToken'>, User>()(
  USERS_ACTION_SCOPE,
  'Add User'
);

export const addUserReducers: Reducers<UsersFeatureState> = [
  on(addUserAction.success, (state, { response }) => userEntityAdapter.upsertOne(response, state)),
  handleHttpAction('add', addUserAction),
];

export const addUserEffects: Effects = {
  addUser$: createFunctionalEffect.dispatching((api = inject(UserAdministrationService)) =>
    onHttpAction(addUserAction, selectUsersActionState('add')).pipe(
      switchMap(({ props }) => toHttpAction(createUser(api, props), addUserAction, props))
    )
  ),
};

async function createUser(
  api: UserAdministrationService,
  props: ReturnType<typeof addUserAction>['props']
) {
  const response = await api.createUser({
    body: {
      alias: props.alias,
      roles: props.roles,
      playerPreferences: props.playerPreferences,
    },
  });
  return response.ok
    ? addUserAction.success(props, toUser(assertBody(response)))
    : addUserAction.error(props, response);
}

function toUser(body: ApiCreateUserResponse): User {
  return {
    ...body.user,
    loginToken: body.loginToken,
  };
}
