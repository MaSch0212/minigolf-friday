import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { AccordionModule } from 'primeng/accordion';
import { MessageService } from 'primeng/api';
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
  selectEvent,
  selectEventTimeslot,
  selectEventsActionState,
} from '../../../+state/events';
import { addEventPreconfigAction } from '../../../+state/events/actions/add-event-preconfig.action';
import { loadUsersAction, selectUsersActionState, userSelectors } from '../../../+state/users';
import { loadUsersByIdAction } from '../../../+state/users/actions/load-users-by-id.action';
import { InterpolatePipe } from '../../../directives/interpolate.pipe';
import { MinigolfEventTimeslot } from '../../../models/event';
import { TranslateService } from '../../../services/translate.service';
import { dateWithTime } from '../../../utils/date.utils';
import { selectSignal } from '../../../utils/ngrx.utils';

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

    effect(() => {
      if (hasActionFailed(this.addPlayerToPreconfigActionState())) {
        this._messageService.add({
          severity: 'error',
          summary: this.translations.events_error_addPlayerToPreconfig(),
          detail: this.translations.shared_tryAgainLater(),
        });
      }
    });
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
}
