import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { InterpolatePipe } from '@ngneers/signal-translate';
import { Actions, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { MessagesModule } from 'primeng/messages';

import { isActionBusy } from '../../../+state/action-state';
import { addMapAction, selectMapsActionState, updateMapAction } from '../../../+state/maps';
import { MinigolfMap } from '../../../models/parsed-models';
import { TranslateService } from '../../../services/translate.service';
import { selectSignal } from '../../../utils/ngrx.utils';

@Component({
  selector: 'app-map-dialog',
  standalone: true,
  imports: [
    ButtonModule,
    CommonModule,
    DialogModule,
    InputTextModule,
    InterpolatePipe,
    MessagesModule,
    ReactiveFormsModule,
  ],
  templateUrl: './map-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MapDialogComponent {
  private readonly _formBuilder = inject(FormBuilder);
  private readonly _store = inject(Store);
  private readonly _randomId = Math.random().toString(36).substring(2, 9);

  protected readonly form = this._formBuilder.group({
    id: new FormControl<string | null>(null),
    name: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
  });
  protected readonly translations = inject(TranslateService).translations;
  protected readonly mapToUpdate = signal<MinigolfMap | undefined>(undefined);
  protected readonly visible = signal(false);

  private readonly _actionState = selectSignal(
    computed(() => selectMapsActionState(this.mapToUpdate() ? 'update' : 'add'))
  );
  protected readonly isLoading = computed(() => isActionBusy(this._actionState()));

  protected readonly formValue = toSignal(this.form.valueChanges, {
    initialValue: this.form.value,
  });

  constructor() {
    const actions$ = inject(Actions);
    actions$
      .pipe(ofType(addMapAction.success, updateMapAction.success), takeUntilDestroyed())
      .subscribe(() => this.close());

    effect(
      () => {
        if (this.isLoading()) {
          this.form.disable();
        } else {
          this.form.enable();
        }
      },
      { allowSignalWrites: true }
    );
  }

  public open(map?: MinigolfMap) {
    this.mapToUpdate.set(map);
    this.form.reset(map);
    this.visible.set(true);
  }

  protected submit() {
    if (!this.form.valid) {
      this.form.markAllAsTouched();
      return;
    }

    const mapToUpdate = this.mapToUpdate();
    if (mapToUpdate) {
      this._store.dispatch(
        updateMapAction({ mapId: mapToUpdate.id, name: this.form.value.name ?? '' })
      );
    } else {
      this._store.dispatch(addMapAction({ name: this.form.value.name ?? '' }));
    }
  }

  protected close() {
    this.visible.set(false);
  }

  protected id(purpose: string) {
    return `${purpose}-${this._randomId}`;
  }
}
