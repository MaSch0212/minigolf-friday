import {
  BaseLoginProvider,
  FacebookLoginProvider,
  SocialAuthService,
  SocialAuthServiceConfig,
  SocialLoginModule,
  SocialUser,
} from '@abacritt/angularx-social-login';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import {
  APP_INITIALIZER,
  Injectable,
  OnDestroy,
  computed,
  effect,
  importProvidersFrom,
  inject,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { AuthApiService } from './auth-api.service';
import { AuthGuard } from './auth.guard';
import { AuthInterceptor } from './auth.interceptor';
import { WellKnownService } from './well-known.service';
import { environment } from '../environments/environment';
import { User, UserLoginType } from '../models/user';

export type AuthState = {
  token: string | null;
  user: User | null;
};

@Injectable()
export class AuthService implements OnDestroy {
  public static TOKEN_KEY = 'access_token';
  public static TOKEN_EXPIRATION_KEY = 'access_token_expiration';

  private readonly _api = inject(AuthApiService);
  private readonly _socialAuthService = inject(SocialAuthService);
  private readonly _router = inject(Router);

  private readonly _token = signal<string | null | undefined>(undefined);
  private readonly _user = signal<User | null | undefined>(undefined);
  private readonly _userLoginType = computed(() => this._user()?.loginType);
  private readonly _socialAuthState = toSignal(this._socialAuthService.authState);

  private _isTokenLoading = false;
  private _tokenRefreshTimeout?: number;

  public token = this._token.asReadonly();
  public user = this._user.asReadonly();
  public isAuthorized = computed(() => !!this._token() && !!this._user());

  constructor() {
    effect(
      () => {
        const socialUser = this._socialAuthState();
        if (socialUser) {
          this.refreshToken();
        } else if (this._userLoginType() === 'facebook') {
          this._token.set(null);
          this._user.set(null);
        }
      },
      { allowSignalWrites: true }
    );

    effect(() => {
      const token = this._token();
      if (token) {
        localStorage.setItem(AuthService.TOKEN_KEY, token);
      } else if (token === null) {
        this.clearTokenRefreshTimeout();
        localStorage.removeItem(AuthService.TOKEN_KEY);
        localStorage.removeItem(AuthService.TOKEN_EXPIRATION_KEY);
      }
    });
  }

  public async initialize() {
    if (this._token() !== undefined || this._user() !== undefined) {
      throw new Error('AuthService already initialized');
    }

    if (!environment.authenticationRequired) {
      this._token.set('abc');
      this._user.set({ name: 'Stub User', id: 'abcdef', isAdmin: true, loginType: 'email' });
      return;
    }

    // Wait for the social auth service to initialize
    await firstValueFrom(this._socialAuthService.authState);

    const token = localStorage.getItem(AuthService.TOKEN_KEY);
    const tokenExpiration = localStorage.getItem(AuthService.TOKEN_EXPIRATION_KEY);
    if (token && tokenExpiration && new Date(tokenExpiration) > new Date()) {
      await this.refreshToken(token);
      return;
    }

    this._token.set(null);
    this._user.set(null);
  }

  public async refreshToken(oldToken?: string) {
    if (!environment.authenticationRequired || this._isTokenLoading) return;

    this._isTokenLoading = true;
    this.clearTokenRefreshTimeout();
    try {
      const response = await firstValueFrom(this._api.getAccessToken(oldToken));
      this.handleTokenResponse(response);
    } catch (err) {
      await this._socialAuthService.signOut().catch();
      this._token.set(null);
      this._user.set(null);
    } finally {
      this._isTokenLoading = false;
    }
  }

  public async signIn(loginType: 'facebook'): Promise<void>;
  public async signIn(loginType: 'email', email: string, password: string): Promise<void>;
  public async signIn(loginType: UserLoginType, email?: string, password?: string): Promise<void> {
    if (!environment.authenticationRequired) return;

    if (loginType === 'facebook') {
      await this._socialAuthService.signIn(FacebookLoginProvider.PROVIDER_ID);
      await this.refreshToken();
    } else if (loginType === 'email' && email && password) {
      const response = await firstValueFrom(this._api.login({ email, password }));
      this.handleTokenResponse(response);
    }
  }

  public async signOut() {
    if (!environment.authenticationRequired) return;

    const loginType = this._user()?.loginType;
    if (loginType === 'facebook') {
      await this._socialAuthService.signOut();
    } else {
      this._token.set(null);
      this._user.set(null);
    }

    this._router.navigate(['/login']);
  }

  public async register(email: string, name: string, password: string) {
    if (!environment.authenticationRequired) return;

    const response = await firstValueFrom(this._api.register({ email, name, password }));
    this.handleTokenResponse(response);
  }

  public ngOnDestroy(): void {
    this.clearTokenRefreshTimeout();
  }

  private handleTokenResponse(response: { token: string; expiresAt: string; user: User }) {
    this._token.set(response.token);
    this._user.set(response.user);
    localStorage.setItem(AuthService.TOKEN_EXPIRATION_KEY, response.expiresAt);
    this.updateTokenRefreshTimeout(response.expiresAt);
  }

  private updateTokenRefreshTimeout(expiration: string) {
    this._tokenRefreshTimeout = setTimeout(
      () => this.refreshToken(),
      Math.max(10000, new Date(expiration).getTime() - Date.now() - 1000 * 60)
    );
  }

  private clearTokenRefreshTimeout() {
    if (this._tokenRefreshTimeout) clearTimeout(this._tokenRefreshTimeout);
    this._tokenRefreshTimeout = undefined;
  }
}

@Injectable()
class MyFacebookLoginProvider extends BaseLoginProvider {
  private readonly _wellKnownService = inject(WellKnownService);
  private facebookProvider?: FacebookLoginProvider;

  public override async initialize(): Promise<void> {
    if (!environment.authenticationRequired) return;

    const wellKnown = await firstValueFrom(this._wellKnownService.getWellKnown());
    this.facebookProvider = new FacebookLoginProvider(wellKnown.facebookAppId);
    await this.facebookProvider.initialize();
  }

  public override getLoginStatus(): Promise<SocialUser> {
    if (!environment.authenticationRequired) {
      return new Promise(r => r({} as SocialUser));
    }

    if (!this.facebookProvider) {
      throw new Error('Facebook provider not initialized');
    }
    return this.facebookProvider.getLoginStatus();
  }

  public override signIn(signInOptions?: object | undefined): Promise<SocialUser> {
    if (!environment.authenticationRequired) {
      return new Promise(r => r({} as SocialUser));
    }

    if (!this.facebookProvider) {
      throw new Error('Facebook provider not initialized');
    }
    return this.facebookProvider.signIn(signInOptions);
  }

  public override signOut(): Promise<void> {
    if (!environment.authenticationRequired) {
      return new Promise(r => r());
    }

    if (!this.facebookProvider) {
      throw new Error('Facebook provider not initialized');
    }
    return this.facebookProvider.signOut();
  }
}

export function provideAuth() {
  return [
    importProvidersFrom(SocialLoginModule),
    AuthService,
    AuthGuard,
    MyFacebookLoginProvider,
    {
      provide: 'SocialAuthServiceConfig',
      useValue: {
        autoLogin: true,
        providers: [
          {
            id: FacebookLoginProvider.PROVIDER_ID,
            provider: MyFacebookLoginProvider,
          },
        ],
      } as SocialAuthServiceConfig,
    },
    {
      provide: HTTP_INTERCEPTORS,
      multi: true,
      useClass: AuthInterceptor,
    },
    {
      provide: APP_INITIALIZER,
      multi: true,
      useFactory: (authService: AuthService) => () => authService.initialize(),
      deps: [AuthService],
    },
  ];
}
