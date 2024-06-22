import { inject } from '@angular/core';
import { mergeMap, of, EMPTY, map } from 'rxjs';

import { addMapEffects } from './actions/add-map.action';
import { loadMapAction, loadMapEffects } from './actions/load-map.action';
import { loadMapsEffects } from './actions/load-maps.action';
import { removeMapAction, removeMapEffects } from './actions/remove-map.action';
import { updateMapEffects } from './actions/update-map.action';
import { resetMapActionStateAction } from './maps.actions';
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
    mapUpdated$: createFunctionalEffect.dispatching(() =>
      inject(RealtimeEventsService).mapChanged.pipe(
        mergeMap(({ mapId, changeType }) => {
          if (changeType === 'updated' || changeType === 'created') {
            return of(loadMapAction({ mapId }));
          } else if (changeType === 'deleted') {
            return of(removeMapAction.success({ mapId }, undefined));
          }
          return EMPTY;
        })
      )
    ),

    onServerReconnected$: createFunctionalEffect.dispatching(() =>
      inject(RealtimeEventsService).onReconnected$.pipe(
        map(() => resetMapActionStateAction({ scope: 'load' }))
      )
    ),
  },
];
