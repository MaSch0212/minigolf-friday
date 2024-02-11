import { createDistinctSelector } from '@ngneers/easy-ngrx-distinct-selector';
import { createFeatureSelector } from '@ngrx/store';

import { EVENTS_FEATURE_NAME } from './consts';
import { EventsFeatureState, eventEntityAdapter } from './events.state';

export const selectEventsFeature = createFeatureSelector<EventsFeatureState>(EVENTS_FEATURE_NAME);

export const eventSelectors = eventEntityAdapter.getSelectors(selectEventsFeature);

export const selectEventsLoadedPages = createDistinctSelector(
  selectEventsFeature,
  state => state.loadedPages
);

export const selectEventsActionStates = createDistinctSelector(
  selectEventsFeature,
  state => state.actionStates
);
export function selectEventsActionState(action: keyof EventsFeatureState['actionStates']) {
  return createDistinctSelector(selectEventsActionStates, state => state[action]);
}

export function selectEvent(id: string) {
  return createDistinctSelector(selectEventsFeature, state => state.entities[id]);
}

export function selectEventTimeslot(eventId: string, timeslotId: string) {
  return createDistinctSelector(selectEventsFeature, state =>
    state.entities[eventId]?.timeslots.find(x => x.id === timeslotId)
  );
}
