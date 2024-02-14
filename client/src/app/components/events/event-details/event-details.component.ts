import { CommonModule, formatDate } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Actions, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessagesModule } from 'primeng/messages';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { filter, map } from 'rxjs';

import { hasActionFailed, isActionBusy } from '../../../+state/action-state';
import {
  loadEventAction,
  removeEventAction,
  selectEvent,
  selectEventsActionState,
} from '../../../+state/events';
import { InterpolatePipe, interpolate } from '../../../directives/interpolate.pipe';
import { TranslateService } from '../../../services/translate.service';
import { compareTimes } from '../../../utils/date.utils';
import { errorToastEffect, selectSignal } from '../../../utils/ngrx.utils';
import { CreateEventTimeslotComponent } from '../create-event-timeslot/create-event-timeslot.component';
import { EventFormComponent } from '../event-form/event-form.component';

@Component({
  selector: 'app-event-details',
  standalone: true,
  imports: [
    ButtonModule,
    CardModule,
    CommonModule,
    CreateEventTimeslotComponent,
    EventFormComponent,
    InterpolatePipe,
    MessagesModule,
    ProgressSpinnerModule,
    RouterLink,
    TooltipModule,
  ],
  templateUrl: './event-details.component.html',
  styleUrl: './event-details.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EventDetailsComponent {
  private readonly _store = inject(Store);
  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _translateService = inject(TranslateService);
  private readonly _confirmationService = inject(ConfirmationService);

  protected readonly translations = this._translateService.translations;
  protected readonly locale = this._translateService.language;

  private readonly eventId = toSignal(this._activatedRoute.params.pipe(map(data => data['id'])));
  private readonly actionState = selectSignal(selectEventsActionState('loadOne'));
  protected readonly isBusy = computed(() => isActionBusy(this.actionState()));
  protected readonly hasFailed = computed(() => hasActionFailed(this.actionState(), [404]));
  protected readonly event = selectSignal(computed(() => selectEvent(this.eventId())));
  protected readonly timeslots = computed(() =>
    [...(this.event()?.timeslots ?? [])].sort((a, b) => compareTimes(a.time, b.time))
  );

  constructor() {
    effect(
      () => {
        if (!this.event()) {
          this._store.dispatch(loadEventAction({ eventId: this.eventId() }));
        }
      },
      { allowSignalWrites: true }
    );

    errorToastEffect(this.translations.events_error_delete, selectEventsActionState('remove'));

    const actions$ = inject(Actions);
    actions$
      .pipe(
        ofType(removeEventAction.success),
        filter(({ props }) => props.eventId === this.eventId()),
        takeUntilDestroyed()
      )
      .subscribe(() => this.navigateBack());
  }

  protected navigateBack() {
    this._router.navigate(['..'], { relativeTo: this._activatedRoute });
  }

  protected deleteEvent() {
    this._confirmationService.confirm({
      header: this.translations.events_deleteDialog_title(),
      message: interpolate(this.translations.events_deleteDialog_text(), { date: formatDate(this.event()!.date, 'mediumDate', this.locale()) }),
      acceptLabel: this.translations.shared_delete(),
      acceptButtonStyleClass: 'p-button-danger',
      acceptIcon: 'p-button-icon-left i-[mdi--delete-outline]',
      rejectLabel: this.translations.shared_cancel(),
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this._store.dispatch(
          removeEventAction({
            eventId: this.eventId()!,
          })
        );
      },
    });
  }
}
