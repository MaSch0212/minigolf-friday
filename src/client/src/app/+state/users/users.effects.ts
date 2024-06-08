import { addUserEffects } from './actions/add-user.action';
import { loadUserLoginTokenEffects } from './actions/load-user-login-token.action';
import { loadUsersEffects } from './actions/load-users.action';
import { removeUserEffects } from './actions/remove-user.action';
import { updateUserEffects } from './actions/update-user.action';
import { Effects } from '../utils';

export const usersFeatureEffects: Effects[] = [
  addUserEffects,
  loadUserLoginTokenEffects,
  loadUsersEffects,
  removeUserEffects,
  updateUserEffects,
];
