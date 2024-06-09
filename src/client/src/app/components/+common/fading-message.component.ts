import {
  ChangeDetectionStrategy,
  Component,
  effect,
  HostBinding,
  input,
  OnDestroy,
  signal,
  untracked,
} from '@angular/core';

@Component({
  standalone: true,
  selector: 'app-fading-message',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<ng-content></ng-content>`,
  styles: `
    :host {
      display: block;
      opacity: 0;
      transition-property: opacity;
      transition-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
      transition-duration: var(--fm-fade-out-duration);

      &.show {
        opacity: 1;
        transition-duration: var(--fm-fade-in-duration);
      }
    }
  `,
})
export class FadingMessageComponent implements OnDestroy {
  private _lastTimeout?: ReturnType<typeof setTimeout>;
  private readonly _isShowing = signal(false);

  public readonly fadeInDuration = input<number>(150);
  public readonly fadeOutDuration = input<number>(3000);
  public readonly showTrigger = input<boolean | null | undefined>();

  @HostBinding('style.--fm-fade-in-duration')
  protected get fadeInDurationValue() {
    return `${this.fadeInDuration()}ms`;
  }

  @HostBinding('style.--fm-fade-out-duration')
  protected get fadeOutDurationValue() {
    return `${this.fadeOutDuration()}ms`;
  }

  @HostBinding('class.show')
  protected get hasShowClass() {
    return this._isShowing();
  }

  constructor() {
    effect(
      () => {
        if (this.showTrigger()) {
          untracked(() => this.show());
        }
      },
      { allowSignalWrites: true }
    );
  }

  public show() {
    if (this._isShowing()) return;
    this._isShowing.set(true);

    this._lastTimeout = setTimeout(() => this._isShowing.set(false), this.fadeInDuration());
  }

  public ngOnDestroy(): void {
    clearTimeout(this._lastTimeout);
  }
}
