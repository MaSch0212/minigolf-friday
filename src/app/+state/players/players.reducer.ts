import { HttpErrorResponse } from '@angular/common/http';
import { EntityState, createEntityAdapter } from '@ngrx/entity';
import { createReducer, on } from '@ngrx/store';
import { produce } from 'immer';

import * as actions from './players.actions';
import { Player } from '../../models/player';
import { LoadState, getInitialLoadState } from '../load-state';

export type PlayersFeatureState = EntityState<Player> & {
  loadState: LoadState<HttpErrorResponse>;
};

export const playersEntityAdapter = createEntityAdapter<Player>({
  selectId: player => player.id,
  sortComparer: (a, b) => a.name.localeCompare(b.name),
});

export const playersReducer = createReducer<PlayersFeatureState>(
  playersEntityAdapter.getInitialState({
    loadState: getInitialLoadState(),
  }),

  on(
    actions.loadPlayersAction,
    produce(state => {
      state.loadState.error = undefined;
      state.loadState.loading = true;
    })
  ),
  on(actions.loadPlayersSuccessAction, (state, { players }) => {
    state = playersEntityAdapter.setAll(players, state);
    state = { ...state, loadState: { loading: false, loaded: true, error: null } };
    return state;
  }),
  on(
    actions.loadPlayersFailureAction,
    produce((state, { error }) => {
      state.loadState.loading = false;
      state.loadState.error = error;
    })
  )
);
