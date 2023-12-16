import { SocialAuthService, SocialUser } from '@abacritt/angularx-social-login';
import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterOutlet } from '@angular/router';
import { ConfirmationService, MessageService, PrimeNGConfig } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';

import { FooterComponent } from './footer/footer.component';
import { MenuComponent } from './menu/menu.component';
import { environment } from '../../environments/environment';
import { AuthGuard } from '../../services/auth.guard';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    ConfirmDialogModule,
    CommonModule,
    FooterComponent,
    RouterOutlet,
    MenuComponent,
    ToastModule,
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  private readonly _socialAuthService = inject(SocialAuthService);

  protected socialUser = environment.authenticationRequired
    ? toSignal(this._socialAuthService.authState)
    : signal({} as SocialUser);
  protected isLoggedIn = computed(() => !!this.socialUser());
  protected authInit = computed(() => this.socialUser() !== undefined);

  constructor() {
    inject(AuthGuard).init();

    const primengConfig = inject(PrimeNGConfig);
    primengConfig.ripple = true;
  }
}
