import { Pipe, PipeTransform } from '@angular/core';

export function interpolate(
  value: string,
  params: Record<string, unknown> | null | undefined
): string {
  if (!params) return value;

  const placeholderRegex = /{{\s*([\w.]+)\s*}}/g;

  let match: RegExpExecArray | null;
  let result = '';
  let lastIndex = 0;
  while ((match = placeholderRegex.exec(value))) {
    const [placeholder, key] = match;
    result += value.slice(lastIndex, match.index) + String(getDeepValue(params, key));
    lastIndex = match.index + placeholder.length;
  }

  return result + value.slice(lastIndex);
}

@Pipe({
  name: 'interpolate',
  standalone: true,
})
export class InterpolatePipe implements PipeTransform {
  public transform(value: string, params: Record<string, unknown> | null | undefined): string {
    return interpolate(value, params);
  }
}

function getDeepValue(obj: Record<string, unknown>, path: string): unknown {
  return path
    .split('.')
    .reduce((acc, key) => (acc as Record<string, unknown>)?.[key] as Record<string, unknown>, obj);
}
