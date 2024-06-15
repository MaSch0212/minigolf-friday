import { Environment } from './environment.type';

const minigolfFriday: { version: string } = (window as any).minigolfFriday;

export const environment: Environment = {
  getProviders: () => [],
  authenticationRequired: true,
  ...minigolfFriday,
};
