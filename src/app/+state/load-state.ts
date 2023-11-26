export type LoadState<Error> = {
  loading: boolean;
  loaded: boolean;
  error: Error | null | undefined;
};

export const getInitialLoadState: <Error>() => LoadState<Error> = () => ({
  loading: false,
  loaded: false,
  error: undefined,
});
