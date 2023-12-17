import { createFeature, provideState } from '@ngrx/store';

import { appReducer } from './app.reducer';

export const appFeature = createFeature({
  name: 'app',
  reducer: appReducer,
});

export function provideAppState() {
  return [provideState(appFeature)];
}
