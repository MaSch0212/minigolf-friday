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
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { Actions, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { AccordionModule } from 'primeng/accordion';
import { ButtonModule } from 'primeng/button';
import { DataViewModule } from 'primeng/dataview';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { ListboxModule } from 'primeng/listbox';
import { MessagesModule } from 'primeng/messages';
import { OverlayPanelModule } from 'primeng/overlaypanel';

import {
  addPlayerAction,
  addPlayerSuccessAction,
  playerSelectors,
  selectPlayerActionState,
  updatePlayerAction,
  updatePlayerSuccessAction,
} from '../../../+state/players';
import { InterpolatePipe } from '../../../directives/interpolate.pipe';
import { Player } from '../../../models/player';
import { TranslateService } from '../../../services/translate.service';
import { areArraysEqual } from '../../../utils/array.utils';
import { notNullish } from '../../../utils/common.utils';
import { autoDestroy } from '../../../utils/rxjs.utils';
import { PlayerItemComponent } from '../player-item/player-item.component';

@Component({
  selector: 'app-player-dialog',
  standalone: true,
  imports: [
    AccordionModule,
    ButtonModule,
    CommonModule,
    DataViewModule,
    DialogModule,
    InputTextModule,
    InterpolatePipe,
    ListboxModule,
    MessagesModule,
    OverlayPanelModule,
    PlayerItemComponent,
    ReactiveFormsModule,
  ],
  templateUrl: './player-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlayerDialogComponent {
  private readonly _formBuilder = inject(FormBuilder);
  private readonly _store = inject(Store);
  private readonly _actions$ = inject(Actions);
  private readonly _allPlayers = this._store.selectSignal(playerSelectors.selectEntities);
  private readonly _randomId = Math.random().toString(36).substring(2, 9);

  protected readonly form = this._formBuilder.group(
    {
      id: new FormControl<string | null>(null),
      alias: new FormControl<string | null>(null),
      name: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
      facebookId: new FormControl<string | null>(null),
      whatsAppNumber: new FormControl<string | null>(null),
      preferences: this._formBuilder.group({
        avoid: new FormControl<string[]>([], { nonNullable: true }),
        prefer: new FormControl<string[]>([], { nonNullable: true }),
      }),
    },
    { validators: [validateComms] }
  );
  protected readonly actionState = this._store.selectSignal(selectPlayerActionState);
  protected readonly translations = inject(TranslateService).translations;
  protected readonly playerToUpdate = signal<Player | undefined>(undefined);
  protected readonly visible = signal(false);

  protected readonly formValue = toSignal(this.form.valueChanges, {
    initialValue: this.form.value,
  });
  private readonly avoid = computed(() => this.formValue().preferences?.avoid ?? [], {
    equal: areArraysEqual,
  });
  private readonly prefer = computed(() => this.formValue().preferences?.prefer ?? [], {
    equal: areArraysEqual,
  });

  protected readonly avoidPlayers = computed(() => this.getPlayersByIds(this.avoid()));
  protected readonly preferPlayers = computed(() => this.getPlayersByIds(this.prefer()));
  protected readonly unassignedPlayers = computed(() =>
    Object.values(this._allPlayers()).filter(
      (x): x is Player => !!x && !this.avoid().includes(x.id) && !this.prefer().includes(x.id)
    )
  );

  protected get commTouched(): boolean {
    return this.form.controls.facebookId.touched || this.form.controls.whatsAppNumber.touched;
  }
  protected get hasCommRequiredError(): boolean {
    return this.form.hasError('commRequired');
  }

  protected trackByPlayerId = (_: number, player: Player) => player.id;

  constructor() {
    autoDestroy(
      this._actions$
        .pipe(ofType(addPlayerSuccessAction, updatePlayerSuccessAction))
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

  public open(player?: Player) {
    this.playerToUpdate.set(player);
    this.form.reset(player);
    this.visible.set(true);
  }

  protected onAddAvoid(player: Player) {
    this.form.controls.preferences.controls.avoid.setValue([...this.avoid(), player.id]);
  }

  protected onRemoveAvoid(player: Player) {
    this.form.controls.preferences.controls.avoid.setValue(
      this.avoid().filter(id => id !== player.id)
    );
  }

  protected onAddPrefer(player: Player) {
    this.form.controls.preferences.controls.prefer.setValue([...this.prefer(), player.id]);
  }

  protected onRemovePrefer(player: Player) {
    this.form.controls.preferences.controls.prefer.setValue(
      this.prefer().filter(id => id !== player.id)
    );
  }

  protected submit() {
    if (!this.form.valid) {
      this.form.markAllAsTouched();
      return;
    }
    const action = this.playerToUpdate() ? updatePlayerAction : addPlayerAction;
    this._store.dispatch(action({ player: this.form.value as Player }));
  }

  protected close() {
    this.visible.set(false);
  }

  protected id(purpose: string) {
    return `${purpose}-${this._randomId}`;
  }

  private getPlayersByIds(ids: string[]) {
    return ids.map(id => this._allPlayers()[id]).filter(notNullish);
  }
}

function validateComms(control: AbstractControl): ValidationErrors | null {
  return control.value.facebookId || control.value.whatsAppNumber ? null : { commRequired: true };
}
