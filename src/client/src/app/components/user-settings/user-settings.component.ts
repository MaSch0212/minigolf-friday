import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { SwPush } from '@angular/service-worker';
import { Actions, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DropdownModule } from 'primeng/dropdown';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputSwitchModule } from 'primeng/inputswitch';
import { MessagesModule } from 'primeng/messages';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { filter, first, map, Subject } from 'rxjs';

import { SavedFadingMessageComponent } from '../+common/saved-fading-message.component';
import { isActionBusy, hasActionFailed } from '../../+state/action-state';
import {
  loadUserSettingsAction,
  selectUserSettings,
  selectUserSettingsActionState,
  updateUserSettingsAction,
} from '../../+state/user-settings';
import { ResetNgModelDirective } from '../../directives/reset-ng-model.directive';
import { UserSettings } from '../../models/parsed-models';
import { AuthService } from '../../services/auth.service';
import { Theme, ThemeService } from '../../services/theme.service';
import { TranslateService } from '../../services/translate.service';
import { WellKnownService } from '../../services/well-known.service';
import { selectSignal } from '../../utils/ngrx.utils';
import { chainSignals } from '../../utils/signal.utils';

type LanguageOption = {
  lang: string | null;
  label: string;
  icon: string;
};

type ThemeOption = {
  theme: Theme | null;
  label: string;
  icon: string;
};

type NotifyTimeslotStartOption = {
  label: string;
  value: number | null;
};

@Component({
  selector: 'app-user-settings',
  standalone: true,
  imports: [
    ButtonModule,
    CardModule,
    CommonModule,
    DropdownModule,
    FormsModule,
    FloatLabelModule,
    InputSwitchModule,
    MessagesModule,
    ProgressSpinnerModule,
    ResetNgModelDirective,
    SavedFadingMessageComponent,
  ],
  templateUrl: './user-settings.component.html',
  styleUrl: './user-settings.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserSettingsComponent {
  private readonly _store = inject(Store);
  private readonly _translateService = inject(TranslateService);
  private readonly _themeService = inject(ThemeService);
  private readonly _authService = inject(AuthService);
  private readonly _wellKnownService = inject(WellKnownService);
  private readonly _actions$ = inject(Actions);
  private readonly _swPush = inject(SwPush);

  private readonly _loadActionState = selectSignal(selectUserSettingsActionState('load'));
  private readonly _updateActionState = selectSignal(selectUserSettingsActionState('update'));
  private readonly _notificationsGranted = signal(Notification.permission === 'granted');

  protected readonly translations = this._translateService.translations;
  protected readonly resetNgModel = new Subject<void>();
  protected readonly settings = selectSignal(selectUserSettings);
  protected readonly notificationsEnabled = chainSignals(
    toSignal(this._swPush.subscription.pipe(map(x => !!x))),
    hasPushSub => computed(() => hasPushSub() && this._notificationsGranted())
  );
  protected readonly notificationsPossible = this._swPush.isEnabled;

  protected readonly languageOptions = computed<LanguageOption[]>(() => [
    {
      lang: null,
      label: `${this.translations.settings_general_useSystemLanguage()} (${this._translateService.getLangDisplay(null)})`,
      icon: 'i-[mdi--translate]',
    },
    {
      lang: 'en',
      label: this._translateService.getLangDisplay('en'),
      icon: 'i-[flag--us-1x1]',
    },
    {
      lang: 'de',
      label: this._translateService.getLangDisplay('de'),
      icon: 'i-[flag--de-1x1]',
    },
  ]);
  protected readonly currentLanguageOption = computed(() =>
    this.languageOptions().find(x => this._translateService.isLanguage(x.lang))
  );

  protected readonly themeOptions = computed<ThemeOption[]>(() => [
    {
      theme: null,
      label: `${this.translations.settings_general_useSystemTheme()} (${this._themeService.getThemeDisplay(null)})`,
      icon: 'i-[mdi--theme-light-dark]',
    },
    {
      theme: 'dark',
      label: this.translations.settings_general_darkTheme(),
      icon: 'i-[mdi--weather-night]',
    },
    {
      theme: 'light',
      label: this.translations.settings_general_lightTheme(),
      icon: 'i-[mdi--weather-sunny]',
    },
  ]);
  protected readonly currentThemeOption = computed(() =>
    this.themeOptions().find(x => this._themeService.isTheme(x.theme))
  );

  protected readonly notifyTimeslotStartOptions = computed(() =>
    [undefined, 0, 1, 2, 5, 10, 15, 20, 30, 45, 60].map(x => this.createNotifyTimeslotOption(x))
  );

  protected readonly isUpdatingPushSubscription = signal(false);
  protected readonly isLoading = computed(() => isActionBusy(this._loadActionState()));
  protected readonly isUpdating = computed(() => isActionBusy(this._updateActionState()));
  protected readonly hasFailed = computed(() => hasActionFailed(this._loadActionState()));
  protected readonly settingsSaveStates = {
    enabledOnAllDevices: this.getSaveState('enableNotifications'),
    eanbled: toObservable(this.isUpdatingPushSubscription),
    notifyEventPublish: this.getSaveState('notifyOnEventPublish'),
    notifyEventStart: this.getSaveState('notifyOnEventStart'),
    notifyEventUpdate: this.getSaveState('notifyOnEventUpdated'),
    notifyTimeslotStart: this.getSaveState('notifyOnTimeslotStart'),
  };

  constructor() {
    this._store.dispatch(loadUserSettingsAction({ reload: false }));

    this._actions$
      .pipe(takeUntilDestroyed(), ofType(updateUserSettingsAction.error))
      .subscribe(() => {
        this.resetNgModel.next();
      });
  }

  protected changeLanguage(option: LanguageOption) {
    this._translateService.setLanguage(option.lang);
  }

  protected changeTheme(option: ThemeOption) {
    this._themeService.setTheme(option.theme);
  }

  protected updateUserSettings(changes: Partial<UserSettings>) {
    this._store.dispatch(updateUserSettingsAction(changes));
  }

  protected toggleNotifications(enabled: boolean) {
    this.isUpdatingPushSubscription.set(true);
    if (enabled) {
      Notification.requestPermission().then(permission => {
        if (permission === 'granted') {
          this._wellKnownService.wellKnown$.pipe(first()).subscribe(({ vapidPublicKey }) => {
            this._swPush.requestSubscription({ serverPublicKey: vapidPublicKey }).finally(() => {
              this.isUpdatingPushSubscription.set(false);
            });
          });
        } else {
          this.isUpdatingPushSubscription.set(false);
        }
      });
    } else {
      this._swPush.unsubscribe().finally(() => {
        this.isUpdatingPushSubscription.set(false);
      });
    }
  }

  protected logout() {
    this._authService.signOut();
  }

  private getSaveState(name: keyof UserSettings) {
    return this._actions$.pipe(
      ofType(
        updateUserSettingsAction.starting,
        updateUserSettingsAction.success,
        updateUserSettingsAction.error
      ),
      filter(x => x.props[name] !== undefined),
      map(x => x.type === updateUserSettingsAction.success.type)
    );
  }

  private createNotifyTimeslotOption(minutes?: number): NotifyTimeslotStartOption {
    return {
      label:
        minutes === undefined
          ? this.translations.shared_none()
          : `${minutes} ${minutes === 1 ? this.translations.shared_minute() : this.translations.shared_minutes()}`,
      value: minutes === undefined ? null : minutes * 60,
    };
  }
}
