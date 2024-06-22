import { inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { withLatestFrom, mergeMap, of, EMPTY, map } from 'rxjs';

import {
  loadUserSettingsAction,
  loadUserSettingsEffects,
} from './actions/load-user-settings.action';
import { updateUserSettingsEffects } from './actions/update-user-settings.action';
import { resetUserSettingsActionStateAction } from './user-settings.actions';
import { selectUserSettings } from './user-settings.selectors';
import { RealtimeEventsService } from '../../services/realtime-events.service';
import { createFunctionalEffect } from '../functional-effect';
import { Effects } from '../utils';

export const userSettingsFeatureEffects: Effects[] = [
  loadUserSettingsEffects,
  updateUserSettingsEffects,
  {
    userSettingsUpdated$: createFunctionalEffect.dispatching((store = inject(Store)) =>
      inject(RealtimeEventsService).userSettingsChanged.pipe(
        withLatestFrom(store.select(selectUserSettings)),
        mergeMap(([_, settings]) => {
          if (settings) {
            return of(loadUserSettingsAction({ reload: true, silent: true }));
          }
          return EMPTY;
        })
      )
    ),

    onServerReconnected$: createFunctionalEffect.dispatching(() =>
      inject(RealtimeEventsService).onReconnected$.pipe(
        map(() => resetUserSettingsActionStateAction({ scope: 'load' }))
      )
    ),
  },
];
