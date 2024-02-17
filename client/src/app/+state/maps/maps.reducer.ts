import { HttpErrorResponse } from '@angular/common/http';
import { EntityState, createEntityAdapter } from '@ngrx/entity';
import { createReducer, on } from '@ngrx/store';
import { produce } from 'immer';

import * as actions from './maps.actions';
import { MinigolfMap } from '../../models/minigolf-map';
import { LoadState, getInitialLoadState } from '../load-state';

export type MapsFeatureState = EntityState<MinigolfMap> & {
  loadState: LoadState<HttpErrorResponse>;
  actionState: LoadState<HttpErrorResponse>;
};

export const mapsEntityAdapter = createEntityAdapter<MinigolfMap>({
  selectId: map => map.id,
  sortComparer: (a, b) => a.name.localeCompare(b.name),
});

export const mapsReducer = createReducer<MapsFeatureState>(
  mapsEntityAdapter.getInitialState({
    loadState: getInitialLoadState(),
    actionState: getInitialLoadState(),
  }),

  on(
    actions.loadMapsAction,
    produce((state, { reload }) => {
      state.loadState.error = undefined;
      state.loadState.loading = !state.loadState.loaded || reload;
    })
  ),
  on(actions.loadMapsSuccessAction, (state, { maps }) =>
    mapsEntityAdapter.setAll(
      maps,
      produce(state, draft => {
        draft.loadState = { loading: false, loaded: true, error: null };
      })
    )
  ),
  on(
    actions.loadMapsFailureAction,
    produce((state, { error }) => {
      state.loadState.loading = false;
      state.loadState.error = error;
    })
  ),

  on(
    actions.addMapAction,
    actions.updateMapAction,
    actions.deleteMapAction,
    produce(state => {
      state.actionState.error = undefined;
      state.actionState.loading = true;
    })
  ),
  on(actions.addMapSuccessAction, (state, { map }) =>
    mapsEntityAdapter.upsertOne(
      map,
      produce(state, draft => {
        draft.actionState = { loading: false, loaded: true, error: null };
      })
    )
  ),
  on(actions.updateMapSuccessAction, (state, { map }) =>
    mapsEntityAdapter.updateOne(
      { id: map.id, changes: map },
      produce(state, draft => {
        draft.actionState = { loading: false, loaded: true, error: null };
      })
    )
  ),
  on(actions.deleteMapSuccessAction, (state, { map }) =>
    mapsEntityAdapter.removeOne(
      map.id,
      produce(state, draft => {
        draft.actionState = { loading: false, loaded: true, error: null };
      })
    )
  ),
  on(
    actions.addMapFailureAction,
    actions.updateMapFailureAction,
    actions.deleteMapFailureAction,
    produce((state, { error }) => {
      state.actionState.loading = false;
      state.actionState.error = error;
    })
  )
);
