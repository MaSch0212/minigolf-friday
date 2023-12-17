import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { MenuItem } from 'primeng/api';
import { MenubarModule } from 'primeng/menubar';

import { selectAppTitle } from '../../../+state/app';
import { AuthService } from '../../../services/auth.service';
import { ThemeService } from '../../../services/theme.service';
import { TranslateService, TranslationKey } from '../../../services/translate.service';
import { chainSignals } from '../../../utils/signal.utils';

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [CommonModule, MenubarModule],
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

  protected menuItems = computed<MenuItem[]>(() => [
    {
      label: this.translations.nav_home(),
      icon: 'i-[mdi--home]',
      routerLink: '/home',
    },
    {
      label: this.translations.nav_manage(),
      icon: 'i-[mdi--table-edit]',
      items: [
        {
          label: this.translations.nav_players(),
          icon: 'i-[mdi--account-multiple]',
          routerLink: '/manage/players',
        },
        {
          label: this.translations.nav_maps(),
          icon: 'i-[mdi--golf]',
          routerLink: '/manage/maps',
        },
      ],
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
        },
        {
          label: this.translations.settings_signOut(),
          icon: 'i-[mdi--logout]',
          command: () => this._authService.signOut(),
        },
      ],
    },
  ]);
}
