import { inject } from '@angular/core';
import { Store, on } from '@ngrx/store';
import { produce } from 'immer';
import { switchMap, withLatestFrom } from 'rxjs';

import { ApiGetEventsResponse } from '../../../api/models';
import { EventAdministrationService } from '../../../api/services';
import { Event, parseEvent } from '../../../models/parsed-models';
import { assertBody } from '../../../utils/http.utils';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { EVENTS_ACTION_SCOPE } from '../consts';
import { selectEventsActionState, selectEventsContinuationToken } from '../events.selectors';
import { EventsFeatureState, eventEntityAdapter } from '../events.state';

type _Response = { events: Event[]; continuationToken: string | null };
export const loadEventsAction = createHttpAction<
  { reload?: boolean; silent?: boolean },
  _Response
>()(EVENTS_ACTION_SCOPE, 'Load Events');

export const loadEventsReducers: Reducers<EventsFeatureState> = [
  on(loadEventsAction.success, (state, { props, response }) =>
    eventEntityAdapter.upsertMany(
      response.events,
      produce(props.reload ? eventEntityAdapter.removeAll(state) : state, draft => {
        draft.continuationToken = response.continuationToken;
      })
    )
  ),
  handleHttpAction('load', loadEventsAction, { condition: (s, p) => !p.silent }),
];

export const loadEventsEffects: Effects = {
  loadEvents$: createFunctionalEffect.dispatching(
    (store = inject(Store), api = inject(EventAdministrationService)) =>
      onHttpAction(loadEventsAction, selectEventsActionState('load'), p => !!p.props.silent).pipe(
        withLatestFrom(store.select(selectEventsContinuationToken)),
        switchMap(([{ props }, continuationToken]) =>
          toHttpAction(getEvents(api, props, continuationToken), loadEventsAction, props)
        )
      )
  ),
};

async function getEvents(
  api: EventAdministrationService,
  props: ReturnType<typeof loadEventsAction>['props'],
  continuationToken: string | null
) {
  const response = await api.getEvents({ continuation: continuationToken ?? undefined });
  return response.ok
    ? loadEventsAction.success(props, toResponse(assertBody(response)))
    : loadEventsAction.error(props, response);
}

function toResponse(response: ApiGetEventsResponse): _Response {
  return {
    events: response.events.map(parseEvent),
    continuationToken: response.continuation ?? null,
  };
}
