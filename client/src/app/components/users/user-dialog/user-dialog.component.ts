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
import { Actions, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import copyToClipboard from 'copy-to-clipboard';
import { Draft } from 'immer';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { ListboxModule } from 'primeng/listbox';
import { MessagesModule } from 'primeng/messages';
import { OverlayPanelModule } from 'primeng/overlaypanel';

import { hasActionFailed, isActionBusy } from '../../../+state/action-state';
import {
  addUserAction,
  loadUserLoginTokenAction,
  selectUserLoginToken,
  selectUsersActionState,
  updateUserAction,
  userSelectors,
} from '../../../+state/users';
import { InterpolatePipe } from '../../../directives/interpolate.pipe';
import { User } from '../../../models/parsed-models';
import { TranslateService } from '../../../services/translate.service';
import { areArraysEqual } from '../../../utils/array.utils';
import { notNullish } from '../../../utils/common.utils';
import { selectSignal } from '../../../utils/ngrx.utils';
import { UserItemComponent } from '../user-item/user-item.component';

@Component({
  selector: 'app-user-dialog',
  standalone: true,
  imports: [
    ButtonModule,
    CheckboxModule,
    CommonModule,
    DialogModule,
    FloatLabelModule,
    IconFieldModule,
    InputIconModule,
    InputTextModule,
    InterpolatePipe,
    ListboxModule,
    OverlayPanelModule,
    MessagesModule,
    UserItemComponent,
    ReactiveFormsModule,
  ],
  templateUrl: './user-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserDialogComponent {
  private readonly _formBuilder = inject(FormBuilder);
  private readonly _store = inject(Store);
  private readonly _messageService = inject(MessageService);
  private readonly _allUsers = selectSignal(userSelectors.selectEntities);
  private readonly _randomId = Math.random().toString(36).substring(2, 9);

  protected readonly form = this._formBuilder.group({
    id: new FormControl<string | null>(null),
    alias: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    isPlayer: new FormControl<boolean>(true, { nonNullable: true }),
    isAdmin: new FormControl<boolean>(false, { nonNullable: true }),
    preferences: this._formBuilder.group({
      avoid: new FormControl<string[]>([], { nonNullable: true }),
      prefer: new FormControl<string[]>([], { nonNullable: true }),
    }),
  });
  protected readonly translations = inject(TranslateService).translations;
  protected readonly userToUpdate = signal<User | undefined>(undefined);
  protected readonly visible = signal(false);

  private readonly _actionState = selectSignal(
    computed(() => selectUsersActionState(this.userToUpdate() ? 'update' : 'add'))
  );
  protected readonly isLoading = computed(() => isActionBusy(this._actionState()));
  protected readonly hasFailed = computed(() => hasActionFailed(this._actionState()));

  private readonly _tokenActionState = selectSignal(selectUsersActionState('loadLoginToken'));
  protected readonly isTokenLoading = computed(() => isActionBusy(this._tokenActionState()));
  protected readonly tokenVisible = signal(false);
  protected readonly loginToken = selectSignal(
    computed(() => selectUserLoginToken(this.userToUpdate()?.id))
  );

  protected readonly formValue = toSignal(this.form.valueChanges, {
    initialValue: this.form.value,
  });
  private readonly avoid = computed(() => this.formValue().preferences?.avoid ?? [], {
    equal: areArraysEqual,
  });
  private readonly prefer = computed(() => this.formValue().preferences?.prefer ?? [], {
    equal: areArraysEqual,
  });

  protected readonly avoidUsers = computed(() => this.getUsersByIds(this.avoid()));
  protected readonly preferUsers = computed(() => this.getUsersByIds(this.prefer()));
  protected readonly unassignedUsers = computed(() =>
    Object.values(this._allUsers()).filter(
      (x): x is User => !!x && !this.avoid().includes(x.id) && !this.prefer().includes(x.id)
    )
  );

  protected trackByUserId = (_: number, user: User) => user.id;

  constructor() {
    const actions$ = inject(Actions);
    actions$
      .pipe(ofType(addUserAction.success, updateUserAction.success), takeUntilDestroyed())
      .subscribe(({ type, response }) => {
        if (type === addUserAction.success.type) {
          this.copyLoginToken(response.loginToken);
        }
        this.close();
      });

    effect(
      () => {
        if (!this.visible()) {
          this.tokenVisible.set(false);
        }
      },
      { allowSignalWrites: true }
    );

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

  public open(user?: User) {
    this.userToUpdate.set(user);
    this.form.reset(
      user
        ? {
            id: user.id ?? null,
            alias: user.alias ?? '',
            isPlayer: user.roles.includes('player') ?? false,
            isAdmin: user.roles.includes('admin') ?? false,
            preferences: {
              avoid: user.playerPreferences.avoid ?? [],
              prefer: user.playerPreferences.prefer ?? [],
            },
          }
        : undefined
    );
    this.visible.set(true);
  }

  protected onAddAvoid(user: User) {
    this.form.controls.preferences.controls.avoid.setValue([...this.avoid(), user.id]);
  }

  protected onRemoveAvoid(user: User) {
    this.form.controls.preferences.controls.avoid.setValue(
      this.avoid().filter(id => id !== user.id)
    );
  }

  protected onAddPrefer(user: User) {
    this.form.controls.preferences.controls.prefer.setValue([...this.prefer(), user.id]);
  }

  protected onRemovePrefer(user: User) {
    this.form.controls.preferences.controls.prefer.setValue(
      this.prefer().filter(id => id !== user.id)
    );
  }

  protected loadLoginToken() {
    if (!this.loginToken()) {
      this._store.dispatch(loadUserLoginTokenAction({ userId: this.userToUpdate()!.id }));
    }
    this.tokenVisible.set(true);
  }

  protected async copyLoginToken(token: string | null | undefined) {
    if (token) {
      copyToClipboard(token, { message: this.translations.shared_copyToClipboard() });
      this._messageService.add({
        severity: 'success',
        summary: this.translations.users_dialog_loginTokenCopied(),
        life: 2000,
      });
    }
  }

  protected submit() {
    if (!this.form.valid) {
      this.form.markAllAsTouched();
      return;
    }

    const user: Draft<Omit<User, 'id' | 'loginToken'>> = {
      alias: this.form.value.alias ?? '',
      roles: [],
      playerPreferences: {
        avoid: this.form.value.preferences?.avoid ?? [],
        prefer: this.form.value.preferences?.prefer ?? [],
      },
    };

    if (this.form.value.isPlayer) {
      user.roles.push('player');
    }
    if (this.form.value.isAdmin) {
      user.roles.push('admin');
    }

    const userToUpdate = this.userToUpdate();
    if (userToUpdate) {
      this._store.dispatch(
        updateUserAction({
          id: userToUpdate.id,
          ...user,
        })
      );
    } else {
      this._store.dispatch(addUserAction(user));
    }
  }

  protected close() {
    this.visible.set(false);
  }

  protected id(purpose: string) {
    return `${purpose}-${this._randomId}`;
  }

  private getUsersByIds(ids: string[]) {
    return ids.map(id => this._allUsers()[id]).filter(notNullish);
  }
}
