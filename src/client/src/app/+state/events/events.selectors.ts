import { createDistinctSelector } from '@ngneers/easy-ngrx-distinct-selector';
import { createFeatureSelector } from '@ngrx/store';

import { EVENTS_FEATURE_NAME } from './consts';
import { EventsFeatureState, eventEntityAdapter } from './events.state';

export const selectEventsFeature = createFeatureSelector<EventsFeatureState>(EVENTS_FEATURE_NAME);

export const eventSelectors = eventEntityAdapter.getSelectors(selectEventsFeature);

export const selectEventsContinuationToken = createDistinctSelector(
  selectEventsFeature,
  state => state.continuationToken
);

export const selectEventsActionStates = createDistinctSelector(
  selectEventsFeature,
  state => state.actionStates
);
export function selectEventsActionState(action: keyof EventsFeatureState['actionStates']) {
  return createDistinctSelector(selectEventsActionStates, state => state[action]);
}

export function selectEvent(id: string | null | undefined) {
  return createDistinctSelector(selectEventsFeature, state =>
    id ? state.entities[id] ?? null : null
  );
}

export function selectEventTimeslot(
  eventId: string | null | undefined,
  timeslotId: string | null | undefined
) {
  return createDistinctSelector(selectEventsFeature, state =>
    eventId && timeslotId
      ? state.entities[eventId]?.timeslots.find(x => x.id === timeslotId) ?? null
      : null
  );
}
