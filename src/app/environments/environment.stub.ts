import { Environment } from './environment.type';
import { provideStubs } from '../stubs';

export const environment: Environment = {
  getProviders: () => [provideStubs()],
  authenticationRequired: false,
};
