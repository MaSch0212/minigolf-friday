export type Environment = {
  getProviders: () => unknown[];
  authenticationRequired: boolean;
};
