import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Actions, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DropdownModule } from 'primeng/dropdown';
import { InputSwitchModule } from 'primeng/inputswitch';
import { MessagesModule } from 'primeng/messages';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { filter, map, Subject, timer } from 'rxjs';

import { FadingMessageComponent } from '../../+common/fading-message.component';
import { hasActionFailed, isActionBusy } from '../../../+state/action-state';
import {
  updateEventRegistrationAction,
  selectPlayerEvent,
  selectPlayerEventsActionState,
} from '../../../+state/player-events';
import { keepPlayerEventLoaded } from '../../../+state/player-events/player-events.utils';
import { ApiEventTimeslotRegistration } from '../../../api/models';
import { ResetNgModelDirective } from '../../../directives/reset-ng-model.directive';
import { PlayerEventTimeslot } from '../../../models/parsed-models';
import { AuthService } from '../../../services/auth.service';
import { TranslateService } from '../../../services/translate.service';
import { areArraysEqual } from '../../../utils/array.utils';
import { ifTruthy } from '../../../utils/common.utils';
import { compareTimes, getTimeDifference } from '../../../utils/date.utils';
import { errorToastEffect, selectSignal } from '../../../utils/ngrx.utils';
import { chainSignals } from '../../../utils/signal.utils';
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
    FadingMessageComponent,
    FormsModule,
    ProgressSpinnerModule,
    OverlayPanelModule,
    RouterLink,
    TooltipModule,
    MessagesModule,
    InputSwitchModule,
    ResetNgModelDirective,
  ],
  templateUrl: './player-event-details.component.html',
  styleUrl: './player-event-details.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlayerEventDetailsComponent {
  private readonly _store = inject(Store);
  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _translateService = inject(TranslateService);
  private readonly _actions$ = inject(Actions);

  protected readonly translations = this._translateService.translations;
  protected readonly locale = this._translateService.language;
  protected readonly hasTouchScreen = hasTouchScreen;
  protected readonly user = inject(AuthService).user;
  protected readonly resetNgModel = new Subject<void>();

  private readonly eventId = toSignal(this._activatedRoute.params.pipe(map(data => data['id'])));
  private readonly actionState = selectSignal(selectPlayerEventsActionState('loadOne'));
  private readonly now = toSignal(timer(1000).pipe(map(() => Date.now())), {
    initialValue: Date.now(),
  });

  protected readonly registerActionState = selectSignal(selectPlayerEventsActionState('register'));
  protected readonly isBusy = computed(() => isActionBusy(this.actionState()));
  protected readonly hasFailed = computed(() => hasActionFailed(this.actionState(), [404]));
  protected readonly isChanginRegistration = computed(() =>
    isActionBusy(this.registerActionState())
  );
  protected readonly event = selectSignal(computed(() => selectPlayerEvent(this.eventId())));
  protected readonly timeslots = computed(() =>
    [...(this.event()?.timeslots ?? [])].sort((a, b) => compareTimes(a.time, b.time))
  );
  protected readonly timeslotSaveStates = chainSignals(
    computed(() => this.timeslots().map(x => x.id), { equal: areArraysEqual }),
    id =>
      computed(() =>
        id().map(id =>
          this._actions$.pipe(
            ofType(
              updateEventRegistrationAction.starting,
              updateEventRegistrationAction.success,
              updateEventRegistrationAction.error
            ),
            filter(x => x.props.timeslotId === id),
            map(x => x.type === updateEventRegistrationAction.success.type)
          )
        )
      )
  );
  protected readonly lastChangedTimeslotId = signal<string | null>(null);
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
          .map<ApiEventTimeslotRegistration>(x => ({
            timeslotId: x.id,
            fallbackTimeslotId: x.chosenFallbackTimeslotId,
          })),
      []
    )
  );

  constructor() {
    keepPlayerEventLoaded(this.eventId);

    effect(
      () => {
        if (hasActionFailed(this.registerActionState())) {
          this.resetNgModel.next();
        }
      },
      { allowSignalWrites: true }
    );

    errorToastEffect(
      computed(() => this.translations.playerEvents_error_registration()),
      this.registerActionState
    );
  }

  protected setTimeslotRegistration(timeslot: PlayerEventTimeslot, isRegistered: boolean) {
    this._store.dispatch(
      updateEventRegistrationAction({
        eventId: this.eventId(),
        timeslotId: timeslot.id,
        isRegistered,
      })
    );
  }

  protected setFallbackTimeslot(timeslot: PlayerEventTimeslot, fallbackTimeslotId: string | null) {
    this._store.dispatch(
      updateEventRegistrationAction({
        eventId: this.eventId(),
        timeslotId: timeslot.id,
        fallbackTimeslotId,
      })
    );
  }

  protected canRegisterForTimeslot(timeslot: PlayerEventTimeslot) {
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
