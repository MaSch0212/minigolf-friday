import { createAction, on, props } from '@ngrx/store';
import { produce } from 'immer';

import { initialActionState } from '../../action-state';
import { Reducers } from '../../utils';
import { EVENTS_ACTION_SCOPE } from '../consts';
import { EventsFeatureState } from '../events.state';

export const resetEventsActionStateAction = createAction(
  `[${EVENTS_ACTION_SCOPE}] Reset Action State`,
  props<{ scope: keyof EventsFeatureState['actionStates'] }>()
);

export const resetEventsActionStateReducers: Reducers<EventsFeatureState> = [
  on(
    resetEventsActionStateAction,
    produce((state, { scope }) => {
      state.actionStates[scope] = initialActionState;
    })
  ),
];
