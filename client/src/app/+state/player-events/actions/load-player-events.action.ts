import { inject } from '@angular/core';
import { Store, on } from '@ngrx/store';
import { produce } from 'immer';
import { switchMap, withLatestFrom } from 'rxjs';

import { GetPlayerEventsResponse } from '../../../models/api/player-event';
import { PlayerEventsService } from '../../../services/player-events.service';
import {
  createHttpAction,
  handleHttpAction,
  mapToHttpAction,
  onHttpAction,
} from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { PLAYER_EVENTS_ACTION_SCOPE } from '../consts';
import {
  selectPlayerEventsActionState,
  selectPlayerEventsLoadedPages,
} from '../player-events.selectors';
import { PlayerEventsFeatureState, playerEventEntityAdapter } from '../player-events.state';

export const loadPlayerEventsAction = createHttpAction<
  { reload?: boolean },
  GetPlayerEventsResponse
>()(PLAYER_EVENTS_ACTION_SCOPE, 'Load Player Events');

export const loadPlayerEventsReducers: Reducers<PlayerEventsFeatureState> = [
  on(loadPlayerEventsAction.success, (state, { props, response }) =>
    playerEventEntityAdapter.upsertMany(
      response.events,
      props.reload
        ? playerEventEntityAdapter.removeAll(
            produce(state, draft => {
              draft.loadedPages = 1;
            })
          )
        : produce(state, draft => {
            draft.loadedPages++;
          })
    )
  ),
  handleHttpAction('load', loadPlayerEventsAction),
];

export const loadPlayerEventsEffects: Effects = {
  loadPlayerEvents$: createFunctionalEffect.dispatching(
    (store = inject(Store), api = inject(PlayerEventsService)) =>
      onHttpAction(loadPlayerEventsAction, selectPlayerEventsActionState('load')).pipe(
        withLatestFrom(store.select(selectPlayerEventsLoadedPages)),
        switchMap(([{ props }, loadedPages]) =>
          api
            .getEvents({ page: props.reload ? 1 : loadedPages + 1 })
            .pipe(mapToHttpAction(loadPlayerEventsAction, props))
        )
      )
  ),
};
