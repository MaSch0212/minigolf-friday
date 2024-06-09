import { loadPlayerEventEffects } from './actions/load-player-event.action';
import { loadPlayerEventsEffects } from './actions/load-player-events.action';
import { updateEventRegistrationEffects } from './actions/update-event-registration.action';
import { Effects } from '../utils';

export const PlayerEventsFeatureEffects: Effects[] = [
  loadPlayerEventsEffects,
  loadPlayerEventEffects,
  updateEventRegistrationEffects,
];
