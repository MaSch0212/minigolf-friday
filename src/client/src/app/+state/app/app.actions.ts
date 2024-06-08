import { createAction, props } from '@ngrx/store';

export const setTitleAction = createAction(
  '[App] Set Title',
  props<{ title: string | undefined; translate?: boolean }>()
);
