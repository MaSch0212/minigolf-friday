import { provideStubs } from './stubs';

export function getAdditionalProviders() {
  return [provideStubs()];
}
