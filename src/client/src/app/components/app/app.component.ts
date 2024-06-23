import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, viewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterOutlet } from '@angular/router';
import { ConfirmationService, MessageService, PrimeNGConfig } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';

import { FooterComponent } from './footer/footer.component';
import { MenuComponent } from './menu/menu.component';
import { NotificationPromptDialogComponent } from './notification-prompt-dialog/notification-prompt-dialog.component';
import { environment } from '../../environments/environment';
import { AuthService } from '../../services/auth.service';
import { WebPushService } from '../../services/web-push.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    ConfirmDialogModule,
    CommonModule,
    FooterComponent,
    RouterOutlet,
    MenuComponent,
    NotificationPromptDialogComponent,
    ToastModule,
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  private readonly _authService = inject(AuthService);
  private readonly _webPushService = inject(WebPushService);

  private readonly _notificationPromptDialog = viewChild.required(
    NotificationPromptDialogComponent
  );

  protected isLoggedIn = this._authService.isAuthorized;
  protected version = environment.version;

  constructor() {
    const primengConfig = inject(PrimeNGConfig);
    primengConfig.ripple = true;

    this._webPushService.onPromptNotification
      .pipe(takeUntilDestroyed())
      .subscribe(() => this._notificationPromptDialog().open());
  }
}
