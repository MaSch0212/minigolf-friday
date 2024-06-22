import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { mergeMap } from 'rxjs';

import { UserAdministrationService } from '../../../api/services';
import { User } from '../../../models/parsed-models';
import { assertBody } from '../../../utils/http.utils';
import { createHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { USERS_ACTION_SCOPE } from '../consts';
import { userEntityAdapter, UsersFeatureState } from '../users.state';

export const loadUserAction = createHttpAction<{ userId: string }, User>()(
  USERS_ACTION_SCOPE,
  'Load User'
);

export const loadUserReducers: Reducers<UsersFeatureState> = [
  on(loadUserAction.success, (state, { response }) => userEntityAdapter.upsertOne(response, state)),
];

export const loadUserEffects: Effects = {
  loadUser$: createFunctionalEffect.dispatching((api = inject(UserAdministrationService)) =>
    onHttpAction(loadUserAction).pipe(
      mergeMap(({ props }) => toHttpAction(getUser(api, props), loadUserAction, props))
    )
  ),
};

async function getUser(
  api: UserAdministrationService,
  props: ReturnType<typeof loadUserAction>['props']
) {
  const response = await api.getUser({ userId: props.userId });
  return response.ok
    ? loadUserAction.success(props, assertBody(response).user)
    : loadUserAction.error(props, response);
}
