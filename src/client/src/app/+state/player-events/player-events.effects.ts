import { loadPlayerEventEffects } from './actions/load-player-event.action';
import { loadPlayerEventsEffects } from './actions/load-player-events.action';
import { registerForEventEffects } from './actions/register-for-event.action';
import { Effects } from '../utils';

export const PlayerEventsFeatureEffects: Effects[] = [
  loadPlayerEventsEffects,
  loadPlayerEventEffects,
  registerForEventEffects,
];
