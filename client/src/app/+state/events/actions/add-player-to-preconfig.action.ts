import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { produce } from 'immer';
import { switchMap } from 'rxjs';

import { AddPlayerToPreconfigRequest } from '../../../models/api/event';
import { EventsService } from '../../../services/events.service';
import {
  createHttpAction,
  handleHttpAction,
  mapToHttpAction,
  onHttpAction,
} from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { EVENTS_ACTION_SCOPE } from '../consts';
import { selectEventsActionState } from '../events.selectors';
import { EventsFeatureState, eventEntityAdapter } from '../events.state';

export const addPlayerToEventPreconfigurationAction = createHttpAction<
  { eventId: string; timeslotId: string; preconfigId: string } & AddPlayerToPreconfigRequest
>()(EVENTS_ACTION_SCOPE, 'Add Player To Preconfiguration');

export const addPlayerToEventPreconfigurationReducers: Reducers<EventsFeatureState> = [
  on(addPlayerToEventPreconfigurationAction.success, (state, { props }) => {
    const event = state.entities[props.eventId];
    return event
      ? eventEntityAdapter.upsertOne(
          produce(event, draft => {
            const timeslot = draft.timeslots.find(x => x.id === props.timeslotId);
            if (!timeslot) return;
            const preconfig = timeslot.preconfigurations.find(x => x.id === props.preconfigId);
            if (!preconfig) return;
            preconfig.playerIds.push(props.playerId);
          }),
          state
        )
      : state;
  }),
  handleHttpAction(
    'addPlayerToPreconfig',
    addPlayerToEventPreconfigurationAction,
    (s, p) =>
      s.entities[p.eventId]?.timeslots.some(
        x => x.id === p.timeslotId && x.preconfigurations.some(y => y.id === p.preconfigId)
      ) === true
  ),
];

export const addPlayerToEventPreconfigurationEffects: Effects = {
  addPlayerToEventPreconfiguration$: createFunctionalEffect.dispatching(
    (api = inject(EventsService)) =>
      onHttpAction(
        addPlayerToEventPreconfigurationAction,
        selectEventsActionState('addPlayerToPreconfig')
      ).pipe(
        switchMap(({ props }) =>
          api
            .addPlayerToPreconfig(props.preconfigId, { playerId: props.playerId })
            .pipe(mapToHttpAction(addPlayerToEventPreconfigurationAction, props))
        )
      )
  ),
};
