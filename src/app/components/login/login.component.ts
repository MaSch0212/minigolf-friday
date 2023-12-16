import { SocialAuthService, FacebookLoginProvider } from '@abacritt/angularx-social-login';
import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { TooltipModule } from 'primeng/tooltip';

import { InterpolatePipe } from '../../directives/interpolate.pipe';
import { ThemeService } from '../../services/theme.service';
import { TranslateService } from '../../services/translate.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ButtonModule, CommonModule, InterpolatePipe, MenuModule, TooltipModule],
  templateUrl: './login.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly _socialAuthService = inject(SocialAuthService);
  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);

  protected translations = inject(TranslateService).translations;
  protected languageItems = inject(TranslateService).languageItems;
  protected themeItems = inject(ThemeService).themeItems;

  protected loginWithFacebook(): void {
    this._socialAuthService.signIn(FacebookLoginProvider.PROVIDER_ID);
  }
}
