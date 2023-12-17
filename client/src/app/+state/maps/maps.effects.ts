import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { EMPTY, catchError, exhaustMap, map, of } from 'rxjs';

import * as actions from './maps.actions';
import { selectMapsLoadState } from './maps.selectors';
import { MapsService } from '../../services/maps.service';

@Injectable()
export class MapsFeatureEffects {
  private readonly _actions$ = inject(Actions);
  private readonly _store = inject(Store);
  private readonly _api = inject(MapsService);

  private readonly loadState = this._store.selectSignal(selectMapsLoadState);

  public loadMaps$ = createEffect(() =>
    this._actions$.pipe(
      ofType(actions.loadMapsAction),
      exhaustMap(({ reload }) =>
        reload || !this.loadState().loaded
          ? this._api.getMaps().pipe(
              map(resp => actions.loadMapsSuccessAction({ maps: resp.maps })),
              catchError(error => of(actions.loadMapsFailureAction({ reload, error })))
            )
          : EMPTY
      )
    )
  );

  public addMap$ = createEffect(() =>
    this._actions$.pipe(
      ofType(actions.addMapAction),
      exhaustMap(({ map: minigolfMap }) =>
        this._api.addMap(minigolfMap).pipe(
          map(resp => actions.addMapSuccessAction({ map: { ...minigolfMap, id: resp.id } })),
          catchError(error => of(actions.addMapFailureAction({ map: minigolfMap, error })))
        )
      )
    )
  );

  public updateMap$ = createEffect(() =>
    this._actions$.pipe(
      ofType(actions.updateMapAction),
      exhaustMap(({ map: minigolfMap }) =>
        this._api.updateMap(minigolfMap).pipe(
          map(() => actions.updateMapSuccessAction({ map: minigolfMap })),
          catchError(error => of(actions.updateMapFailureAction({ map: minigolfMap, error })))
        )
      )
    )
  );

  public deleteMap$ = createEffect(() =>
    this._actions$.pipe(
      ofType(actions.deleteMapAction),
      exhaustMap(({ map: minigolfMap }) =>
        this._api.deleteMap(minigolfMap).pipe(
          map(() => actions.deleteMapSuccessAction({ map: minigolfMap })),
          catchError(error => of(actions.deleteMapFailureAction({ map: minigolfMap, error })))
        )
      )
    )
  );
}
