import { animate, style, transition, trigger } from '@angular/animations';
import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { SwUpdate } from '@angular/service-worker';
import { Store } from '@ngrx/store';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { MenubarModule } from 'primeng/menubar';
import { TooltipModule } from 'primeng/tooltip';
import { fromEvent } from 'rxjs';

import { selectAppTitle } from '../../../+state/app';
import { AuthService } from '../../../services/auth.service';
import { ThemeService } from '../../../services/theme.service';
import { TranslateService, TranslationKey } from '../../../services/translate.service';
import { chainSignals } from '../../../utils/signal.utils';

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [ButtonModule, CommonModule, MenubarModule, MenuModule, TooltipModule],
  templateUrl: './menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  animations: [
    trigger('flyInTopAnimation', [
      transition(':enter', [
        style({ transform: 'translateY(-200%)', opacity: 0 }),
        animate('300ms ease-out', style({ transform: 'translateY(0)', opacity: 1 })),
      ]),
    ]),
  ],
})
export class MenuComponent {
  private readonly _store = inject(Store);
  private readonly _translateService = inject(TranslateService);
  private readonly _themeService = inject(ThemeService);
  private readonly _authService = inject(AuthService);
  private readonly _swUpdate = inject(SwUpdate);

  private readonly _versionInfo = toSignal(this._swUpdate.versionUpdates);

  protected readonly translations = this._translateService.translations;
  protected readonly title = chainSignals(this._store.selectSignal(selectAppTitle), title =>
    computed(() =>
      title().translate ? this.translations[title().title as TranslationKey]() : title().title
    )
  );
  protected readonly isLoggedIn = this._authService.isAuthorized;
  protected readonly isAdmin = computed(
    () => this._authService.user()?.roles.includes('admin') ?? false
  );
  protected readonly newVersionAvailable = computed(
    () => this._versionInfo()?.type === 'VERSION_READY'
  );

  protected readonly menuItems = computed<MenuItem[]>(() => [
    {
      label: this.translations.nav_home(),
      icon: 'i-[mdi--home]',
      routerLink: '/home',
      visible: this.isLoggedIn(),
    },
    {
      label: this.translations.nav_manage(),
      icon: 'i-[mdi--table-edit]',
      items: [
        {
          label: this.translations.nav_maps(),
          icon: 'i-[mdi--golf]',
          routerLink: '/manage/maps',
        },
        {
          label: this.translations.nav_users(),
          icon: 'i-[mdi--account-multiple]',
          routerLink: '/manage/users',
        },
        {
          label: this.translations.nav_events(),
          icon: 'i-[mdi--calendar]',
          routerLink: '/manage/events',
        },
      ],
      visible: this.isAdmin(),
    },
    {
      separator: true,
      visible: !this.isAdmin(),
      state: { grow: true },
    },
    {
      label: this.translations.nav_settings(),
      icon: 'i-[mdi--cog]',
      items: [
        {
          label: this.translations.settings_theme(),
          icon: 'i-[mdi--theme-light-dark]',
          items: this._themeService.themeItems(),
        },
        {
          label: this.translations.settings_language(),
          icon: 'i-[mdi--translate]',
          items: this._translateService.languageItems(),
        },
        {
          separator: true,
          visible: this.isLoggedIn(),
        },
        {
          label: this.translations.settings_signOut(),
          icon: 'i-[mdi--logout]',
          command: () => this._authService.signOut(),
          visible: this.isLoggedIn(),
        },
      ],
      state: {
        expand: true,
      },
    },
  ]);

  constructor() {
    fromEvent(document, 'visibilitychange')
      .pipe(takeUntilDestroyed())
      .subscribe(() => {
        if (!document.hidden) {
          console.info('Checking for updates...');
          this._swUpdate.checkForUpdate().then(x => console.info('Update check result:', x));
        }
      });
  }

  protected updateApp() {
    location.reload();
  }
}
