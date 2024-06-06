import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { produce } from 'immer';
import { switchMap } from 'rxjs';

import { UserAdministrationService } from '../../../api/services';
import { assertBody } from '../../../utils/http.utils';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { USERS_ACTION_SCOPE } from '../consts';
import { selectUsersActionState } from '../users.selectors';
import { userEntityAdapter, UsersFeatureState } from '../users.state';

export const loadUserLoginTokenAction = createHttpAction<
  { userId: string },
  { loginToken: string }
>()(USERS_ACTION_SCOPE, 'Load User Login Token');

export const loadUserLoginTokenReducers: Reducers<UsersFeatureState> = [
  on(loadUserLoginTokenAction.success, (state, { props, response }) =>
    userEntityAdapter.mapOne(
      {
        id: props.userId,
        map: produce(draft => {
          draft.loginToken = response.loginToken;
        }),
      },
      state
    )
  ),
  handleHttpAction('loadLoginToken', loadUserLoginTokenAction),
];

export const loadUserLoginTokenEffects: Effects = {
  loadUserLoginToken$: createFunctionalEffect.dispatching(
    (api = inject(UserAdministrationService)) =>
      onHttpAction(loadUserLoginTokenAction, selectUsersActionState('loadLoginToken')).pipe(
        switchMap(({ props }) =>
          toHttpAction(getUserLoginToken(api, props), loadUserLoginTokenAction, props)
        )
      )
  ),
};

async function getUserLoginToken(
  api: UserAdministrationService,
  props: ReturnType<typeof loadUserLoginTokenAction>['props']
) {
  const response = await api.getUserLoginToken({ userId: props.userId });
  return response.ok
    ? loadUserLoginTokenAction.success(props, assertBody(response))
    : loadUserLoginTokenAction.error(props, response);
}
