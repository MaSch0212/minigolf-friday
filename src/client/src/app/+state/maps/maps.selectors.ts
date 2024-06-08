import { createDistinctSelector } from '@ngneers/easy-ngrx-distinct-selector';
import { createFeatureSelector } from '@ngrx/store';

import { MAPS_FEATURE_NAME } from './consts';
import { mapsEntityAdapter, MapsFeatureState } from './maps.state';

export const selectMapsFeature = createFeatureSelector<MapsFeatureState>(MAPS_FEATURE_NAME);

export const mapSelectors = mapsEntityAdapter.getSelectors(selectMapsFeature);

export const selectMapsActionStates = createDistinctSelector(
  selectMapsFeature,
  state => state.actionStates
);
export function selectMapsActionState(action: keyof MapsFeatureState['actionStates']) {
  return createDistinctSelector(selectMapsActionStates, state => state[action]);
}
