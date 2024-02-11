export function notNullish<T>(value: T): value is NonNullable<T> {
  return value != null;
}

export type RemoveUndefinedProperties<T extends object> = {
  [K in keyof T as undefined extends T[K] ? never : keyof T]: T[K];
} & { [K in keyof T as undefined extends T[K] ? keyof T : never]?: T[K] };
export function removeUndefinedProperties<T extends object>(obj: T): RemoveUndefinedProperties<T> {
  return Object.fromEntries(Object.entries(obj).filter(([, value]) => value !== undefined)) as any;
}

export function throwExp(message: string): never {
  throw new Error(message);
}

export function deepClone<T>(obj: T): T {
  return JSON.parse(JSON.stringify(obj));
}
