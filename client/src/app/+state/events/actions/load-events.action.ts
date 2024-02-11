import { inject } from '@angular/core';
import { Store, on } from '@ngrx/store';
import { produce } from 'immer';
import { switchMap, withLatestFrom } from 'rxjs';

import { GetAllEventsResponse } from '../../../models/api/event';
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
import { selectEventsActionState, selectEventsLoadedPages } from '../events.selectors';
import { EventsFeatureState, eventEntityAdapter } from '../events.state';

export const loadEventsAction = createHttpAction<{ reload?: boolean }, GetAllEventsResponse>()(
  EVENTS_ACTION_SCOPE,
  'Load Events'
);

export const loadEventsReducers: Reducers<EventsFeatureState> = [
  on(loadEventsAction.success, (state, { props, response }) =>
    eventEntityAdapter.addMany(
      response.events,
      props.reload
        ? eventEntityAdapter.removeAll(
            produce(state, draft => {
              draft.loadedPages = 1;
            })
          )
        : produce(state, draft => {
            draft.loadedPages++;
          })
    )
  ),
  handleHttpAction('load', loadEventsAction),
];

export const loadEventsEffects: Effects = {
  loadEvents$: createFunctionalEffect.dispatching(
    (store = inject(Store), api = inject(EventsService)) =>
      onHttpAction(loadEventsAction, selectEventsActionState('load')).pipe(
        withLatestFrom(store.select(selectEventsLoadedPages)),
        switchMap(([{ props }, loadedPages]) =>
          api
            .getAllEvents({ page: props.reload ? 1 : loadedPages + 1 })
            .pipe(mapToHttpAction(loadEventsAction, props))
        )
      )
  ),
};
