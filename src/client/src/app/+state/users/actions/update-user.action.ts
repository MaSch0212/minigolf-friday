import { inject } from '@angular/core';
import { concatLatestFrom } from '@ngrx/operators';
import { on, Store } from '@ngrx/store';
import { filter, switchMap } from 'rxjs';

import { ApiUpdateUserRequest } from '../../../api/models';
import { UserAdministrationService } from '../../../api/services';
import { User } from '../../../models/parsed-models';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Reducers } from '../../utils';
import { USERS_ACTION_SCOPE } from '../consts';
import { selectUser, selectUsersActionState } from '../users.selectors';
import { userEntityAdapter, UsersFeatureState } from '../users.state';

export const updateUserAction = createHttpAction<User>()(USERS_ACTION_SCOPE, 'Update User');

export const updateUserReducers: Reducers<UsersFeatureState> = [
  on(updateUserAction.success, (state, { props }) => userEntityAdapter.upsertOne(props, state)),
  handleHttpAction('update', updateUserAction),
];

export const updateUserEffects = {
  updateUser$: createFunctionalEffect.dispatching(
    (store = inject(Store), api = inject(UserAdministrationService)) =>
      onHttpAction(updateUserAction, selectUsersActionState('update')).pipe(
        concatLatestFrom(({ props }) => store.select(selectUser(props.id))),
        filter(([, oldUser]) => !!oldUser),
        switchMap(([{ props }, oldUser]) =>
          toHttpAction(updateUser(api, props, oldUser!), updateUserAction, props)
        )
      )
  ),
};

async function updateUser(
  api: UserAdministrationService,
  props: ReturnType<typeof updateUserAction>['props'],
  oldUser: User
) {
  const request: ApiUpdateUserRequest = {
    alias: props.alias !== oldUser.alias ? props.alias : undefined,
    addRoles: props.roles.filter(role => !oldUser.roles.includes(role)),
    removeRoles: oldUser.roles.filter(role => !props.roles.includes(role)),
    playerPreferences: {
      addAvoid: props.playerPreferences.avoid.filter(
        avoid => !oldUser.playerPreferences.avoid.includes(avoid)
      ),
      removeAvoid: oldUser.playerPreferences.avoid.filter(
        avoid => !props.playerPreferences.avoid.includes(avoid)
      ),
      addPrefer: props.playerPreferences.prefer.filter(
        prefer => !oldUser.playerPreferences.prefer.includes(prefer)
      ),
      removePrefer: oldUser.playerPreferences.prefer.filter(
        prefer => !props.playerPreferences.prefer.includes(prefer)
      ),
    },
  };

  const response = await api.updateUser({
    userId: oldUser.id,
    body: request,
  });
  return response.ok
    ? updateUserAction.success(props, undefined)
    : updateUserAction.error(props, response);
}
