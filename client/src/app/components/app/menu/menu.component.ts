import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { MenubarModule } from 'primeng/menubar';
import { TooltipModule } from 'primeng/tooltip';

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
})
export class MenuComponent {
  private readonly _store = inject(Store);
  private readonly _translateService = inject(TranslateService);
  private readonly _themeService = inject(ThemeService);
  private readonly _authService = inject(AuthService);

  protected translations = this._translateService.translations;
  protected title = chainSignals(this._store.selectSignal(selectAppTitle), title =>
    computed(() =>
      title().translate ? this.translations[title().title as TranslationKey]() : title().title
    )
  );
  protected isLoggedIn = this._authService.isAuthorized;
  protected isAdmin = computed(() => this._authService.user()?.isAdmin === true);

  protected menuItems = computed<MenuItem[]>(() => [
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
}
