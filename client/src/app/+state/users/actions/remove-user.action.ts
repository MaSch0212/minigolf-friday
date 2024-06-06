import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { switchMap } from 'rxjs';

import { UserAdministrationService } from '../../../api/services';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { USERS_ACTION_SCOPE } from '../consts';
import { selectUsersActionState } from '../users.selectors';
import { userEntityAdapter, UsersFeatureState } from '../users.state';

export const removeUserAction = createHttpAction<{ userId: string }>()(
  USERS_ACTION_SCOPE,
  'Remove User'
);

export const removeUserReducers: Reducers<UsersFeatureState> = [
  on(removeUserAction.success, (state, { props }) =>
    userEntityAdapter.removeOne(props.userId, state)
  ),
  handleHttpAction('remove', removeUserAction),
];

export const removeUserEffects: Effects = {
  removeUser$: createFunctionalEffect.dispatching((api = inject(UserAdministrationService)) =>
    onHttpAction(removeUserAction, selectUsersActionState('remove')).pipe(
      switchMap(({ props }) => toHttpAction(deleteUser(api, props), removeUserAction, props))
    )
  ),
};

async function deleteUser(
  api: UserAdministrationService,
  props: ReturnType<typeof removeUserAction>['props']
) {
  const response = await api.deleteUser({ userId: props.userId });
  return response.ok
    ? removeUserAction.success(props, undefined)
    : removeUserAction.error(props, response);
}
