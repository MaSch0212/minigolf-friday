export type Prettify<T> = {
  [K in keyof T]: T[K];
} & {};

export type ChangePropertyTypes<T, U extends Partial<Record<keyof T, unknown>>> = {
  [K in keyof T]: K extends keyof U ? U[K] : T[K];
};
