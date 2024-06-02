import { createEntityAdapter, EntityState } from '@ngrx/entity';

import { MinigolfMap } from '../../models/parsed-models';
import { ActionState, initialActionState } from '../action-state';

export type MapsFeatureState = EntityState<MinigolfMap> & {
  actionStates: {
    load: ActionState;
    add: ActionState;
    update: ActionState;
    remove: ActionState;
  };
};

export const mapsEntityAdapter = createEntityAdapter<MinigolfMap>({
  selectId: map => map.id,
  sortComparer: (a, b) => a.name.localeCompare(b.name),
});

export const initialMapsFeatureState: MapsFeatureState = mapsEntityAdapter.getInitialState({
  actionStates: {
    load: initialActionState,
    add: initialActionState,
    update: initialActionState,
    remove: initialActionState,
  },
});
