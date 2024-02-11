import { EntityState, createEntityAdapter } from '@ngrx/entity';

import { User } from '../../models/user';
import { ActionState, initialActionState } from '../action-state';

export type UsersFeatureState = EntityState<User> & {
  actionStates: {
    load: ActionState;
    loadByIds: ActionState;
  };
};

export const userEntityAdapter = createEntityAdapter<User>({
  selectId: user => user.id,
  sortComparer: (a, b) => a.name.localeCompare(b.name),
});

export const initialUsersFeatureState: UsersFeatureState = userEntityAdapter.getInitialState({
  actionStates: {
    load: initialActionState,
    loadByIds: initialActionState,
  },
});
