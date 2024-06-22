import { createReducer, on } from '@ngrx/store';
import { produce } from 'immer';

import { loadPlayerEventReducers } from './actions/load-player-event.action';
import { loadPlayerEventsReducers } from './actions/load-player-events.action';
import { playerEventRemovedReducers } from './actions/player-event-removed.action';
import { resetPlayerEventsReducers } from './actions/reset-player-events.action';
import { updateEventRegistrationReducers } from './actions/update-event-registration.action';
import {
  PlayerEventsFeatureState,
  initialPlayerEventsFeatureState,
  playerEventEntityAdapter,
} from './player-events.state';
import {
  addEventAction,
  addEventTimeslotAction,
  removeEventAction,
  removeEventTimeslotAction,
  startEventAction,
  updateEventTimeslotAction,
} from '../events';

export const playerEventsReducer = createReducer<PlayerEventsFeatureState>(
  initialPlayerEventsFeatureState,

  ...loadPlayerEventsReducers,
  ...loadPlayerEventReducers,
  ...playerEventRemovedReducers,
  ...resetPlayerEventsReducers,
  ...updateEventRegistrationReducers,

  on(addEventAction.success, (state, { response }) =>
    state.actionStates.load.state === 'none'
      ? state
      : playerEventEntityAdapter.addOne(
          {
            id: response.id,
            date: response.date,
            isStarted: !!response.startedAt,
            registrationDeadline: response.registrationDeadline,
            timeslots: response.timeslots.map(t => ({
              id: t.id,
              time: t.time,
              isFallbackAllowed: t.isFallbackAllowed,
              isRegistered: false,
            })),
          },
          state
        )
  ),
  on(removeEventAction.success, (state, { props }) =>
    playerEventEntityAdapter.removeOne(props.eventId, state)
  ),
  on(addEventTimeslotAction.success, (state, { props, response }) =>
    playerEventEntityAdapter.mapOne(
      {
        id: props.eventId,
        map: produce(draft => {
          draft.timeslots.push({
            id: response.id,
            time: response.time,
            isFallbackAllowed: response.isFallbackAllowed,
            isRegistered: false,
          });
        }),
      },
      state
    )
  ),
  on(updateEventTimeslotAction.success, (state, { props }) =>
    playerEventEntityAdapter.mapOne(
      {
        id: props.eventId,
        map: produce(draft => {
          const timeslot = draft.timeslots.find(t => t.id === props.timeslotId);
          if (timeslot && props.changes.isFallbackAllowed !== undefined) {
            timeslot.isFallbackAllowed = props.changes.isFallbackAllowed;
          }
        }),
      },
      state
    )
  ),
  on(removeEventTimeslotAction.success, (state, { props }) =>
    playerEventEntityAdapter.mapOne(
      {
        id: props.eventId,
        map: produce(draft => {
          draft.timeslots = draft.timeslots.filter(t => t.id !== props.timeslotId);
        }),
      },
      state
    )
  ),
  on(startEventAction.success, (state, { props }) =>
    playerEventEntityAdapter.mapOne(
      {
        id: props.eventId,
        map: produce(draft => {
          draft.isStarted = true;
        }),
      },
      state
    )
  )
);
