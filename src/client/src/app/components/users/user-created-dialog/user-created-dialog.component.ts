import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { interpolate, InterpolatePipe } from '@ngneers/signal-translate';
import copyToClipboard from 'copy-to-clipboard';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';

import { User } from '../../../models/parsed-models';
import { TranslateService } from '../../../services/translate.service';

@Component({
  selector: 'app-user-created-dialog',
  standalone: true,
  imports: [ButtonModule, CommonModule, DialogModule, InterpolatePipe],
  templateUrl: './user-created-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserCreatedDialogComponent {
  private readonly _messageService = inject(MessageService);
  protected readonly translations = inject(TranslateService).translations;

  protected readonly visible = signal(false);
  protected readonly user = signal<User | undefined>(undefined);

  public open(user: User) {
    this.user.set(user);
    this.visible.set(true);
  }

  protected copyWelcomeMessage() {
    const message = interpolate(this.translations.users_userCreatedDialog_welcomeMessage(), {
      url: (document.head.querySelector('base') as HTMLBaseElement).href,
    });
    copyToClipboard(message);
    this._messageService.add({
      severity: 'success',
      summary: this.translations.users_userCreatedDialog_welcomeMessageCopied(),
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
