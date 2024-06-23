import { CommonModule, formatDate } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Actions, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { AccordionModule } from 'primeng/accordion';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessagesModule } from 'primeng/messages';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { filter, map, timer } from 'rxjs';

import { hasActionFailed, isActionBusy } from '../../../+state/action-state';
import {
  buildEventInstancesAction,
  removeEventAction,
  selectEvent,
  selectEventsActionState,
  startEventAction,
  commitEventAction,
} from '../../../+state/events';
import { keepEventLoaded } from '../../../+state/events/events.utils';
import { mapSelectors } from '../../../+state/maps';
import { keepMapsLoaded } from '../../../+state/maps/maps.utils';
import { keepUsersLoaded, userSelectors } from '../../../+state/users';
import { InterpolatePipe, interpolate } from '../../../directives/interpolate.pipe';
import { TranslateService } from '../../../services/translate.service';
import { ifTruthy } from '../../../utils/common.utils';
import { compareTimes } from '../../../utils/date.utils';
import { errorToastEffect, selectSignal } from '../../../utils/ngrx.utils';
import { EventFormComponent } from '../event-form/event-form.component';
import { EventTimeslotDialogComponent } from '../event-timeslot-dialog/event-timeslot-dialog.component';

@Component({
  selector: 'app-event-details',
  standalone: true,
  imports: [
    AccordionModule,
    ButtonModule,
    CardModule,
    CommonModule,
    EventTimeslotDialogComponent,
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
  private readonly now = toSignal(timer(1000).pipe(map(() => Date.now())), {
    initialValue: Date.now(),
  });

  private readonly eventId = toSignal(this._activatedRoute.params.pipe(map(data => data['id'])));
  private readonly actionState = selectSignal(selectEventsActionState('loadOne'));
  private readonly startActionState = selectSignal(selectEventsActionState('start'));
  private readonly buildActionState = selectSignal(selectEventsActionState('buildInstances'));

  protected readonly isBusy = computed(() => isActionBusy(this.actionState()));
  protected readonly hasFailed = computed(() => hasActionFailed(this.actionState(), [404]));
  protected readonly isStartBusy = computed(() => isActionBusy(this.startActionState()));
  protected readonly isBuildBusy = computed(() => isActionBusy(this.buildActionState()));
  protected readonly event = selectSignal(computed(() => selectEvent(this.eventId())));
  protected readonly timeslots = computed(() =>
    [...(this.event()?.timeslots ?? [])].sort((a, b) => compareTimes(a.time, b.time))
  );
  protected readonly maps = selectSignal(mapSelectors.selectEntities);
  protected readonly allUsers = selectSignal(userSelectors.selectEntities);
  protected readonly hasInstances = computed(() =>
    this.timeslots().some(x => x.instances.length > 0)
  );
  protected readonly allTimeslotsHaveMaps = computed(
    () =>
      !this.event()
        ?.timeslots.filter(x => x.instances.length > 0)
        .some(x => x.mapId === null || x.mapId === undefined)
  );

  protected readonly canBuildInstances = computed(() =>
    ifTruthy(this.event(), event => event.registrationDeadline.getTime() < this.now(), false)
  );
  protected readonly canStart = computed(
    () => this.canBuildInstances() && this.event() && !this.event()?.startedAt
  );

  protected readonly canCommit = computed(
    () =>
      this.event() &&
      this.event()?.staged &&
      this.event()?.timeslots &&
      this.event()!.timeslots.length > 0
  );

  protected readonly allowToStart = computed(
    () => this.hasInstances() && this.allTimeslotsHaveMaps()
  );

  constructor() {
    keepMapsLoaded();
    keepUsersLoaded();
    keepEventLoaded(this.eventId);

    errorToastEffect(this.translations.events_error_delete, selectEventsActionState('remove'));
    errorToastEffect(this.translations.events_error_start, this.startActionState);
    errorToastEffect(this.translations.events_error_buildGroups, this.buildActionState);

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
      message: interpolate(this.translations.events_deleteDialog_text(), {
        date: formatDate(this.event()!.date, 'mediumDate', this.locale()),
      }),
      acceptLabel: this.translations.shared_delete(),
      acceptButtonStyleClass: 'p-button-danger',
      acceptIcon: 'p-button-icon-left i-[mdi--delete]',
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

  protected startEvent() {
    this._confirmationService.confirm({
      header: this.translations.events_startDialog_title(),
      message: interpolate(this.translations.events_startDialog_text(), {
        date: formatDate(this.event()!.date, 'mediumDate', this.locale()),
      }),
      acceptLabel: this.translations.shared_start(),
      acceptButtonStyleClass: 'p-button-success',
      acceptIcon: 'p-button-icon-left i-[mdi--play]',
      rejectLabel: this.translations.shared_cancel(),
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this._store.dispatch(
          startEventAction({
            eventId: this.eventId()!,
          })
        );
      },
    });
  }

  protected commitEvent() {
    this._confirmationService.confirm({
      header: this.translations.events_commitDialog_title(),
      message: interpolate(this.translations.events_commitDialog_text(), {
        date: formatDate(this.event()!.date, 'mediumDate', this.locale()),
      }),
      acceptLabel: this.translations.shared_commit(),
      acceptButtonStyleClass: 'p-button-success',
      acceptIcon: 'p-button-icon-left i-[mdi--play]',
      rejectLabel: this.translations.shared_cancel(),
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this._store.dispatch(
          commitEventAction({
            eventId: this.eventId()!,
            commit: true,
          })
        );
      },
    });
  }

  protected buildInstances() {
    this._store.dispatch(buildEventInstancesAction({ eventId: this.eventId() }));
  }
}
