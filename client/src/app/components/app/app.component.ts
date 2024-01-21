import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ConfirmationService, MessageService, PrimeNGConfig } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';

import { FooterComponent } from './footer/footer.component';
import { MenuComponent } from './menu/menu.component';
import { AuthGuard } from '../../services/auth.guard';
import { AuthService } from '../../services/auth.service';
import { notNullish } from '../../utils/common.utils';

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
  private readonly _authService = inject(AuthService);

  protected authState = this._authService.authState;
  protected isLoggedIn = computed(() => notNullish(this.authState()?.token));
  protected authInit = computed(() => this.authState()?.isInitized === true);

  constructor() {
    inject(AuthGuard).init();

    const primengConfig = inject(PrimeNGConfig);
    primengConfig.ripple = true;
  }
}
