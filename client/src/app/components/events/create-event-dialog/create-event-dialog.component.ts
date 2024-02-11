import {
  ChangeDetectionStrategy,
  Component,
  ViewChild,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { Store } from '@ngrx/store';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';

import { isActionBusy } from '../../../+state/action-state';
import { addEventAction, selectEventsActionState } from '../../../+state/events';
import { AddEventRequest } from '../../../models/api/event';
import { TranslateService } from '../../../services/translate.service';
import { EventFormComponent } from '../event-form/event-form.component';

@Component({
  selector: 'app-create-event-dialog',
  standalone: true,
  imports: [ButtonModule, DialogModule, EventFormComponent],
  templateUrl: './create-event-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateEventDialogComponent {
  private readonly _store = inject(Store);

  protected readonly translations = inject(TranslateService).translations;

  protected readonly actionState = this._store.selectSignal(selectEventsActionState('add'));
  protected readonly isBusy = computed(() => isActionBusy(this.actionState()));
  protected readonly visible = signal(false);

  @ViewChild('form')
  protected readonly form!: EventFormComponent;

  constructor() {
    effect(
      () => {
        if (this.actionState().state === 'success') {
          this.visible.set(false);
        }
      },
      { allowSignalWrites: true }
    );

    effect(() => {
      if (!this.visible()) {
        this.form.reset();
      }
    });
  }

  public open() {
    this.visible.set(true);
  }

  protected onSubmit(data: AddEventRequest) {
    this._store.dispatch(addEventAction(data));
  }
}
