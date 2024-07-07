import { inject } from '@angular/core';
import { Store, on } from '@ngrx/store';
import { produce } from 'immer';
import { switchMap, withLatestFrom } from 'rxjs';

import { ApiGetPlayerEventsResponse } from '../../../api/models';
import { EventsService } from '../../../api/services';
import { parsePlayerEvent, PlayerEvent } from '../../../models/parsed-models';
import { assertBody } from '../../../utils/http.utils';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { PLAYER_EVENTS_ACTION_SCOPE } from '../consts';
import {
  selectPlayerEventsActionState,
  selectPlayerEventsContinuationToken,
} from '../player-events.selectors';
import { PlayerEventsFeatureState, playerEventEntityAdapter } from '../player-events.state';

type _Response = { events: PlayerEvent[]; continuationToken: string | null };
export const loadPlayerEventsAction = createHttpAction<
  { reload?: boolean; silent?: boolean },
  _Response
>()(PLAYER_EVENTS_ACTION_SCOPE, 'Load Player Events');

export const loadPlayerEventsReducers: Reducers<PlayerEventsFeatureState> = [
  on(loadPlayerEventsAction.success, (state, { props, response }) =>
    playerEventEntityAdapter.upsertMany(
      response.events,
      produce(props.reload ? playerEventEntityAdapter.removeAll(state) : state, draft => {
        draft.continuationToken = response.continuationToken;
      })
    )
  ),
  handleHttpAction('load', loadPlayerEventsAction, {
    condition: (s, p) => !p.silent,
    startCondition: (s, p) => s.actionStates.load.state === 'none' || p.reload === true,
  }),
];

export const loadPlayerEventsEffects: Effects = {
  loadPlayerEvents$: createFunctionalEffect.dispatching(
    (store = inject(Store), api = inject(EventsService)) =>
      onHttpAction(
        loadPlayerEventsAction,
        selectPlayerEventsActionState('load'),
        p => !!p.props.silent
      ).pipe(
        withLatestFrom(store.select(selectPlayerEventsContinuationToken)),
        switchMap(([{ props }, continuationToken]) =>
          toHttpAction(
            getPlayerEvents(api, props, continuationToken),
            loadPlayerEventsAction,
            props
          )
        )
      )
  ),
};

async function getPlayerEvents(
  api: EventsService,
  props: ReturnType<typeof loadPlayerEventsAction>['props'],
  continuationToken: string | null
) {
  const response = await api.getPlayerEvents({ continuation: continuationToken ?? undefined });
  return response.ok
    ? loadPlayerEventsAction.success(props, toResponse(assertBody(response)))
    : loadPlayerEventsAction.error(props, response);
}

function toResponse(response: ApiGetPlayerEventsResponse): _Response {
  return {
    events: response.events.map(parsePlayerEvent),
    continuationToken: response.continuation ?? null,
  };
}
