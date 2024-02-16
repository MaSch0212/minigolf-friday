import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { ButtonModule } from 'primeng/button';
import { CalendarModule } from 'primeng/calendar';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { InputSwitchModule } from 'primeng/inputswitch';
import { MessagesModule } from 'primeng/messages';

import { hasActionFailed, isActionBusy } from '../../../+state/action-state';
import { addEventTimeslotAction, selectEventsActionState } from '../../../+state/events';
import { loadMapsAction, mapSelectors, selectMapsLoadState } from '../../../+state/maps';
import { ErrorTextDirective } from '../../../directives/error-text.directive';
import { MinigolfEvent } from '../../../models/event';
import { TranslateService } from '../../../services/translate.service';
import { compareTimes, getTimeFromDate, timeToString } from '../../../utils/date.utils';
import { hasTouchScreen } from '../../../utils/user-agent.utils';

@Component({
  selector: 'app-create-event-timeslot',
  standalone: true,
  imports: [
    DialogModule,
    CalendarModule,
    ButtonModule,
    DropdownModule,
    InputSwitchModule,
    MessagesModule,
    ReactiveFormsModule,
    ErrorTextDirective,
  ],
  templateUrl: './create-event-timeslot.component.html',
  styleUrl: './create-event-timeslot.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateEventTimeslotComponent {
  private readonly _store = inject(Store);
  private readonly _formBuilder = inject(FormBuilder);

  public readonly event = input<MinigolfEvent>();

  protected readonly translations = inject(TranslateService).translations;
  protected readonly hasTouchScreen = hasTouchScreen;

  protected readonly actionState = this._store.selectSignal(selectEventsActionState('addTimeslot'));
  protected readonly hasFailed = computed(() => hasActionFailed(this.actionState()));
  protected readonly isBusy = computed(() => isActionBusy(this.actionState()));
  protected readonly visible = signal(false);
  protected readonly mapsLoadState = this._store.selectSignal(selectMapsLoadState);
  protected readonly maps = this._store.selectSignal(mapSelectors.selectAll);

  protected readonly form = this._formBuilder.group({
    time: this._formBuilder.control<Date | null>(null, [
      Validators.required,
      this.getTimeValidator(),
    ]),
    mapId: this._formBuilder.control<string | null>(null, [Validators.required]),
    isFallbackAllowed: this._formBuilder.control<boolean>(false, { nonNullable: true }),
  });

  constructor() {
    effect(
      () => {
        if (this.actionState().state === 'success') {
          this.visible.set(false);
        }
      },
      { allowSignalWrites: true }
    );

    effect(
      () => {
        if (!this.visible()) {
          this.form.reset();
          this.form.controls.mapId.markAsPristine();
        }
      },
      { allowSignalWrites: true }
    );

    effect(
      () => {
        this.event();
        this.form.controls.time.updateValueAndValidity();
      },
      { allowSignalWrites: true }
    );
  }

  public open() {
    if (!this.mapsLoadState().loaded) {
      this._store.dispatch(loadMapsAction({ reload: false }));
    }

    this.visible.set(true);
  }

  protected submit() {
    if (!this.form.valid) {
      this.form.markAllAsTouched();
      return;
    }

    const event = this.event();
    if (!event) {
      throw new Error('Event is required');
    }

    const { time, mapId, isFallbackAllowed } = this.form.value;
    if (!time || !mapId) return;

    this._store.dispatch(
      addEventTimeslotAction({
        eventId: event.id,
        time: timeToString(getTimeFromDate(time), 'minutes'),
        mapId,
        isFallbackAllowed: !!isFallbackAllowed,
      })
    );
  }

  private getTimeValidator(): ValidatorFn {
    return control => {
      const event = this.event();
      if (!event) return null;

      if (!(control.value instanceof Date)) return null;
      const time = getTimeFromDate(control.value);

      if (
        event.timeslots.some(t => timeToString(t.time, 'minutes') === timeToString(time, 'minutes'))
      ) {
        return { duplicateTimeslot: true };
      }

      if (compareTimes(getTimeFromDate(event.registrationDeadline), time) > 0) {
        return { afterRegistrationDeadline: true };
      }

      return null;
    };
  }
}
