import { createAction, on } from '@ngrx/store';
import { produce } from 'immer';

import { initialActionState } from '../../action-state';
import { Reducers } from '../../utils';
import { MAPS_ACTION_SCOPE } from '../consts';
import { mapsEntityAdapter, MapsFeatureState } from '../maps.state';

export const resetMapsAction = createAction(`[${MAPS_ACTION_SCOPE}] Reset`);

export const resetMapsReducers: Reducers<MapsFeatureState> = [
  on(resetMapsAction, state =>
    mapsEntityAdapter.removeAll(
      produce(state, draft => {
        draft.actionStates.load = initialActionState;
      })
    )
  ),
];
