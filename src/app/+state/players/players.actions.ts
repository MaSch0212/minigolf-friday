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

export const addPlayerAction = createAction('[Players] Add Player', props<{ player: Player }>());
export const addPlayerSuccessAction = createAction(
  '[Players] Add Player Success',
  props<{ player: Player }>()
);
export const addPlayerFailureAction = createAction(
  '[Players] Add Player Failure',
  props<{ error: HttpErrorResponse }>()
);

export const updatePlayerAction = createAction(
  '[Players] Update Player',
  props<{ player: Player }>()
);
export const updatePlayerSuccessAction = createAction(
  '[Players] Update Player Success',
  props<{ player: Player }>()
);
export const updatePlayerFailureAction = createAction(
  '[Players] Update Player Failure',
  props<{ error: HttpErrorResponse }>()
);
