import { createDistinctSelector } from '@ngneers/easy-ngrx-distinct-selector';
import { createFeatureSelector } from '@ngrx/store';

import { PLAYER_EVENTS_FEATURE_NAME } from './consts';
import { PlayerEventsFeatureState, playerEventEntityAdapter } from './player-events.state';

export const selectPlayerEventsFeature = createFeatureSelector<PlayerEventsFeatureState>(
  PLAYER_EVENTS_FEATURE_NAME
);

export const playerEventSelectors =
  playerEventEntityAdapter.getSelectors(selectPlayerEventsFeature);

export const selectPlayerEventsLoadedPages = createDistinctSelector(
  selectPlayerEventsFeature,
  state => state.loadedPages
);

export const selectPlayerEventsActionStates = createDistinctSelector(
  selectPlayerEventsFeature,
  state => state.actionStates
);
export function selectPlayerEventsActionState(
  action: keyof PlayerEventsFeatureState['actionStates']
) {
  return createDistinctSelector(selectPlayerEventsActionStates, state => state[action]);
}

export function selectPlayerEvent(id: string) {
  return createDistinctSelector(selectPlayerEventsFeature, state => state.entities[id]);
}

export function selectPlayerEventTimeslot(eventId: string, timeslotId: string) {
  return createDistinctSelector(selectPlayerEventsFeature, state =>
    state.entities[eventId]?.timeslots.find(x => x.id === timeslotId)
  );
}
