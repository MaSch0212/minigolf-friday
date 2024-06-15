import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, viewChild } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { Router, RouterOutlet } from '@angular/router';
import { SwPush } from '@angular/service-worker';
import { ConfirmationService, MessageService, PrimeNGConfig } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { combineLatest, filter, first, pairwise, startWith } from 'rxjs';

import { FooterComponent } from './footer/footer.component';
import { MenuComponent } from './menu/menu.component';
import { NotificationPromptDialogComponent } from './notification-prompt-dialog/notification-prompt-dialog.component';
import { NotificationsService } from '../../api/services';
import { environment } from '../../environments/environment';
import { PushNotificationData } from '../../models/push-notification-data';
import { AuthService } from '../../services/auth.service';
import { getHasRejectedPush } from '../../services/storage';
import { TranslateService } from '../../services/translate.service';
import { arrayBufferToBase64 } from '../../utils/buffer.utils';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    ConfirmDialogModule,
    CommonModule,
    FooterComponent,
    RouterOutlet,
    MenuComponent,
    NotificationPromptDialogComponent,
    ToastModule,
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  private readonly _authService = inject(AuthService);
  private readonly _translateService = inject(TranslateService);
  private readonly _swPush = inject(SwPush);
  private readonly _router = inject(Router);
  private readonly _notificationsService = inject(NotificationsService);

  private readonly _notificationPromptDialog = viewChild.required(
    NotificationPromptDialogComponent
  );

  protected isLoggedIn = this._authService.isAuthorized;
  protected version = environment.version;

  constructor() {
    const primengConfig = inject(PrimeNGConfig);
    primengConfig.ripple = true;

    if (this._swPush.isEnabled) {
      if (!getHasRejectedPush()) {
        combineLatest([toObservable(this.isLoggedIn), this._swPush.subscription])
          .pipe(
            filter(([isLoggedIn, subscription]) => isLoggedIn && !subscription),
            first(),
            takeUntilDestroyed()
          )
          .subscribe(() => this._notificationPromptDialog().open());
      }

      combineLatest([
        toObservable(this._authService.user),
        toObservable(this._translateService.language),
        this._swPush.subscription,
      ])
        .pipe(startWith([undefined, undefined, undefined]), pairwise(), takeUntilDestroyed())
        .subscribe(([[_oldUser, _oldLang, oldSub], [newUser, newLang, newSub]]) => {
          if (newSub && newLang && newUser && newUser.id !== 'admin') {
            // Update subscription if subscription changes or language changes
            this._notificationsService.subscribeToNotifications({
              body: {
                lang: newLang,
                endpoint: newSub.endpoint,
                p256DH: arrayBufferToBase64(newSub.getKey('p256dh')),
                auth: arrayBufferToBase64(newSub.getKey('auth')),
              },
            });
          } else if (oldSub && !newSub) {
            // Unsubscribe if subscription is removed
            this._notificationsService.unsubscribeFromNotifications({
              body: { endpoint: oldSub.endpoint },
            });
          }
        });

      this._swPush.notificationClicks
        .pipe(takeUntilDestroyed())
        .subscribe(({ notification: { data } }) => {
          if (data && typeof data === 'object' && 'type' in data) {
            const parsedData = data as PushNotificationData;
            switch (parsedData.type) {
              case 'event-published':
              case 'event-started':
              case 'event-instance-updated':
              case 'event-timeslot-starting':
                this._router.navigate(['/events', parsedData.eventId]);
                break;
              default:
                console.warn('Unknown notification type:', data.type);
                break;
            }
          } else {
            console.warn('Invalid notification data:', data);
          }
        });
    }
  }
}
