import { inject } from '@angular/core';
import { EMPTY, map, mergeMap, of } from 'rxjs';

import { addUserEffects } from './actions/add-user.action';
import { loadUserLoginTokenEffects } from './actions/load-user-login-token.action';
import { loadUserAction, loadUserEffects } from './actions/load-user.action';
import { loadUsersEffects } from './actions/load-users.action';
import { removeUserAction, removeUserEffects } from './actions/remove-user.action';
import { updateUserEffects } from './actions/update-user.action';
import { resetUsersActionStateAction } from './users.actions';
import { RealtimeEventsService } from '../../services/realtime-events.service';
import { createFunctionalEffect } from '../functional-effect';
import { Effects } from '../utils';

export const usersFeatureEffects: Effects[] = [
  addUserEffects,
  loadUserLoginTokenEffects,
  loadUserEffects,
  loadUsersEffects,
  removeUserEffects,
  updateUserEffects,
  {
    userUpdated$: createFunctionalEffect.dispatching(() =>
      inject(RealtimeEventsService).userChanged.pipe(
        mergeMap(({ userId, changeType }) => {
          if (changeType === 'updated' || changeType === 'created') {
            return of(loadUserAction({ userId }));
          } else if (changeType === 'deleted') {
            return of(removeUserAction.success({ userId }, undefined));
          }
          return EMPTY;
        })
      )
    ),

    onServerReconnected$: createFunctionalEffect.dispatching(() =>
      inject(RealtimeEventsService).onReconnected$.pipe(
        map(() => resetUsersActionStateAction({ scope: 'load' }))
      )
    ),
  },
];
