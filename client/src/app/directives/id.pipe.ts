import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'id',
  standalone: true,
})
export class IdPipe implements PipeTransform {
  public transform(value: string): string {
    const id = Math.random().toString(36).substring(2, 9);
    return `${value}-${id}`;
  }
}
