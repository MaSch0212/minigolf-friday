import { EntityState, createEntityAdapter } from '@ngrx/entity';

import { PlayerEvent } from '../../models/parsed-models';
import { ActionState, initialActionState } from '../action-state';

export type PlayerEventsFeatureState = EntityState<PlayerEvent> & {
  continuationToken: string | null;
  actionStates: {
    load: ActionState;
    loadOne: ActionState;
    register: ActionState;
  };
};

export const playerEventEntityAdapter = createEntityAdapter<PlayerEvent>({
  selectId: event => event.id,
  sortComparer: (a, b) => b.date.getTime() - a.date.getTime(),
});

export const initialPlayerEventsFeatureState: PlayerEventsFeatureState =
  playerEventEntityAdapter.getInitialState({
    continuationToken: null,
    actionStates: {
      load: initialActionState,
      loadOne: initialActionState,
      register: initialActionState,
    },
  });
