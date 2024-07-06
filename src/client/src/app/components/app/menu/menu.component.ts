import { animate, style, transition, trigger } from '@angular/animations';
import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { MenubarModule } from 'primeng/menubar';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { TooltipModule } from 'primeng/tooltip';

import { selectAppTitle } from '../../../+state/app';
import { AuthService } from '../../../services/auth.service';
import { RealtimeEventsService } from '../../../services/realtime-events.service';
import { TranslateService, TranslationKey } from '../../../services/translate.service';
import { UpdateService } from '../../../services/update.service';
import { chainSignals } from '../../../utils/signal.utils';

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [
    ButtonModule,
    CommonModule,
    MenubarModule,
    MenuModule,
    TooltipModule,
    OverlayPanelModule,
  ],
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
  private readonly _authService = inject(AuthService);
  private readonly _realtimeEventsService = inject(RealtimeEventsService);
  private readonly _updateService = inject(UpdateService);

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
  protected readonly newVersionAvailable = this._updateService.newVersionAvailable;

  protected readonly isServerConnected = this._realtimeEventsService.isConnected;

  protected readonly adminMenuItems = computed<MenuItem[]>(() => [
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
    },
    {
      label: this.translations.nav_settings(),
      icon: 'i-[mdi--cog]',
      routerLink: '/user-settings',
    },
  ]);

  protected updateApp() {
    location.reload();
  }
}
