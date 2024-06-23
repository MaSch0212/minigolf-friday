import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';

import { setHasConfiguredPush } from '../../../services/storage';
import { TranslateService } from '../../../services/translate.service';
import { WebPushService } from '../../../services/web-push.service';

@Component({
  selector: 'app-notification-prompt-dialog',
  standalone: true,
  imports: [ButtonModule, DialogModule],
  templateUrl: './notification-prompt-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationPromptDialogComponent {
  private readonly _webPushService = inject(WebPushService);
  protected readonly translations = inject(TranslateService).translations;

  protected readonly visible = signal(false);
  protected readonly isLoading = signal(false);

  public open() {
    this.visible.set(true);
  }

  protected async onAccept() {
    this.isLoading.set(true);
    try {
      await this._webPushService.enable();
    } finally {
      this.isLoading.set(false);
      this.visible.set(false);
    }
  }

  protected onReject() {
    setHasConfiguredPush(true);
    this.visible.set(false);
  }
}
