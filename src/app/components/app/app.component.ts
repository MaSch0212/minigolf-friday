import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Store } from '@ngrx/store';
import { ConfirmationService, MenuItem, MessageService, PrimeNGConfig } from 'primeng/api';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MenubarModule } from 'primeng/menubar';
import { ToastModule } from 'primeng/toast';

import { selectAppTitle } from '../../+state/app';
import { ThemeService } from '../../services/theme.service';
import { TranslateService, TranslationKey } from '../../services/translate.service';
import { chainSignals } from '../../utils/signal.utils';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    BreadcrumbModule,
    ConfirmDialogModule,
    CommonModule,
    RouterOutlet,
    MenubarModule,
    ToastModule,
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  private readonly _store = inject(Store);
  private readonly _translateService = inject(TranslateService);
  private readonly _themeService = inject(ThemeService);

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
          items: [
            {
              label: `${this.translations.settings_useSystemTheme()} (${this.getThemeDisplay(
                this._themeService.isDarkColorSchemePrefered() ? 'dark' : 'light'
              )})`,
              icon: this._themeService.isTheme(null)
                ? 'i-[mdi--check]'
                : 'i-[mdi--theme-light-dark]',
              command: () => this._themeService.setTheme(null),
            },
            {
              separator: true,
            },
            {
              label: this.translations.settings_darkTheme(),
              icon: this._themeService.isTheme('dark')
                ? 'i-[mdi--check]'
                : 'i-[mdi--weather-night]',
              command: () => this._themeService.setTheme('dark'),
            },
            {
              label: this.translations.settings_lightTheme(),
              icon: this._themeService.isTheme('light')
                ? 'i-[mdi--check]'
                : 'i-[mdi--weather-sunny]',
              command: () => this._themeService.setTheme('light'),
            },
          ],
        },
        {
          label: this.translations.settings_language(),
          icon: 'i-[mdi--translate]',
          items: [
            {
              label: `${this.translations.settings_useSystemLanguage()} (${this.getLangDisplay(
                this._translateService.browserLanguage()
              )})`,
              icon: this._translateService.isLanguage(null)
                ? 'i-[mdi--check]'
                : 'i-[mdi--translate]',
              command: () => this._translateService.setLanguage(null),
            },
            {
              separator: true,
            },
            {
              label: this.getLangDisplay('en'),
              icon: this._translateService.isLanguage('en') ? 'i-[mdi--check]' : 'i-[flag--us-1x1]',
              command: () => this._translateService.setLanguage('en'),
            },
            {
              label: this.getLangDisplay('de'),
              icon: this._translateService.isLanguage('de') ? 'i-[mdi--check]' : 'i-[flag--de-1x1]',
              command: () => this._translateService.setLanguage('de'),
            },
          ],
        },
      ],
    },
  ]);

  constructor() {
    const primengConfig = inject(PrimeNGConfig);
    primengConfig.ripple = true;
  }

  private getLangDisplay(lang: string) {
    switch (lang) {
      case 'en':
        return 'English';
      case 'de':
        return 'Deutsch';
      default:
        return lang;
    }
  }

  private getThemeDisplay(theme: string) {
    switch (theme) {
      case 'dark':
        return this.translations.settings_darkTheme();
      case 'light':
        return this.translations.settings_lightTheme();
      default:
        throw new Error(`Unknown theme: ${theme}`);
    }
  }
}
