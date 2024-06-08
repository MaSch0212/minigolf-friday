import { Directive, EventEmitter, HostBinding, Output } from '@angular/core';

@Directive({
  selector: '(enterPressed)',
  standalone: true,
})
export class OnEnterDirective {
  @Output()
  public enterPressed = new EventEmitter<void>();

  @HostBinding('keydown')
  public onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' || event.key === 'NumpadEnter') {
      this.enterPressed.emit();
    }
  }
}
