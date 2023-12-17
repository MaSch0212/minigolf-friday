import { createDistinctSelector } from '@ngneers/easy-ngrx-distinct-selector';
import { createFeatureSelector } from '@ngrx/store';

import { appFeature } from './app.feature';
import { AppFeatureState } from './app.reducer';

export const selectAppFeature = createFeatureSelector<AppFeatureState>(appFeature.name);

export const selectAppTitle = createDistinctSelector(selectAppFeature, state => ({
  title: state.title,
  translate: state.translateTitle,
}));
