import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { EMPTY, catchError, exhaustMap, map, of } from 'rxjs';

import * as actions from './players.actions';
import { selectPlayersLoadState } from './players.selectors';
import { PlayersService } from '../../services/players.service';

@Injectable()
export class PlayersFeatureEffects {
  private readonly _actions$ = inject(Actions);
  private readonly _store = inject(Store);
  private readonly _api = inject(PlayersService);

  private readonly loadState = this._store.selectSignal(selectPlayersLoadState);

  public loadPlayers$ = createEffect(() =>
    this._actions$.pipe(
      ofType(actions.loadPlayersAction),
      exhaustMap(({ reload }) =>
        reload || !this.loadState().loaded
          ? this._api.getPlayers().pipe(
              map(resp => actions.loadPlayersSuccessAction({ players: resp.players })),
              catchError(error => of(actions.loadPlayersFailureAction({ reload, error })))
            )
          : EMPTY
      )
    )
  );

  public addPlayer$ = createEffect(() =>
    this._actions$.pipe(
      ofType(actions.addPlayerAction),
      exhaustMap(({ player }) =>
        this._api.addPlayer(player).pipe(
          map(resp => actions.addPlayerSuccessAction({ player: { ...player, id: resp.id } })),
          catchError(error => of(actions.addPlayerFailureAction({ player, error })))
        )
      )
    )
  );

  public updatePlayer$ = createEffect(() =>
    this._actions$.pipe(
      ofType(actions.updatePlayerAction),
      exhaustMap(({ player }) =>
        this._api.updatePlayer(player).pipe(
          map(() => actions.updatePlayerSuccessAction({ player })),
          catchError(error => of(actions.updatePlayerFailureAction({ player, error })))
        )
      )
    )
  );

  public deletePlayer$ = createEffect(() =>
    this._actions$.pipe(
      ofType(actions.deletePlayerAction),
      exhaustMap(({ player }) =>
        this._api.deletePlayer(player).pipe(
          map(() => actions.deletePlayerSuccessAction({ player })),
          catchError(error => of(actions.deletePlayerFailureAction({ player, error })))
        )
      )
    )
  );
}
