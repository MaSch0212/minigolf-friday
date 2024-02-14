import { addEventPreconfigEffects } from './actions/add-event-preconfig.action';
import { addEventTimeslotEffects } from './actions/add-event-timeslot.action';
import { addEventEffects } from './actions/add-event.action';
import { addPlayerToEventPreconfigurationEffects } from './actions/add-player-to-preconfig.action';
import { loadEventEffects } from './actions/load-event.action';
import { loadEventsEffects } from './actions/load-events.action';
import { removeEventPreconfigEffects } from './actions/remove-event-preconfig.action';
import { removeEventTimeslotEffects } from './actions/remove-event-timeslot.action';
import { removeEventEffects } from './actions/remove-event.action';
import { removePlayerFromPreconfigEffects } from './actions/remove-player-from-preconfig.action';
import { Effects } from '../utils';

export const eventsFeatureEffects: Effects[] = [
  addEventPreconfigEffects,
  addEventTimeslotEffects,
  addEventEffects,
  addPlayerToEventPreconfigurationEffects,
  loadEventEffects,
  loadEventsEffects,
  removeEventPreconfigEffects,
  removeEventTimeslotEffects,
  removeEventEffects,
  removePlayerFromPreconfigEffects,
];
