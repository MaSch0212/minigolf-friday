import { loadUsersByIdEffects } from './actions/load-users-by-id.action';
import { loadUsersEffects } from './actions/load-users.action';
import { Effects } from '../utils';

export const usersFeatureEffects: Effects[] = [loadUsersByIdEffects, loadUsersEffects];
