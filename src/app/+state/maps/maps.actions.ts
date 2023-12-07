import { HttpErrorResponse } from '@angular/common/http';
import { createAction, props } from '@ngrx/store';

import { MinigolfMap } from '../../models/minigolf-map';

export const loadMapsAction = createAction('[Maps] Load Maps', props<{ reload: boolean }>());
export const loadMapsSuccessAction = createAction(
  '[Maps] Load Maps Success',
  props<{ maps: MinigolfMap[] }>()
);
export const loadMapsFailureAction = createAction(
  '[Maps] Load Maps Failure',
  props<{ reload: boolean; error: HttpErrorResponse }>()
);

export const addMapAction = createAction('[Maps] Add Map', props<{ map: MinigolfMap }>());
export const addMapSuccessAction = createAction(
  '[Maps] Add Map Success',
  props<{ map: MinigolfMap }>()
);
export const addMapFailureAction = createAction(
  '[Maps] Add Map Failure',
  props<{ map: MinigolfMap; error: HttpErrorResponse }>()
);

export const updateMapAction = createAction('[Maps] Update Map', props<{ map: MinigolfMap }>());
export const updateMapSuccessAction = createAction(
  '[Maps] Update Map Success',
  props<{ map: MinigolfMap }>()
);
export const updateMapFailureAction = createAction(
  '[Maps] Update Map Failure',
  props<{ map: MinigolfMap; error: HttpErrorResponse }>()
);

export const deleteMapAction = createAction('[Maps] Delete Map', props<{ map: MinigolfMap }>());
export const deleteMapSuccessAction = createAction(
  '[Maps] Delete Map Success',
  props<{ map: MinigolfMap }>()
);
export const deleteMapFailureAction = createAction(
  '[Maps] Delete Map Failure',
  props<{ map: MinigolfMap; error: HttpErrorResponse }>()
);
