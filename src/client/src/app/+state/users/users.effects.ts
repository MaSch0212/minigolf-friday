import { inject } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { Store } from '@ngrx/store';
import { EMPTY, filter, map, mergeMap, of, skip, withLatestFrom } from 'rxjs';

import { addUserEffects } from './actions/add-user.action';
import { loadUserLoginTokenEffects } from './actions/load-user-login-token.action';
import { loadUserAction, loadUserEffects } from './actions/load-user.action';
import { loadUsersEffects } from './actions/load-users.action';
import { removeUserAction, removeUserEffects } from './actions/remove-user.action';
import { updateUserEffects } from './actions/update-user.action';
import { resetUsersAction } from './users.actions';
import { userSelectors } from './users.selectors';
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
    userUpdated$: createFunctionalEffect.dispatching((store = inject(Store)) =>
      inject(RealtimeEventsService).userChanged.pipe(
        withLatestFrom(store.select(userSelectors.selectEntities)),
        mergeMap(([{ userId, changeType }, entities]) => {
          if (changeType === 'updated') {
            return userId in entities ? of(loadUserAction({ userId })) : EMPTY;
          } else if (changeType === 'created') {
            return of(loadUserAction({ userId }));
          } else if (changeType === 'deleted') {
            return of(removeUserAction.success({ userId }, undefined));
          }
          return EMPTY;
        })
      )
    ),

    onServerReconnect$: createFunctionalEffect.dispatching(() =>
      toObservable(inject(RealtimeEventsService).isConnected).pipe(
        skip(1),
        filter(x => x),
        map(() => resetUsersAction())
      )
    ),
  },
];
