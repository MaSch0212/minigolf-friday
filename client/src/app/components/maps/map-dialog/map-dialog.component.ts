import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { Actions, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { MessagesModule } from 'primeng/messages';

import {
  addMapAction,
  addMapSuccessAction,
  selectMapActionState,
  updateMapAction,
  updateMapSuccessAction,
} from '../../../+state/maps';
import { InterpolatePipe } from '../../../directives/interpolate.pipe';
import { MinigolfMap } from '../../../models/minigolf-map';
import { TranslateService } from '../../../services/translate.service';
import { autoDestroy } from '../../../utils/rxjs.utils';

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
  private readonly _actions$ = inject(Actions);
  private readonly _randomId = Math.random().toString(36).substring(2, 9);

  protected readonly form = this._formBuilder.group({
    id: new FormControl<string | null>(null),
    name: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
  });
  protected readonly actionState = this._store.selectSignal(selectMapActionState);
  protected readonly translations = inject(TranslateService).translations;
  protected readonly mapToUpdate = signal<MinigolfMap | undefined>(undefined);
  protected readonly visible = signal(false);

  protected readonly formValue = toSignal(this.form.valueChanges, {
    initialValue: this.form.value,
  });

  constructor() {
    autoDestroy(
      this._actions$
        .pipe(ofType(addMapSuccessAction, updateMapSuccessAction))
        .subscribe(() => this.close())
    );
    effect(
      () => {
        if (this.actionState().loading) {
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
    const action = this.mapToUpdate() ? updateMapAction : addMapAction;
    this._store.dispatch(action({ map: this.form.value as MinigolfMap }));
  }

  protected close() {
    this.visible.set(false);
  }

  protected id(purpose: string) {
    return `${purpose}-${this._randomId}`;
  }
}
