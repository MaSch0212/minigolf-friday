import { Directive, effect, inject, input, OnDestroy } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';

@Directive({
  selector: '[resetNgModel]',
  standalone: true,
})
export class ResetNgModelDirective implements OnDestroy {
  private readonly _valueAccessor = inject(NG_VALUE_ACCESSOR);
  private _subscription?: Subscription;

  public readonly resetNgModel = input.required<Observable<unknown>>();
  public readonly ngModel = input<unknown>();

  constructor() {
    effect(() => {
      this._subscription = this.resetNgModel().subscribe(() => {
        this._valueAccessor.forEach(x => x.writeValue(this.ngModel()));
      });
    });
  }

  public ngOnDestroy(): void {
    this._subscription?.unsubscribe();
  }
}
