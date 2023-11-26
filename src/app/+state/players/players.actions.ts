import { HttpErrorResponse } from '@angular/common/http';
import { createAction, props } from '@ngrx/store';

import { Player } from '../../models/player';

export const loadPlayersAction = createAction(
  '[Players] Load Players',
  props<{ reload: boolean }>()
);
export const loadPlayersSuccessAction = createAction(
  '[Players] Load Players Success',
  props<{ players: Player[] }>()
);
export const loadPlayersFailureAction = createAction(
  '[Players] Load Players Failure',
  props<{ error: HttpErrorResponse }>()
);
