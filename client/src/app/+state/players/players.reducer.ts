import { HttpErrorResponse } from '@angular/common/http';
import { EntityState, createEntityAdapter } from '@ngrx/entity';
import { createReducer, on } from '@ngrx/store';
import { produce } from 'immer';

import * as actions from './players.actions';
import { Player } from '../../models/player';
import { LoadState, getInitialLoadState } from '../load-state';

export type PlayersFeatureState = EntityState<Player> & {
  loadState: LoadState<HttpErrorResponse>;
  actionState: LoadState<HttpErrorResponse>;
};

export const playersEntityAdapter = createEntityAdapter<Player>({
  selectId: player => player.id,
  sortComparer: (a, b) => a.name.localeCompare(b.name),
});

export const playersReducer = createReducer<PlayersFeatureState>(
  playersEntityAdapter.getInitialState({
    loadState: getInitialLoadState(),
    actionState: getInitialLoadState(),
  }),

  on(
    actions.loadPlayersAction,
    produce(state => {
      state.loadState.error = undefined;
      state.loadState.loading = true;
    })
  ),
  on(actions.loadPlayersSuccessAction, (state, { players }) =>
    playersEntityAdapter.setAll(
      players,
      produce(state, draft => {
        draft.loadState = { loading: false, loaded: true, error: null };
      })
    )
  ),
  on(
    actions.loadPlayersFailureAction,
    produce((state, { error }) => {
      state.loadState.loading = false;
      state.loadState.error = error;
    })
  ),

  on(
    actions.addPlayerAction,
    actions.updatePlayerAction,
    actions.deletePlayerAction,
    produce(state => {
      state.actionState.error = undefined;
      state.actionState.loading = true;
    })
  ),
  on(actions.addPlayerSuccessAction, (state, { player }) =>
    playersEntityAdapter.addOne(
      player,
      produce(state, draft => {
        draft.actionState = { loading: false, loaded: true, error: null };
      })
    )
  ),
  on(actions.updatePlayerSuccessAction, (state, { player }) =>
    playersEntityAdapter.upsertOne(
      player,
      produce(state, draft => {
        draft.actionState = { loading: false, loaded: true, error: null };
      })
    )
  ),
  on(actions.deletePlayerSuccessAction, (state, { player }) =>
    playersEntityAdapter.removeOne(
      player.id,
      produce(state, draft => {
        draft.actionState = { loading: false, loaded: true, error: null };
      })
    )
  ),
  on(
    actions.addPlayerFailureAction,
    actions.updatePlayerFailureAction,
    actions.deletePlayerFailureAction,
    produce((state, { error }) => {
      state.actionState.loading = false;
      state.actionState.error = error;
    })
  )
);
