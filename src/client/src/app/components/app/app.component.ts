import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { ConfirmationService, MessageService, PrimeNGConfig } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';

import { FooterComponent } from './footer/footer.component';
import { MenuComponent } from './menu/menu.component';
import { AuthService } from '../../services/auth.service';

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
  private readonly _router = inject(Router);

  protected isLoggedIn = this._authService.isAuthorized;
  protected version = (window as any)['minigolfFridayVersion'];

  constructor() {
    const primengConfig = inject(PrimeNGConfig);
    primengConfig.ripple = true;
  }
}
