import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { switchMap } from 'rxjs';

import { MapAdministrationService } from '../../../api/services';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { MAPS_ACTION_SCOPE } from '../consts';
import { selectMapsActionState } from '../maps.selectors';
import { mapsEntityAdapter, MapsFeatureState } from '../maps.state';

export const removeMapAction = createHttpAction<{ mapId: string }>()(
  MAPS_ACTION_SCOPE,
  'Remove Map'
);

export const removeMapReducers: Reducers<MapsFeatureState> = [
  on(removeMapAction.success, (state, { props }) =>
    mapsEntityAdapter.removeOne(props.mapId, state)
  ),
  handleHttpAction('remove', removeMapAction),
];

export const removeMapEffects: Effects = {
  removeMap$: createFunctionalEffect.dispatching((api = inject(MapAdministrationService)) =>
    onHttpAction(removeMapAction, selectMapsActionState('remove')).pipe(
      switchMap(({ props }) => toHttpAction(deleteMap(api, props), removeMapAction, props))
    )
  ),
};

async function deleteMap(
  api: MapAdministrationService,
  props: ReturnType<typeof removeMapAction>['props']
) {
  const response = await api.deleteMap({ mapId: props.mapId });
  return response.ok
    ? removeMapAction.success(props, undefined)
    : removeMapAction.error(props, response);
}
