import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { SwPush } from '@angular/service-worker';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { first } from 'rxjs';

import { setHasRejectedPush } from '../../../services/storage';
import { TranslateService } from '../../../services/translate.service';
import { WellKnownService } from '../../../services/well-known.service';

@Component({
  selector: 'app-notification-prompt-dialog',
  standalone: true,
  imports: [ButtonModule, DialogModule],
  templateUrl: './notification-prompt-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationPromptDialogComponent {
  private readonly _swPush = inject(SwPush);
  private readonly _wellKnownService = inject(WellKnownService);
  protected readonly translations = inject(TranslateService).translations;

  protected readonly visible = signal(false);
  protected readonly isLoading = signal(false);

  public open() {
    this.visible.set(true);
  }

  protected onAccept() {
    this.isLoading.set(true);
    Notification.requestPermission().then(permission => {
      if (permission === 'granted') {
        this._wellKnownService.wellKnown$.pipe(first()).subscribe(({ vapidPublicKey }) => {
          this._swPush.requestSubscription({ serverPublicKey: vapidPublicKey }).finally(() => {
            this.isLoading.set(false);
            this.visible.set(false);
          });
        });
      } else {
        setHasRejectedPush(true);
        this.isLoading.set(false);
        this.visible.set(false);
      }
    });
  }

  protected onReject() {
    setHasRejectedPush(true);
    this.visible.set(false);
  }
}
