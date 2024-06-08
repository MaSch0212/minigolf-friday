import { createReducer } from '@ngrx/store';

import { addMapReducers } from './actions/add-map.action';
import { loadMapsReducers } from './actions/load-maps.action';
import { removeMapReducers } from './actions/remove-map.action';
import { updateMapReducers } from './actions/update-map.action';
import { initialMapsFeatureState, MapsFeatureState } from './maps.state';

export const mapsReducer = createReducer<MapsFeatureState>(
  initialMapsFeatureState,

  ...addMapReducers,
  ...loadMapsReducers,
  ...removeMapReducers,
  ...updateMapReducers
);
