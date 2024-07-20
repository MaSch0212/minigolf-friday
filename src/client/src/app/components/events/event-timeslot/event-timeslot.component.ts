import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Actions, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { AccordionModule } from 'primeng/accordion';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DropdownModule } from 'primeng/dropdown';
import { MessagesModule } from 'primeng/messages';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { map } from 'rxjs';

import { isActionBusy, hasActionFailed } from '../../../+state/action-state';
import {
  addPlayerToEventPreconfigurationAction,
  removeEventPreconfigAction,
  removeEventTimeslotAction,
  removePlayerFromPreconfigAction,
  selectEvent,
  selectEventTimeslot,
  selectEventsActionState,
} from '../../../+state/events';
import { addEventPreconfigAction } from '../../../+state/events/actions/add-event-preconfig.action';
import { keepEventLoaded } from '../../../+state/events/events.utils';
import { mapSelectors } from '../../../+state/maps';
import { keepMapsLoaded } from '../../../+state/maps/maps.utils';
import {
  keepUsersLoaded,
  loadUsersAction,
  selectUsersActionState,
  userSelectors,
} from '../../../+state/users';
import { EventInstancePreconfiguration, User } from '../../../models/parsed-models';
import { TranslateService } from '../../../services/translate.service';
import { ifTruthy, isNullish } from '../../../utils/common.utils';
import { dateWithTime, timeToString } from '../../../utils/date.utils';
import { errorToastEffect, selectSignal } from '../../../utils/ngrx.utils';
import { EventTimeslotDialogComponent } from '../event-timeslot-dialog/event-timeslot-dialog.component';

function asString(value: unknown): string | null {
  return typeof value === 'string' ? value : null;
}

@Component({
  selector: 'app-event-timeslot',
  standalone: true,
  imports: [
    AccordionModule,
    ButtonModule,
    CardModule,
    CommonModule,
    DropdownModule,
    EventTimeslotDialogComponent,
    FormsModule,
    MessagesModule,
    ProgressSpinnerModule,
    TooltipModule,
  ],
  templateUrl: './event-timeslot.component.html',
  styleUrl: './event-timeslot.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EventTimeslotComponent {
  private readonly _store = inject(Store);
  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _translateService = inject(TranslateService);
  private readonly _confirmationService = inject(ConfirmationService);

  protected readonly translations = this._translateService.translations;
  protected readonly locale = this._translateService.language;

  private readonly eventId = toSignal(
    this._activatedRoute.params.pipe(map(data => asString(data['eventId'])))
  );
  private readonly timeslotId = toSignal(
    this._activatedRoute.params.pipe(map(data => asString(data['timeslotId'])))
  );
  private readonly actionState = selectSignal(selectEventsActionState('loadOne'));
  private readonly loadUsersActionState = selectSignal(selectUsersActionState('load'));
  private readonly addPreconfigActionState = selectSignal(selectEventsActionState('addPreconfig'));
  private readonly addPlayerToPreconfigActionState = selectSignal(
    selectEventsActionState('addPlayerToPreconfig')
  );

  protected readonly isBusy = computed(() => isActionBusy(this.actionState()));
  protected readonly hasFailed = computed(() => hasActionFailed(this.actionState(), [404]));
  protected readonly isUsersBusy = computed(() => isActionBusy(this.loadUsersActionState()));
  protected readonly hasUsersFailed = computed(() => hasActionFailed(this.loadUsersActionState()));
  protected readonly isAddPreconfigBusy = computed(() =>
    isActionBusy(this.addPreconfigActionState())
  );
  protected readonly isAddPlayerToPreconfigBusy = computed(() =>
    isActionBusy(this.addPlayerToPreconfigActionState())
  );

  protected readonly event = selectSignal(computed(() => selectEvent(this.eventId())));
  protected readonly timeslot = selectSignal(
    computed(() => selectEventTimeslot(this.eventId(), this.timeslotId()))
  );
  protected readonly maps = selectSignal(mapSelectors.selectEntities);
  protected readonly allUsers = selectSignal(userSelectors.selectEntities);
  protected readonly players = computed(() =>
    ifTruthy(
      this.timeslot(),
      timeslot =>
        timeslot.playerIds
          .map<Partial<User> & { id: string }>(x => this.allUsers()[x] ?? { id: x })
          .sort((a, b) => (a?.alias ?? '').localeCompare(b?.alias ?? '')),
      []
    )
  );
  protected readonly preconfigPlayerOptions = computed(() =>
    this.players().filter(
      x => !this.timeslot()?.preconfigurations.some(p => p.playerIds.includes(x.id))
    )
  );
  protected readonly dateTime = computed(() => {
    const event = this.event();
    const timeslot = this.timeslot();
    return event && timeslot ? dateWithTime(event.date, timeslot.time) : null;
  });

  constructor() {
    const actions$ = inject(Actions);

    keepMapsLoaded();
    keepUsersLoaded();
    keepEventLoaded(this.eventId);

    errorToastEffect(
      this.translations.events_error_addPlayerToPreconfig,
      this.addPlayerToPreconfigActionState
    );

    errorToastEffect(
      this.translations.events_error_deletePreconfig,
      selectEventsActionState('removePreconfig')
    );

    errorToastEffect(
      this.translations.events_error_removePlayerFromPreconfig,
      selectEventsActionState('removePlayerFromPreconfig')
    );

    const removeTimeslotActionState = selectSignal(selectEventsActionState('removeTimeslot'));
    errorToastEffect(this.translations.events_error_deleteTimeslot, removeTimeslotActionState);

    actions$
      .pipe(takeUntilDestroyed(), ofType(removeEventTimeslotAction.success))
      .subscribe(() => this.navigateBack());
  }

  protected navigateBack() {
    this._router.navigate(this.event() ? ['../..'] : ['../../..'], {
      relativeTo: this._activatedRoute,
    });
  }

  protected reloadUsers() {
    if (hasActionFailed(this.loadUsersActionState())) {
      this._store.dispatch(loadUsersAction({ reload: true }));
    }
  }

  protected addPreconfig() {
    const eventId = this.eventId();
    const timeslotId = this.timeslotId();
    if (isNullish(eventId) || isNullish(timeslotId)) return;

    this._store.dispatch(addEventPreconfigAction({ eventId, timeslotId }));
  }

  protected removePreconfig(preconfig: EventInstancePreconfiguration) {
    const eventId = this.eventId();
    const timeslotId = this.timeslotId();
    if (isNullish(eventId) || isNullish(timeslotId)) return;

    this._confirmationService.confirm({
      header: this.translations.events_deletePreconfigDialog_title(),
      message: this.translations.events_deletePreconfigDialog_text(preconfig),
      acceptLabel: this.translations.shared_delete(),
      acceptButtonStyleClass: 'p-button-danger',
      acceptIcon: 'p-button-icon-left i-[mdi--delete]',
      rejectLabel: this.translations.shared_cancel(),
      rejectButtonStyleClass: 'p-button-text',
      accept: () =>
        this._store.dispatch(
          removeEventPreconfigAction({ eventId, timeslotId, preconfigId: preconfig.id })
        ),
    });
  }

  protected addPlayerToPreconfig(preconfigId: string, userId: string) {
    const eventId = this.eventId();
    const timeslotId = this.timeslotId();
    if (isNullish(eventId) || isNullish(timeslotId)) return;

    this._store.dispatch(
      addPlayerToEventPreconfigurationAction({
        eventId,
        timeslotId,
        preconfigId,
        playerId: userId,
      })
    );
  }

  protected removePlayerFromPreconfig(preconfigId: string, userId: string) {
    const eventId = this.eventId();
    const timeslotId = this.timeslotId();
    if (isNullish(eventId) || isNullish(timeslotId)) return;

    this._store.dispatch(
      removePlayerFromPreconfigAction({
        eventId,
        timeslotId,
        preconfigId,
        playerId: userId,
      })
    );
  }

  protected deleteTimeslot() {
    const eventId = this.eventId();
    const timeslot = this.timeslot();
    if (isNullish(eventId) || isNullish(timeslot)) return;

    this._confirmationService.confirm({
      header: this.translations.events_deleteTimeslotDialog_title(),
      message: this.translations.events_deleteTimeslotDialog_text({
        time: timeToString(timeslot.time, 'minutes'),
      }),
      acceptLabel: this.translations.shared_delete(),
      acceptButtonStyleClass: 'p-button-danger',
      acceptIcon: 'p-button-icon-left i-[mdi--delete]',
      rejectLabel: this.translations.shared_cancel(),
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this._store.dispatch(removeEventTimeslotAction({ eventId, timeslotId: timeslot.id }));
      },
    });
  }
}
