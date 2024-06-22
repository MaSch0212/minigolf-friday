import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { switchMap } from 'rxjs';

import { MapAdministrationService } from '../../../api/services';
import { MinigolfMap } from '../../../models/parsed-models';
import { assertBody } from '../../../utils/http.utils';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { MAPS_ACTION_SCOPE } from '../consts';
import { selectMapsActionState } from '../maps.selectors';
import { mapsEntityAdapter, MapsFeatureState } from '../maps.state';

export const loadMapsAction = createHttpAction<
  { reload?: boolean; silent?: boolean },
  MinigolfMap[]
>()(MAPS_ACTION_SCOPE, 'Load Maps');

export const loadMapsReducers: Reducers<MapsFeatureState> = [
  on(loadMapsAction.success, (state, { props, response }) =>
    mapsEntityAdapter.upsertMany(
      response,
      props.reload ? mapsEntityAdapter.removeAll(state) : state
    )
  ),
  handleHttpAction('load', loadMapsAction, { condition: (s, p) => !p.silent }),
];

export const loadMapsEffects: Effects = {
  loadMaps$: createFunctionalEffect.dispatching((api = inject(MapAdministrationService)) =>
    onHttpAction(loadMapsAction, selectMapsActionState('load'), p => !!p.props.silent).pipe(
      switchMap(({ props }) => toHttpAction(getMaps(api, props), loadMapsAction, props))
    )
  ),
};

async function getMaps(
  api: MapAdministrationService,
  props: ReturnType<typeof loadMapsAction>['props']
) {
  const response = await api.getMaps();
  return response.ok
    ? loadMapsAction.success(props, assertBody(response).maps)
    : loadMapsAction.error(props, response);
}
