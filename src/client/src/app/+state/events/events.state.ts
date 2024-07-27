import { EntityState, createEntityAdapter } from '@ngrx/entity';

import { Event } from '../../models/parsed-models';
import { ActionState, initialActionState } from '../action-state';

export type EventsFeatureState = EntityState<Event> & {
  continuationToken: string | null;
  actionStates: {
    load: ActionState;
    loadOne: ActionState;
    add: ActionState;
    remove: ActionState;
    start: ActionState;
    update: ActionState;
    addTimeslot: ActionState;
    removeTimeslot: ActionState;
    buildInstances: ActionState;
    setInstances: ActionState;
    updateTimeslot: ActionState;
    addPreconfig: ActionState;
    removePreconfig: ActionState;
    addPlayerToPreconfig: ActionState;
    removePlayerFromPreconfig: ActionState;
  };
};

export const eventEntityAdapter = createEntityAdapter<Event>({
  selectId: event => event.id,
  sortComparer: (a, b) => b.date.getTime() - a.date.getTime(),
});

export const initialEventsFeatureState: EventsFeatureState = eventEntityAdapter.getInitialState({
  continuationToken: null,
  actionStates: {
    load: initialActionState,
    loadOne: initialActionState,
    add: initialActionState,
    remove: initialActionState,
    start: initialActionState,
    update: initialActionState,
    addTimeslot: initialActionState,
    removeTimeslot: initialActionState,
    buildInstances: initialActionState,
    setInstances: initialActionState,
    updateTimeslot: initialActionState,
    addPreconfig: initialActionState,
    removePreconfig: initialActionState,
    addPlayerToPreconfig: initialActionState,
    removePlayerFromPreconfig: initialActionState,
  },
});
