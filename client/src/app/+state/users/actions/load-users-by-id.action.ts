import { inject } from '@angular/core';
import { Store, on } from '@ngrx/store';
import { switchMap, withLatestFrom } from 'rxjs';

import { GetUsersResponse } from '../../../models/api/user';
import { UsersService } from '../../../services/users.service';
import { distinct } from '../../../utils/array.utils';
import {
  createHttpAction,
  handleHttpAction,
  mapToHttpAction,
  onHttpAction,
} from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { USERS_ACTION_SCOPE } from '../consts';
import { selectUsersActionState, userSelectors } from '../users.selectors';
import { UsersFeatureState, userEntityAdapter } from '../users.state';

export const loadUsersByIdAction = createHttpAction<
  { userIds: string[]; reload?: boolean },
  GetUsersResponse
>()(USERS_ACTION_SCOPE, 'Load User');

export const loadUsersByIdReducers: Reducers<UsersFeatureState> = [
  on(loadUsersByIdAction.success, (state, { response }) =>
    userEntityAdapter.addMany(response.users, state)
  ),
  handleHttpAction(
    'loadByIds',
    loadUsersByIdAction,
    (s, p) => p.userIds.some(x => !s.ids.includes(x as never)) || p.reload === true
  ),
];

export const loadUsersByIdEffects: Effects = {
  loadUser$: createFunctionalEffect.dispatching(
    (store = inject(Store), api = inject(UsersService)) =>
      onHttpAction(loadUsersByIdAction, selectUsersActionState('loadByIds')).pipe(
        withLatestFrom(store.select(userSelectors.selectEntities)),
        switchMap(([{ props }, entities]) =>
          api
            .getUsersByIds({ userIds: distinct(props.userIds).filter(x => !entities[x]) })
            .pipe(mapToHttpAction(loadUsersByIdAction, props))
        )
      )
  ),
};
