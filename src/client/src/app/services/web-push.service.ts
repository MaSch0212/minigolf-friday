import { computed, DestroyRef, EventEmitter, inject, Injectable, signal } from '@angular/core';
import { toObservable, takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { SwPush } from '@angular/service-worker';
import { combineLatest, filter, first, startWith, pairwise, firstValueFrom, map } from 'rxjs';

import { AuthService } from './auth.service';
import { Logger } from './logger.service';
import { getHasConfiguredPush, setHasConfiguredPush } from './storage';
import { TranslateService } from './translate.service';
import { WellKnownService } from './well-known.service';
import { NotificationsService } from '../api/services';
import { arrayBufferToBase64 } from '../utils/buffer.utils';
import { chainSignals } from '../utils/signal.utils';

const Notification = 'Notification' in window ? window.Notification : null;

@Injectable({ providedIn: 'root' })
export class WebPushService {
  private readonly _authService = inject(AuthService);
  private readonly _translateService = inject(TranslateService);
  private readonly _swPush = inject(SwPush);
  private readonly _notificationsService = inject(NotificationsService);
  private readonly _wellKnownService = inject(WellKnownService);

  private readonly _subscription = toSignal(this._swPush.subscription);

  public readonly onPromptNotification = new EventEmitter<void>();
  public readonly notificationsPermission = signal(Notification?.permission ?? 'denied');
  public readonly notificationsSupported =
    !!Notification && this._swPush.isEnabled && !/OculusBrowser/.test(navigator.userAgent);
  public readonly notificationsEnabled = this.notificationsSupported
    ? chainSignals(toSignal(this._swPush.subscription.pipe(map(x => !!x))), hasPushSub =>
        computed(() => hasPushSub() && this.notificationsPermission() === 'granted')
      )
    : signal(false);

  constructor() {
    if (!this.notificationsSupported) {
      Logger.logInfo('WebPushService', 'Web push notifications are not supported');
      return;
    }

    if (!getHasConfiguredPush()) {
      combineLatest([toObservable(this._authService.isAuthorized), this._swPush.subscription])
        .pipe(
          filter(
            ([isLoggedIn, subscription]) =>
              isLoggedIn &&
              !subscription &&
              this.notificationsPermission() !== 'denied' &&
              !getHasConfiguredPush()
          ),
          first(),
          takeUntilDestroyed()
        )
        .subscribe(() => this.onPromptNotification.emit());
    }

    combineLatest([
      toObservable(this._authService.user),
      toObservable(this._translateService.language),
      this._swPush.subscription,
    ])
      .pipe(startWith([undefined, undefined, undefined]), pairwise(), takeUntilDestroyed())
      .subscribe(([[oldUser, oldLang, oldSub], [newUser, newLang, newSub]]) => {
        Logger.logDebug('WebPushService', 'User, lang, sub changed', {
          oldUser,
          oldLang,
          oldSub,
          newUser,
          newLang,
          newSub,
        });
        if (newSub && newLang && newUser && newUser.id !== 'admin') {
          this.registerSubscription(newLang, newSub);
        } else if (oldSub && !newSub) {
          this.unregisterSubscription(oldSub);
        }
      });

    const sub = this._authService.onBeforeSignOut(() => this.disable(true));
    inject(DestroyRef).onDestroy(() => sub.unsubscribe());
  }

  public async enable(): Promise<boolean> {
    if (!this.notificationsSupported || !Notification || Notification.permission === 'denied') {
      return false;
    }

    let permission: NotificationPermission = Notification.permission;
    if (permission === 'default') {
      Logger.logDebug('WebPushService', 'Requesting permission for push notifications');
      permission = await Notification.requestPermission();
      this.notificationsPermission.set(permission);
    }

    Logger.logDebug('WebPushService', 'Permission for push notifications', permission);
    if (permission === 'granted') {
      const subscription = this._subscription();
      if (subscription) {
        Logger.logDebug('WebPushService', 'Already subscribed to push notifications', subscription);
        await this.registerSubscription(this._translateService.language(), subscription);
        setHasConfiguredPush(true);
        return true;
      }
      Logger.logDebug('WebPushService', 'Getting VAPID key');
      const { vapidPublicKey } = await firstValueFrom(this._wellKnownService.wellKnown$);
      Logger.logDebug('WebPushService', 'Requesting subscription to push notifications');
      await this._swPush.requestSubscription({ serverPublicKey: vapidPublicKey });
      setHasConfiguredPush(true);
      return true;
    } else {
      setHasConfiguredPush(true);
      return false;
    }
  }

  public async disable(keepSubscription: boolean = false): Promise<void> {
    if (!this.notificationsSupported) return;

    const subscription = this._subscription();
    if (!subscription) return;

    if (keepSubscription) {
      await this.unregisterSubscription(subscription);
    } else {
      Logger.logDebug('WebPushService', 'Unsubscribing from push notifications', subscription);
      await this._swPush.unsubscribe();
      setHasConfiguredPush(true);
    }
  }

  private async registerSubscription(newLang: string, newSub: PushSubscription) {
    Logger.logDebug('WebPushService', 'Registering subscription', newSub);
    await this._notificationsService.subscribeToNotifications({
      body: {
        lang: newLang,
        endpoint: newSub.endpoint,
        p256DH: arrayBufferToBase64(newSub.getKey('p256dh')),
        auth: arrayBufferToBase64(newSub.getKey('auth')),
      },
    });
  }

  private async unregisterSubscription(oldSub: PushSubscription) {
    Logger.logDebug('WebPushService', 'Unregistering subscription', oldSub);
    await this._notificationsService.unsubscribeFromNotifications({
      body: { endpoint: oldSub.endpoint },
    });
  }
}
