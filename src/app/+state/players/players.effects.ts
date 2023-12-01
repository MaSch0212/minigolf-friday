import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of } from 'rxjs';

import * as actions from './players.actions';
import { PlayersService } from '../../services/players.service';

@Injectable()
export class PlayersFeatureEffects {
  private readonly _actions$ = inject(Actions);
  private readonly _api = inject(PlayersService);

  public loadPlayers$ = createEffect(() =>
    this._actions$.pipe(
      ofType(actions.loadPlayersAction),
      exhaustMap(() =>
        this._api.getPlayers().pipe(
          map(resp => actions.loadPlayersSuccessAction({ players: resp.players })),
          catchError(error => of(actions.loadPlayersFailureAction({ error })))
        )
      )
    )
  );

  public addPlayer$ = createEffect(() =>
    this._actions$.pipe(
      ofType(actions.addPlayerAction),
      exhaustMap(({ player }) =>
        this._api.addPlayer(player).pipe(
          map(resp => actions.addPlayerSuccessAction({ player: { ...player, id: resp.id } })),
          catchError(error => of(actions.addPlayerFailureAction({ error })))
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
          catchError(error => of(actions.updatePlayerFailureAction({ error })))
        )
      )
    )
  );
}