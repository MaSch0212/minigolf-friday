import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { switchMap } from 'rxjs';

import { MapAdministrationService } from '../../../api/services';
import { MinigolfMap } from '../../../models/parsed-models';
import { assertBody } from '../../../utils/http.utils';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { EVENTS_ACTION_SCOPE } from '../../events/consts';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { selectMapsActionState } from '../maps.selectors';
import { mapsEntityAdapter, MapsFeatureState } from '../maps.state';

export const addMapAction = createHttpAction<{ name: string }, MinigolfMap>()(
  EVENTS_ACTION_SCOPE,
  'Add Map'
);

export const addMapReducers: Reducers<MapsFeatureState> = [
  on(addMapAction.success, (state, { response }) => mapsEntityAdapter.addOne(response, state)),
  handleHttpAction('add', addMapAction),
];

export const addMapEffects: Effects = {
  addMap$: createFunctionalEffect.dispatching((api = inject(MapAdministrationService)) =>
    onHttpAction(addMapAction, selectMapsActionState('add')).pipe(
      switchMap(({ props }) => toHttpAction(createMap(api, props), addMapAction, props))
    )
  ),
};

async function createMap(
  api: MapAdministrationService,
  props: ReturnType<typeof addMapAction>['props']
) {
  const response = await api.createMap({ body: { name: props.name } });
  return response.ok
    ? addMapAction.success(props, assertBody(response).map)
    : addMapAction.error(props, response);
}
