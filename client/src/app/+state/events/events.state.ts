import { EntityState, createEntityAdapter } from '@ngrx/entity';

import { MinigolfEvent } from '../../models/event';
import { ActionState, initialActionState } from '../action-state';

export type EventsFeatureState = EntityState<MinigolfEvent> & {
  loadedPages: number;
  actionStates: {
    load: ActionState;
    loadOne: ActionState;
    add: ActionState;
    remove: ActionState;
    addTimeslot: ActionState;
    removeTimeslot: ActionState;
    buildInstances: ActionState;
    updateTimeslot: ActionState;
    addPreconfig: ActionState;
    removePreconfig: ActionState;
    addPlayerToPreconfig: ActionState;
    removePlayerFromPreconfig: ActionState;
  };
};

export const eventEntityAdapter = createEntityAdapter<MinigolfEvent>({
  selectId: event => event.id,
  sortComparer: (a, b) => b.date.getTime() - a.date.getTime(),
});

export const initialEventsFeatureState: EventsFeatureState = eventEntityAdapter.getInitialState({
  loadedPages: 0,
  actionStates: {
    load: initialActionState,
    loadOne: initialActionState,
    add: initialActionState,
    remove: initialActionState,
    addTimeslot: initialActionState,
    removeTimeslot: initialActionState,
    buildInstances: initialActionState,
    updateTimeslot: initialActionState,
    addPreconfig: initialActionState,
    removePreconfig: initialActionState,
    addPlayerToPreconfig: initialActionState,
    removePlayerFromPreconfig: initialActionState,
  },
});
