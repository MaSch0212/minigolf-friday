import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { MenubarModule } from 'primeng/menubar';

import { ThemeService } from '../../services/theme.service';
import { TranslateService } from '../../services/translate.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, MenubarModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  private readonly _translateService = inject(TranslateService);
  private readonly _themeService = inject(ThemeService);

  protected translations = this._translateService.translations;
  protected menuItems = computed<MenuItem[]>(() => [
    {
      label: this.translations.menu_home(),
      icon: 'mdi mdi-home',
      routerLink: '/home',
    },
    {
      label: this.translations.menu_players(),
      icon: 'mdi mdi-account-multiple',
      routerLink: '/players',
    },
    {
      label: this.translations.menu_settings(),
      icon: 'mdi mdi-cog',
      items: [
        {
          label: this.translations.menu_theme(),
          icon: 'mdi mdi-theme-light-dark',
          items: [
            {
              label: `${this.translations.menu_useSystemTheme()} (${this.getThemeDisplay(
                this._themeService.isDarkColorSchemePrefered() ? 'dark' : 'light'
              )})`,
              icon: `mdi ${
                this._themeService.isTheme(null) ? 'mdi-check' : 'mdi-theme-light-dark'
              }`,
              command: () => this._themeService.setTheme(null),
            },
            {
              separator: true,
            },
            {
              label: this.translations.menu_darkTheme(),
              icon: `mdi ${this._themeService.isTheme('dark') ? 'mdi-check' : 'mdi-weather-night'}`,
              command: () => this._themeService.setTheme('dark'),
            },
            {
              label: this.translations.menu_lightTheme(),
              icon: `mdi ${
                this._themeService.isTheme('light') ? 'mdi-check' : 'mdi-weather-sunny'
              }`,
              command: () => this._themeService.setTheme('light'),
            },
          ],
        },
        {
          label: this.translations.menu_language(),
          icon: 'mdi mdi-translate',
          items: [
            {
              label: `${this.translations.menu_useSystemLanguage()} (${this.getLangDisplay(
                this._translateService.browserLanguage()
              )})`,
              icon: `mdi ${
                this._translateService.isLanguage(null) ? 'mdi-check' : 'mdi-translate'
              }`,
              command: () => this._translateService.setLanguage(null),
            },
            {
              separator: true,
            },
            {
              label: this.getLangDisplay('en'),
              icon: this._translateService.isLanguage('en') ? 'mdi mdi-check' : 'fi fi-us',
              command: () => this._translateService.setLanguage('en'),
            },
            {
              label: this.getLangDisplay('de'),
              icon: this._translateService.isLanguage('de') ? 'mdi mdi-check' : 'fi fi-de',
              command: () => this._translateService.setLanguage('de'),
            },
          ],
        },
      ],
    },
  ]);

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
        return this.translations.menu_darkTheme();
      case 'light':
        return this.translations.menu_lightTheme();
      default:
        throw new Error(`Unknown theme: ${theme}`);
    }
  }
}
