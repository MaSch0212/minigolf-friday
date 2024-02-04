import {
  AfterViewInit,
  Directive,
  ElementRef,
  Injector,
  effect,
  inject,
  input,
} from '@angular/core';

import { interpolate } from './interpolate.pipe';
import { TranslateService } from '../services/translate.service';

@Directive({
  standalone: true,
  selector: '[formControlErrors]',
})
export class ErrorTextDirective implements AfterViewInit {
  private readonly _element = inject(ElementRef);
  private readonly _injector = inject(Injector);
  private readonly _translateService = inject(TranslateService);

  public readonly formControlErrors = input<Record<string, unknown> | null>(null);

  public ngAfterViewInit(): void {
    const element = this._element.nativeElement as HTMLElement;
    element.classList.add('error');

    effect(
      () => {
        const errors = this.formControlErrors();
        const firstError = !errors ? undefined : Object.entries(errors)[0];
        if (firstError) {
          const [errorKey, errorValue] = firstError;
          element.textContent = interpolate(
            this._translateService.translate(`validation_${errorKey}`),
            errorValue as Record<string, unknown>
          );
        } else {
          element.textContent = '';
        }
      },
      { injector: this._injector }
    );
  }
}
