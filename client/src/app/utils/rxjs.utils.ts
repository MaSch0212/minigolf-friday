import { DestroyRef, inject } from '@angular/core';
import { OperatorFunction, Unsubscribable, map } from 'rxjs';
import { z } from 'zod';

export function autoDestroy(unsubscribable: Unsubscribable) {
  const destoryRef = inject(DestroyRef);
  destoryRef.onDestroy(() => {
    unsubscribable.unsubscribe();
  });
}

export function mapUsingZod<T>(schema: z.ZodType<T, any, any>): OperatorFunction<unknown, T>;
export function mapUsingZod<T, U>(
  schema: z.ZodType<T, any, any>,
  mapFn: (x: T) => U
): OperatorFunction<unknown, U>;
export function mapUsingZod<T, U>(
  schema: z.ZodType<T, any, any>,
  mapFn?: (x: T) => U
): OperatorFunction<unknown, T | U> {
  return map(x => (mapFn ? mapFn(schema.parse(x)) : schema.parse(x)));
}
