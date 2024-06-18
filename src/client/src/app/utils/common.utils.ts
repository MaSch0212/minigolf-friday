export function notNullish<T>(value: T): value is NonNullable<T> {
  return value != null;
}

export type RemoveUndefinedProperties<T extends object> = {
  [K in keyof T as undefined extends T[K] ? never : K]: T[K];
} & { [K in keyof T as undefined extends T[K] ? K : never]?: T[K] };
export function removeUndefinedProperties<T extends object>(obj: T): RemoveUndefinedProperties<T> {
  return Object.fromEntries(Object.entries(obj).filter(([, value]) => value !== undefined)) as any;
}

export function isEmptyObject(
  obj: object,
  options: { ignoreUndefinedProperties: true }
): obj is Record<string, undefined>;
export function isEmptyObject(
  obj: object,
  options?: { ignoreUndefinedProperties: boolean }
): obj is Record<string, never>;
export function isEmptyObject(
  obj: object,
  options?: { ignoreUndefinedProperties: boolean }
): boolean {
  return (
    Object.keys(obj).length === 0 ||
    (!!options?.ignoreUndefinedProperties && Object.values(obj).every(value => value === undefined))
  );
}

export function throwExp(message: string): never {
  throw new Error(message);
}

export function deepClone<T>(obj: T): T {
  return JSON.parse(JSON.stringify(obj));
}

export function ifTruthy<T, R>(value: T, fn: (value: NonNullable<T>) => R, elseValue: R): R;
export function ifTruthy<T, R>(
  value: T,
  fn: (value: NonNullable<T>) => R,
  elseFn: (value: T) => R
): R;
export function ifTruthy<T, R>(value: T, fn: (value: NonNullable<T>) => R): R | undefined;
export function ifTruthy<T, R>(
  value: T,
  fn: (value: NonNullable<T>) => R,
  elseFn?: ((value: T) => R) | R
): R | undefined {
  if (value) return fn(value);
  if (typeof elseFn === 'function') return (elseFn as any)(value);
  return elseFn;
}
