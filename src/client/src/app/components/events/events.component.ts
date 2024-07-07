import { CommonModule, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Actions, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { MessagesModule } from 'primeng/messages';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { RippleModule } from 'primeng/ripple';
import { TooltipModule } from 'primeng/tooltip';

import { CreateEventDialogComponent } from './create-event-dialog/create-event-dialog.component';
import { hasActionFailed, isActionBusy } from '../../+state/action-state';
import {
  addEventAction,
  eventSelectors,
  loadEventsAction,
  selectEventsActionState,
} from '../../+state/events';
import { keepEventsLoaded } from '../../+state/events/events.utils';
import { TranslateService } from '../../services/translate.service';
import { selectSignal } from '../../utils/ngrx.utils';

@Component({
  selector: 'app-events',
  standalone: true,
  imports: [
    ButtonModule,
    CommonModule,
    CreateEventDialogComponent,
    MenuModule,
    MessagesModule,
    ProgressSpinnerModule,
    RippleModule,
    RouterLink,
    TooltipModule,
  ],
  templateUrl: './events.component.html',
  styleUrl: './events.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [DatePipe],
})
export class EventsComponent {
  private readonly _store = inject(Store);
  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _translateService = inject(TranslateService);
  private readonly _datePipe = inject(DatePipe);

  protected readonly translations = this._translateService.translations;
  protected readonly locale = this._translateService.language;

  private readonly actionState = selectSignal(selectEventsActionState('load'));
  protected readonly isBusy = computed(() => isActionBusy(this.actionState()));
  protected readonly hasFailed = computed(() => hasActionFailed(this.actionState()));
  protected readonly events = this._store.selectSignal(eventSelectors.selectAll);
  protected readonly menuItems = computed(() =>
    this.events().map(
      event =>
        <MenuItem>{
          label: this._datePipe.transform(event.date, 'fullDate', undefined, this.locale()),
          id: event.id,
          routerLink: [event.id],
        }
    )
  );

  constructor() {
    keepEventsLoaded();

    const actions$ = inject(Actions);
    actions$
      .pipe(takeUntilDestroyed(), ofType(addEventAction.success))
      .subscribe(({ response }) =>
        this._router.navigate([response.id], { relativeTo: this._activatedRoute })
      );
  }

  protected loadNextPage(): void {
    this._store.dispatch(loadEventsAction({ reload: false }));
  }
}
