import { Environment } from './environment.type';

export const environment: Environment = {
  getProviders: () => [],
  authenticationRequired: true,
  version: (document.head.querySelector('meta[name="version"]') as HTMLMetaElement)?.content ?? '',
};
