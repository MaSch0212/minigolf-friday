import { Pipe, PipeTransform } from '@angular/core';

export function interpolate(value: string, params: Record<string, unknown>): string {
  const placeholderRegex = /{{\s*([\w.]+)\s*}}/g;

  let match: RegExpExecArray | null;
  let result = '';
  let lastIndex = 0;
  while ((match = placeholderRegex.exec(value))) {
    const [placeholder, key] = match;
    result += value.slice(lastIndex, match.index) + String(params[key]);
    lastIndex = match.index + placeholder.length;
  }

  return result + value.slice(lastIndex);
}

@Pipe({
  name: 'interpolate',
  standalone: true,
})
export class InterpolatePipe implements PipeTransform {
  public transform(value: string, params: Record<string, unknown>): string {
    return interpolate(value, params);
  }
}
