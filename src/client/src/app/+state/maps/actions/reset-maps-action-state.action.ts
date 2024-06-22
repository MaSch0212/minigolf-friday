import { createAction, on, props } from '@ngrx/store';
import { produce } from 'immer';

import { initialActionState } from '../../action-state';
import { Reducers } from '../../utils';
import { MAPS_ACTION_SCOPE } from '../consts';
import { MapsFeatureState } from '../maps.state';

export const resetMapsActionStateAction = createAction(
  `[${MAPS_ACTION_SCOPE}] Reset Action State`,
  props<{ scope: keyof MapsFeatureState['actionStates'] }>()
);

export const resetMapsActionStateReducers: Reducers<MapsFeatureState> = [
  on(
    resetMapsActionStateAction,
    produce((state, { scope }) => {
      state.actionStates[scope] = initialActionState;
    })
  ),
];
