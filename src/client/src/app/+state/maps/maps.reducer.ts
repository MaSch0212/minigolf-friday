import { createReducer } from '@ngrx/store';

import { addMapReducers } from './actions/add-map.action';
import { loadMapReducers } from './actions/load-map.action';
import { loadMapsReducers } from './actions/load-maps.action';
import { removeMapReducers } from './actions/remove-map.action';
import { resetMapsActionStateReducers } from './actions/reset-maps-action-state.action';
import { updateMapReducers } from './actions/update-map.action';
import { initialMapsFeatureState, MapsFeatureState } from './maps.state';

export const mapsReducer = createReducer<MapsFeatureState>(
  initialMapsFeatureState,

  ...addMapReducers,
  ...loadMapReducers,
  ...loadMapsReducers,
  ...removeMapReducers,
  ...resetMapsActionStateReducers,
  ...updateMapReducers
);
