import { EntityState, createEntityAdapter } from '@ngrx/entity';

import { MinigolfPlayerEvent } from '../../models/player-event';
import { ActionState, initialActionState } from '../action-state';

export type PlayerEventsFeatureState = EntityState<MinigolfPlayerEvent> & {
  loadedPages: number;
  actionStates: {
    load: ActionState;
    loadOne: ActionState;
    register: ActionState;
  };
};

export const playerEventEntityAdapter = createEntityAdapter<MinigolfPlayerEvent>({
  selectId: event => event.id,
  sortComparer: (a, b) => b.date.getTime() - a.date.getTime(),
});

export const initialPlayerEventsFeatureState: PlayerEventsFeatureState =
  playerEventEntityAdapter.getInitialState({
    loadedPages: 0,
    actionStates: {
      load: initialActionState,
      loadOne: initialActionState,
      register: initialActionState,
    },
  });
