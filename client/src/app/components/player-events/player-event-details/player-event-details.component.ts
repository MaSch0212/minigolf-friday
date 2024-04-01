import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { produce } from 'immer';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DropdownModule } from 'primeng/dropdown';
import { InputSwitchModule } from 'primeng/inputswitch';
import { MessagesModule } from 'primeng/messages';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { map, timer } from 'rxjs';

import { hasActionFailed, isActionBusy } from '../../../+state/action-state';
import {
  loadPlayerEventAction,
  registerForEventAction,
  selectPlayerEvent,
  selectPlayerEventsActionState,
} from '../../../+state/player-events';
import { EventTimeslotRegistration } from '../../../models/api/player-event';
import { MinigolfPlayerEventTimeslot } from '../../../models/player-event';
import { AuthService } from '../../../services/auth.service';
import { TranslateService } from '../../../services/translate.service';
import { ifTruthy } from '../../../utils/common.utils';
import { compareTimes, getTimeDifference } from '../../../utils/date.utils';
import { selectSignal } from '../../../utils/ngrx.utils';
import { hasTouchScreen } from '../../../utils/user-agent.utils';

// 1 1/2 hours
const gameDuration = 90 * 60 * 1000;

@Component({
  selector: 'app-player-event-details',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    CardModule,
    DropdownModule,
    FormsModule,
    ProgressSpinnerModule,
    OverlayPanelModule,
    RouterLink,
    TooltipModule,
    MessagesModule,
    InputSwitchModule,
  ],
  templateUrl: './player-event-details.component.html',
  styleUrl: './player-event-details.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlayerEventDetailsComponent {
  private readonly _store = inject(Store);
  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _translateService = inject(TranslateService);

  protected readonly translations = this._translateService.translations;
  protected readonly locale = this._translateService.language;
  protected readonly hasTouchScreen = hasTouchScreen;
  protected readonly user = inject(AuthService).user;

  private readonly eventId = toSignal(this._activatedRoute.params.pipe(map(data => data['id'])));
  private readonly actionState = selectSignal(selectPlayerEventsActionState('loadOne'));
  private readonly registerActionState = selectSignal(selectPlayerEventsActionState('register'));
  private readonly now = toSignal(timer(1000).pipe(map(() => Date.now())), {
    initialValue: Date.now(),
  });

  protected readonly isBusy = computed(() => isActionBusy(this.actionState()));
  protected readonly hasFailed = computed(() => hasActionFailed(this.actionState(), [404]));
  protected readonly isChanginRegistration = computed(() =>
    isActionBusy(this.registerActionState())
  );
  protected readonly hasRegistrationFailed = computed(() =>
    hasActionFailed(this.registerActionState())
  );
  protected readonly event = selectSignal(computed(() => selectPlayerEvent(this.eventId())));
  protected readonly timeslots = computed(() =>
    [...(this.event()?.timeslots ?? [])].sort((a, b) => compareTimes(a.time, b.time))
  );
  protected readonly games = computed(() =>
    this.timeslots()
      .filter(x => x.instance)
      .map(x => ({
        timeslot: x,
        instance: x.instance!,
      }))
  );
  protected readonly timeslotsWithoutFallback = computed(() =>
    this.timeslots().filter(x => !x.isFallbackAllowed)
  );
  protected readonly canRegister = computed(() =>
    ifTruthy(this.event(), event => event.registrationDeadline.getTime() > this.now(), false)
  );
  protected readonly currentRegistrations = computed(() =>
    ifTruthy(
      this.event(),
      event =>
        event.timeslots
          .filter(x => x.isRegistered)
          .map<EventTimeslotRegistration>(x => ({
            timeslotId: x.id,
            fallbackTimeslotId: x.chosenFallbackTimeslotId,
          })),
      []
    )
  );

  constructor() {
    effect(
      () => {
        if (!this.event()) {
          this._store.dispatch(loadPlayerEventAction({ eventId: this.eventId() }));
        }
      },
      { allowSignalWrites: true }
    );
  }

  protected setTimeslotRegistration(timeslot: MinigolfPlayerEventTimeslot, isRegistered: boolean) {
    this._store.dispatch(
      registerForEventAction({
        eventId: this.eventId(),
        registrations: produce(this.currentRegistrations(), draft => {
          const existing = draft.find(x => x.timeslotId === timeslot.id);
          if (existing && !isRegistered) {
            draft.splice(draft.indexOf(existing), 1);
          } else if (!existing && isRegistered) {
            draft.push({ timeslotId: timeslot.id });
          }
        }),
      })
    );
  }

  protected setFallbackTimeslot(timeslot: MinigolfPlayerEventTimeslot, fallbackTimeslotId: string) {
    this._store.dispatch(
      registerForEventAction({
        eventId: this.eventId(),
        registrations: produce(this.currentRegistrations(), draft => {
          const existing = draft.find(x => x.timeslotId === timeslot.id);
          if (existing) {
            existing.fallbackTimeslotId = fallbackTimeslotId;
          }
        }),
      })
    );
  }

  protected canRegisterForTimeslot(timeslot: MinigolfPlayerEventTimeslot) {
    const currentRegistrations = this.currentRegistrations();
    if (currentRegistrations.some(x => x.timeslotId === timeslot.id)) {
      return true;
    }
    for (const registration of currentRegistrations) {
      if (timeslot.id === registration.fallbackTimeslotId) return false;
      const regTimeslot = this.timeslots().find(x => x.id === registration.timeslotId);
      if (regTimeslot) {
        const diff = Math.abs(getTimeDifference(timeslot.time, regTimeslot.time));
        if (diff < gameDuration) return false;
      }
      if (registration.fallbackTimeslotId) {
        const fallbackTimeslot = this.timeslots().find(
          x => x.id === registration.fallbackTimeslotId
        );
        if (fallbackTimeslot) {
          const diff = Math.abs(getTimeDifference(timeslot.time, fallbackTimeslot.time));
          if (diff < gameDuration) return false;
        }
      }
    }
    return true;
  }
}
