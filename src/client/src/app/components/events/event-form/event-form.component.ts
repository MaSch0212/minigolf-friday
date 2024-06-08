import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Output,
  computed,
  effect,
  inject,
  input,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CalendarModule } from 'primeng/calendar';
import { MessagesModule } from 'primeng/messages';

import { ActionState, hasActionFailed } from '../../../+state/action-state';
import { ErrorTextDirective } from '../../../directives/error-text.directive';
import { TranslateService } from '../../../services/translate.service';
import { hasTouchScreen } from '../../../utils/user-agent.utils';

@Component({
  selector: 'app-event-form',
  standalone: true,
  imports: [CalendarModule, ErrorTextDirective, MessagesModule, ReactiveFormsModule],
  templateUrl: './event-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EventFormComponent {
  private readonly _formBuilder = inject(FormBuilder);
  protected readonly translations = inject(TranslateService).translations;

  public readonly actionState = input.required<ActionState>();
  public readonly value = input<{ date: Date; registrationDeadline: Date }>();

  protected readonly hasFailed = computed(() => hasActionFailed(this.actionState()));
  protected readonly hasTouchScreen = hasTouchScreen;

  @Output()
  public readonly submitted = new EventEmitter<{ date: Date; registrationDeadline: Date }>();

  protected readonly form = this._formBuilder.group({
    date: this._formBuilder.control<Date | null>(null, Validators.required),
    registrationDeadline: this._formBuilder.control<Date | null>(null, Validators.required),
  });

  constructor() {
    effect(() => {
      const value = this.value();
      if (value) {
        this.form.setValue(value);
      }
    });

    this.form.controls.date.valueChanges.pipe(takeUntilDestroyed()).subscribe(date => {
      if (date !== null && this.form.controls.registrationDeadline.value === null) {
        const deadline = new Date(date.getTime());
        deadline.setHours(19, 0);
        this.form.controls.registrationDeadline.setValue(deadline);
      }
    });
  }

  public reset() {
    this.form.reset();
  }

  public submit() {
    if (!this.form.valid) {
      this.form.markAllAsTouched();
      return;
    }

    const { date, registrationDeadline } = this.form.value;
    if (!date || !registrationDeadline) return;

    this.submitted.emit({ date, registrationDeadline });
  }
}
