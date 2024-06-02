import { addMapEffects } from './actions/add-map.action';
import { loadMapsEffects } from './actions/load-maps.action';
import { removeMapEffects } from './actions/remove-map.action';
import { updateMapEffects } from './actions/update-map.action';
import { Effects } from '../utils';

export const mapsFeatureEffects: Effects[] = [
  addMapEffects,
  loadMapsEffects,
  removeMapEffects,
  updateMapEffects,
];
