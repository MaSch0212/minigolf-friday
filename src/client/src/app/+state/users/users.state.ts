import { EntityState, createEntityAdapter } from '@ngrx/entity';

import { User } from '../../models/parsed-models';
import { ActionState, initialActionState } from '../action-state';

export type UsersFeatureState = EntityState<User> & {
  actionStates: {
    load: ActionState;
    add: ActionState;
    update: ActionState;
    remove: ActionState;
    loadLoginToken: ActionState;
  };
};

export const userEntityAdapter = createEntityAdapter<User>({
  selectId: user => user.id,
  sortComparer: (a, b) => a.alias.localeCompare(b.alias),
});

export const initialUsersFeatureState: UsersFeatureState = userEntityAdapter.getInitialState({
  actionStates: {
    load: initialActionState,
    add: initialActionState,
    update: initialActionState,
    remove: initialActionState,
    loadLoginToken: initialActionState,
  },
});
