import { addEventEffects } from './actions/add-event.action';
import { addPlayerToEventPreconfigurationEffects } from './actions/add-player-to-preconfig.action';
import { loadEventEffects } from './actions/load-event.action';
import { loadEventsEffects } from './actions/load-events.action';
import { Effects } from '../utils';

export const eventsFeatureEffects: Effects[] = [
  addEventEffects,
  addPlayerToEventPreconfigurationEffects,
  loadEventEffects,
  loadEventsEffects,
];
