export type Environment = {
  getProviders: () => unknown[];
  authenticationRequired: boolean;
  version: string;
};
