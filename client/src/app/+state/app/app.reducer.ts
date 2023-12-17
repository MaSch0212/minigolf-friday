import { createReducer, on } from '@ngrx/store';

import { setTitleAction } from './app.actions';

export type AppFeatureState = {
  title: string | undefined;
  translateTitle: boolean;
};

export const appReducer = createReducer<AppFeatureState>(
  {
    title: undefined,
    translateTitle: false,
  },
  on(setTitleAction, (state, { title, translate }) => ({
    ...state,
    title,
    translateTitle: translate ?? false,
  }))
);
