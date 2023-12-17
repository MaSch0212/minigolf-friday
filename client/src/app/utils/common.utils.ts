export function notNullish<T>(value: T): value is NonNullable<T> {
  return value != null;
}
