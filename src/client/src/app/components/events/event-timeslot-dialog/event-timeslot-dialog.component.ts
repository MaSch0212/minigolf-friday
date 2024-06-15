import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal,
  untracked,
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
import {
  addEventTimeslotAction,
  selectEventsActionState,
  updateEventTimeslotAction,
} from '../../../+state/events';
import { loadMapsAction, mapSelectors } from '../../../+state/maps';
import { ErrorTextDirective } from '../../../directives/error-text.directive';
import { Event, EventTimeslot } from '../../../models/parsed-models';
import { TranslateService } from '../../../services/translate.service';
import {
  compareTimes,
  dateWithTime,
  getTimeFromDate,
  timeToString,
} from '../../../utils/date.utils';
import { selectSignal } from '../../../utils/ngrx.utils';
import { hasTouchScreen } from '../../../utils/user-agent.utils';

@Component({
  selector: 'app-event-timeslot-dialog',
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
  templateUrl: './event-timeslot-dialog.component.html',
  styleUrl: './event-timeslot-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EventTimeslotDialogComponent {
  private readonly _store = inject(Store);
  private readonly _formBuilder = inject(FormBuilder);

  public readonly event = input.required<Event>();
  public readonly timeslot = input<EventTimeslot | null>(null);

  protected readonly translations = inject(TranslateService).translations;
  protected readonly hasTouchScreen = hasTouchScreen;

  protected readonly actionState = selectSignal(
    computed(() => selectEventsActionState(this.timeslot() ? 'updateTimeslot' : 'addTimeslot'))
  );
  protected readonly hasFailed = computed(() => hasActionFailed(this.actionState()));
  protected readonly isBusy = computed(() => isActionBusy(this.actionState()));
  protected readonly visible = signal(false);
  protected readonly maps = this._store.selectSignal(mapSelectors.selectAll);

  protected readonly form = this._formBuilder.group({
    time: this._formBuilder.control<Date | null>(null, [
      Validators.required,
      this.getTimeValidator(),
    ]),
    mapId: this._formBuilder.control<string | null>(null),
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
          this.form.controls.time.enable();
        }

        const timeslot = untracked(() => this.timeslot());
        if (this.visible() && timeslot) {
          console.log('timeslot', timeslot);
          untracked(() =>
            this.form.setValue({
              time: dateWithTime(new Date(), timeslot.time),
              mapId: timeslot.mapId ?? null,
              isFallbackAllowed: timeslot.isFallbackAllowed,
            })
          );
          this.form.controls.time.disable();
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
    this._store.dispatch(loadMapsAction({ reload: false }));

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
    const timeslot = this.timeslot();

    if (timeslot) {
      this._store.dispatch(
        updateEventTimeslotAction({
          eventId: event.id,
          timeslotId: timeslot.id,
          changes: {
            mapId: mapId ?? null,
            isFallbackAllowed: !!isFallbackAllowed,
          },
        })
      );
    } else {
      if (!time) return;
      this._store.dispatch(
        addEventTimeslotAction({
          eventId: event.id,
          time: getTimeFromDate(time),
          mapId: mapId ?? null,
          isFallbackAllowed: !!isFallbackAllowed,
        })
      );
    }
  }

  private getTimeValidator(): ValidatorFn {
    return control => {
      let event: Event;
      try {
        event = this.event();
      } catch {
        return null;
      }

      if (!(control.value instanceof Date)) return null;
      const time = getTimeFromDate(control.value);

      if (
        event.timeslots.some(
          t =>
            timeToString(t.time, 'minutes') === timeToString(time, 'minutes') &&
            t.id !== this.timeslot()?.id
        )
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
