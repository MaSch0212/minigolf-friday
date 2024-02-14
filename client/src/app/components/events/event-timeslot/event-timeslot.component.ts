import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { AccordionModule } from 'primeng/accordion';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DropdownModule } from 'primeng/dropdown';
import { MessagesModule } from 'primeng/messages';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { map } from 'rxjs';

import { isActionBusy, hasActionFailed, hasActionSucceeded } from '../../../+state/action-state';
import {
  addPlayerToEventPreconfigurationAction,
  loadEventAction,
  removeEventPreconfigAction,
  removeEventTimeslotAction,
  removePlayerFromPreconfigAction,
  selectEvent,
  selectEventTimeslot,
  selectEventsActionState,
} from '../../../+state/events';
import { addEventPreconfigAction } from '../../../+state/events/actions/add-event-preconfig.action';
import { loadUsersAction, selectUsersActionState, userSelectors } from '../../../+state/users';
import { loadUsersByIdAction } from '../../../+state/users/actions/load-users-by-id.action';
import { InterpolatePipe, interpolate } from '../../../directives/interpolate.pipe';
import {
  MinigolfEventInstancePreconfiguration,
  MinigolfEventTimeslot,
} from '../../../models/event';
import { TranslateService } from '../../../services/translate.service';
import { dateWithTime, timeToString } from '../../../utils/date.utils';
import { errorToastEffect, selectSignal } from '../../../utils/ngrx.utils';

@Component({
  selector: 'app-event-timeslot',
  standalone: true,
  imports: [
    AccordionModule,
    ButtonModule,
    CardModule,
    CommonModule,
    DropdownModule,
    FormsModule,
    InterpolatePipe,
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
  private readonly _messageService = inject(MessageService);
  private readonly _confirmationService = inject(ConfirmationService);

  protected readonly translations = this._translateService.translations;
  protected readonly locale = this._translateService.language;

  private readonly eventId = toSignal(
    this._activatedRoute.params.pipe(map(data => data['eventId']))
  );
  private readonly timeslotId = toSignal(
    this._activatedRoute.params.pipe(map(data => data['timeslotId']))
  );
  private readonly actionState = selectSignal(selectEventsActionState('loadOne'));
  private readonly loadUsersActionState = selectSignal(selectUsersActionState('load'));
  private readonly loadUsersByIdActionState = selectSignal(selectUsersActionState('loadByIds'));
  private readonly addPreconfigActionState = selectSignal(selectEventsActionState('addPreconfig'));
  private readonly addPlayerToPreconfigActionState = selectSignal(
    selectEventsActionState('addPlayerToPreconfig')
  );

  protected readonly isBusy = computed(() => isActionBusy(this.actionState()));
  protected readonly hasFailed = computed(() => hasActionFailed(this.actionState(), [404]));
  protected readonly isUsersBusy = computed(
    () => isActionBusy(this.loadUsersActionState()) || isActionBusy(this.loadUsersByIdActionState())
  );
  protected readonly hasUsersFailed = computed(
    () =>
      hasActionFailed(this.loadUsersByIdActionState()) ||
      hasActionFailed(this.loadUsersActionState())
  );
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
  protected readonly allUsers = selectSignal(userSelectors.selectEntities);
  protected readonly dateTime = computed(() => {
    const event = this.event();
    const timeslot = this.timeslot();
    return event && timeslot ? dateWithTime(event.date, timeslot.time) : null;
  });

  constructor() {
    this._store.dispatch(loadUsersAction({ reload: false }));

    effect(
      () => {
        if (!this.event()) {
          this._store.dispatch(loadEventAction({ eventId: this.eventId() }));
        } else if (!this.timeslot()) {
          this.navigateBack();
        }
      },
      { allowSignalWrites: true }
    );

    effect(
      () => {
        const timeslot = this.timeslot();
        if (timeslot && hasActionSucceeded(this.loadUsersActionState())) {
          this.loadUsersFromTimeslot(timeslot);
        }
      },
      { allowSignalWrites: true }
    );

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
    if (hasActionFailed(this.loadUsersByIdActionState())) {
      const timeslot = this.timeslot();
      if (timeslot) {
        this.loadUsersFromTimeslot(timeslot);
      }
    }
  }

  protected addPreconfig() {
    this._store.dispatch(
      addEventPreconfigAction({
        eventId: this.eventId()!,
        timeslotId: this.timeslotId()!,
      })
    );
  }

  protected removePreconfig(preconfig: MinigolfEventInstancePreconfiguration) {
    this._confirmationService.confirm({
      header: this.translations.events_deletePreconfigDialog_title(),
      message: interpolate(this.translations.events_deletePreconfigDialog_text(), preconfig),
      acceptLabel: this.translations.shared_delete(),
      acceptButtonStyleClass: 'p-button-danger',
      acceptIcon: 'p-button-icon-left i-[mdi--delete-outline]',
      rejectLabel: this.translations.shared_cancel(),
      rejectButtonStyleClass: 'p-button-text',
      accept: () =>
        this._store.dispatch(
          removeEventPreconfigAction({
            eventId: this.eventId()!,
            timeslotId: this.timeslotId()!,
            preconfigId: preconfig.id,
          })
        ),
    });
  }

  protected addPlayerToPreconfig(preconfigId: string, userId: string) {
    this._store.dispatch(
      addPlayerToEventPreconfigurationAction({
        eventId: this.eventId()!,
        timeslotId: this.timeslotId()!,
        preconfigId,
        playerId: userId,
      })
    );
  }

  protected removePlayerFromPreconfig(preconfigId: string, userId: string) {
    this._store.dispatch(
      removePlayerFromPreconfigAction({
        eventId: this.eventId()!,
        timeslotId: this.timeslotId()!,
        preconfigId,
        playerId: userId,
      })
    );
  }

  protected deleteTimeslot() {
    this._confirmationService.confirm({
      header: this.translations.events_deleteTimeslotDialog_title(),
      message: interpolate(this.translations.events_deleteTimeslotDialog_text(), { time: timeToString(this.timeslot()!.time, 'minutes') }),
      acceptLabel: this.translations.shared_delete(),
      acceptButtonStyleClass: 'p-button-danger',
      acceptIcon: 'p-button-icon-left i-[mdi--delete-outline]',
      rejectLabel: this.translations.shared_cancel(),
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this._store.dispatch(
          removeEventTimeslotAction({
            eventId: this.eventId()!,
            timeslotId: this.timeslotId()!,
          })
        );
      },
    });
  }

  protected getAllPlayersExcept(playerIds: readonly string[]) {
    return Object.values(this.allUsers()).filter(user => user && !playerIds.includes(user.id));
  }

  private loadUsersFromTimeslot(timeslot: MinigolfEventTimeslot) {
    const allUserIds = [
      ...timeslot.playerIds,
      ...timeslot.preconfigurations.flatMap(x => x.playerIds),
    ];
    this._store.dispatch(loadUsersByIdAction({ userIds: allUserIds }));
  }

  protected testEvent(e: unknown) {
    console.log(e);
  }
}
