import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Store } from '@ngrx/store';
import copyToClipboard from 'copy-to-clipboard';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { isActionBusy } from '../../../+state/action-state';
import {
  loadUserLoginTokenAction,
  selectUserLoginToken,
  selectUsersActionState,
} from '../../../+state/users';
import { User } from '../../../models/parsed-models';
import { TranslateService } from '../../../services/translate.service';
import { selectSignal } from '../../../utils/ngrx.utils';

@Component({
  selector: 'app-user-welcome-dialog',
  standalone: true,
  imports: [ButtonModule, CommonModule, DialogModule, ProgressSpinnerModule],
  templateUrl: './user-welcome-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserWelcomeDialogComponent {
  private readonly _messageService = inject(MessageService);
  private readonly _store = inject(Store);
  protected readonly translations = inject(TranslateService).translations;

  protected readonly visible = signal(false);
  protected readonly user = signal<User | undefined>(undefined);
  private readonly _loadTokenState = selectSignal(selectUsersActionState('loadLoginToken'));
  protected readonly isLoading = computed(() => isActionBusy(this._loadTokenState()));
  protected readonly loginToken = selectSignal(
    computed(() => selectUserLoginToken(this.user()?.id))
  );

  protected loadLoginToken() {
    if (this.loginToken()) {
      return;
    }

    const user = this.user();
    if (!user) return;

    this._store.dispatch(loadUserLoginTokenAction({ userId: user.id }));
  }

  public open(user: User) {
    this.user.set(user);
    this.loadLoginToken();
    this.visible.set(true);
  }

  protected copyWelcomeMessage() {
    const message = this.translations.users_userWelcomeDialog_welcomeMessage({
      url: (document.head.querySelector('base') as HTMLBaseElement).href,
    });
    copyToClipboard(message);
    this._messageService.add({
      severity: 'success',
      summary: this.translations.users_userWelcomeDialog_welcomeMessageCopied(),
      life: 2000,
    });
  }

  protected copyPassword(password: string) {
    copyToClipboard(password);
    this._messageService.add({
      severity: 'success',
      summary: this.translations.users_dialog_loginTokenCopied(),
      life: 2000,
    });
  }
}
