import { createDistinctSelector } from '@ngneers/easy-ngrx-distinct-selector';
import { createFeatureSelector } from '@ngrx/store';

import { USERS_FEATURE_NAME } from './consts';
import { UsersFeatureState, userEntityAdapter } from './users.state';

export const selectUsersFeature = createFeatureSelector<UsersFeatureState>(USERS_FEATURE_NAME);

export const userSelectors = userEntityAdapter.getSelectors(selectUsersFeature);

export const selectUsersActionStates = createDistinctSelector(
  selectUsersFeature,
  state => state.actionStates
);
export function selectUsersActionState(action: keyof UsersFeatureState['actionStates']) {
  return createDistinctSelector(selectUsersActionStates, state => state[action]);
}

export function selectUser(id: string) {
  return createDistinctSelector(selectUsersFeature, state => state.entities[id]);
}

export function selectUserLoginToken(id: string | null | undefined) {
  return createDistinctSelector(selectUsersFeature, state =>
    id ? state.entities[id]?.loginToken : undefined
  );
}
