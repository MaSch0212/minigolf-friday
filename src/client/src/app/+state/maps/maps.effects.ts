import { inject } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { Store } from '@ngrx/store';
import { withLatestFrom, mergeMap, of, EMPTY, skip, filter, map } from 'rxjs';

import { addMapEffects } from './actions/add-map.action';
import { loadMapAction, loadMapEffects } from './actions/load-map.action';
import { loadMapsEffects } from './actions/load-maps.action';
import { removeMapAction, removeMapEffects } from './actions/remove-map.action';
import { updateMapEffects } from './actions/update-map.action';
import { resetMapsAction } from './maps.actions';
import { mapSelectors } from './maps.selectors';
import { RealtimeEventsService } from '../../services/realtime-events.service';
import { createFunctionalEffect } from '../functional-effect';
import { Effects } from '../utils';

export const mapsFeatureEffects: Effects[] = [
  addMapEffects,
  loadMapEffects,
  loadMapsEffects,
  removeMapEffects,
  updateMapEffects,
  {
    mapUpdated$: createFunctionalEffect.dispatching((store = inject(Store)) =>
      inject(RealtimeEventsService).mapChanged.pipe(
        withLatestFrom(store.select(mapSelectors.selectEntities)),
        mergeMap(([{ mapId, changeType }, entities]) => {
          if (changeType === 'updated') {
            return mapId in entities ? of(loadMapAction({ mapId })) : EMPTY;
          } else if (changeType === 'created') {
            return of(loadMapAction({ mapId }));
          } else if (changeType === 'deleted') {
            return of(removeMapAction.success({ mapId }, undefined));
          }
          return EMPTY;
        })
      )
    ),

    onServerReconnect$: createFunctionalEffect.dispatching(() =>
      toObservable(inject(RealtimeEventsService).isConnected).pipe(
        skip(1),
        filter(x => x),
        map(() => resetMapsAction())
      )
    ),
  },
];
