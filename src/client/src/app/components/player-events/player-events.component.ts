import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { ButtonModule } from 'primeng/button';
import { MessagesModule } from 'primeng/messages';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { map, timer } from 'rxjs';

import { hasActionFailed, isActionBusy } from '../../+state/action-state';
import {
  loadPlayerEventsAction,
  playerEventSelectors,
  selectPlayerEventsContinuationToken,
  selectPlayerEventsActionState,
} from '../../+state/player-events';
import { keepPlayerEventsLoaded } from '../../+state/player-events/player-events.utils';
import { PlayerEvent } from '../../models/parsed-models';
import { AuthService } from '../../services/auth.service';
import { TranslateService } from '../../services/translate.service';
import { areArraysEqual } from '../../utils/array.utils';
import { selectSignal } from '../../utils/ngrx.utils';

const dayMillis = 24 * 60 * 60 * 1000;
const SCROLL_THRESHOLD_PX = 200; // Load more events when within this many pixels of bottom

@Component({
  selector: 'app-player-events',
  standalone: true,
  imports: [
    ButtonModule,
    CommonModule,
    MessagesModule,
    ProgressSpinnerModule,
    RouterLink,
    TooltipModule,
  ],
  templateUrl: './player-events.component.html',
  styleUrl: './player-events.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlayerEventsComponent {
  private readonly _translateService = inject(TranslateService);
  private readonly _store = inject(Store);

  protected readonly translations = this._translateService.translations;
  protected readonly locale = this._translateService.language;
  protected readonly user = inject(AuthService).user;

  private readonly events = selectSignal(playerEventSelectors.selectAll);
  private readonly now = toSignal(timer(60000).pipe(map(() => Date.now())), {
    initialValue: Date.now(),
  });
  private readonly loadActionState = selectSignal(selectPlayerEventsActionState('load'));
  private readonly continuationToken = selectSignal(selectPlayerEventsContinuationToken);

  protected readonly hasEvents = computed(() => this.events().length > 0);
  protected readonly currentEvents = computed(
    () => this.events().filter(event => event.date.getTime() + dayMillis > this.now()),
    { equal: areArraysEqual }
  );
  protected readonly pastEvents = computed(
    () => this.events().filter(event => event.date.getTime() + dayMillis <= this.now()),
    { equal: areArraysEqual }
  );
  protected readonly isLoading = computed(() => isActionBusy(this.loadActionState()));
  protected readonly hasLoadFailed = computed(() => hasActionFailed(this.loadActionState()));
  protected readonly hasMoreEvents = computed(() => this.continuationToken() !== null);

  constructor() {
    keepPlayerEventsLoaded();
  }

  protected onScroll(event: Event): void {
    const element = event.target as HTMLElement;
    const position = element.scrollTop + element.clientHeight;
    const height = element.scrollHeight;

    if (position > height - SCROLL_THRESHOLD_PX && this.hasMoreEvents() && !this.isLoading()) {
      this._store.dispatch(loadPlayerEventsAction({ reload: false }));
    }
  }

  protected getRegisteredTimeslotsCount(event: PlayerEvent) {
    return event.timeslots.reduce(
      (count, timeslot) => (timeslot.isRegistered ? count + 1 : count),
      0
    );
  }
}
