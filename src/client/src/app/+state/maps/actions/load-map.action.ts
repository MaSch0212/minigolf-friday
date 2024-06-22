import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { mergeMap } from 'rxjs';

import { MapAdministrationService } from '../../../api/services';
import { MinigolfMap } from '../../../models/parsed-models';
import { assertBody } from '../../../utils/http.utils';
import { createHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { MAPS_ACTION_SCOPE } from '../consts';
import { mapsEntityAdapter, MapsFeatureState } from '../maps.state';

export const loadMapAction = createHttpAction<{ mapId: string }, MinigolfMap>()(
  MAPS_ACTION_SCOPE,
  'Load Map'
);

export const loadMapReducers: Reducers<MapsFeatureState> = [
  on(loadMapAction.success, (state, { response }) => mapsEntityAdapter.upsertOne(response, state)),
];

export const loadMapEffects: Effects = {
  loadMap$: createFunctionalEffect.dispatching((api = inject(MapAdministrationService)) =>
    onHttpAction(loadMapAction).pipe(
      mergeMap(({ props }) => toHttpAction(getMap(api, props), loadMapAction, props))
    )
  ),
};

async function getMap(
  api: MapAdministrationService,
  props: ReturnType<typeof loadMapAction>['props']
) {
  const response = await api.getMap({ mapId: props.mapId });
  return response.ok
    ? loadMapAction.success(props, assertBody(response).map)
    : loadMapAction.error(props, response);
}
