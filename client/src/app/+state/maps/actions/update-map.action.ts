import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { produce } from 'immer';
import { switchMap } from 'rxjs';

import { MapAdministrationService } from '../../../api/services';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Reducers, Effects } from '../../utils';
import { MAPS_ACTION_SCOPE } from '../consts';
import { selectMapsActionState } from '../maps.selectors';
import { MapsFeatureState, mapsEntityAdapter } from '../maps.state';

export const updateMapAction = createHttpAction<{ mapId: string; name: string }>()(
  MAPS_ACTION_SCOPE,
  'Update Map'
);

export const updateMapReducers: Reducers<MapsFeatureState> = [
  on(updateMapAction.success, (state, { props }) =>
    mapsEntityAdapter.mapOne(
      {
        id: props.mapId,
        map: produce(draft => {
          draft.name = props.name;
        }),
      },
      state
    )
  ),
  handleHttpAction('update', updateMapAction),
];

export const updateMapEffects: Effects = {
  updateMap$: createFunctionalEffect.dispatching((api = inject(MapAdministrationService)) =>
    onHttpAction(updateMapAction, selectMapsActionState('update')).pipe(
      switchMap(({ props }) => toHttpAction(updateMap(api, props), updateMapAction, props))
    )
  ),
};

async function updateMap(
  api: MapAdministrationService,
  props: ReturnType<typeof updateMapAction>['props']
) {
  const response = await api.updateMap({ mapId: props.mapId, body: { name: props.name } });
  return response.ok
    ? updateMapAction.success(props, undefined)
    : updateMapAction.error(props, response);
}
