import {
  BaseLoginProvider,
  FacebookLoginProvider,
  SocialAuthService,
  SocialAuthServiceConfig,
  SocialLoginModule,
  SocialUser,
} from '@abacritt/angularx-social-login';
import { HTTP_INTERCEPTORS, HttpClient } from '@angular/common/http';
import {
  Injectable,
  OnDestroy,
  computed,
  effect,
  importProvidersFrom,
  inject,
  signal,
} from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { firstValueFrom } from 'rxjs';

import { AuthGuard } from './auth.guard';
import { AuthInterceptor } from './auth.interceptor';
import { WellKnownService } from './well-known.service';
import { environment } from '../environments/environment';

type GetAccessTokenResponse = {
  token: string;
  expiresAt: string;
};

export type AuthState = {
  isInitized: boolean;
  token?: string | null;
  user?: SocialUser;
};

@Injectable()
export class AuthService implements OnDestroy {
  public static TOKEN_KEY = 'access_token';

  private readonly _http = inject(HttpClient);
  private readonly _socialAuthService = inject(SocialAuthService);

  private readonly _token = signal<string | null | undefined>(undefined);
  private readonly _socialInitState = toSignal(this._socialAuthService.initState);
  private readonly _socialAuthState = toSignal(this._socialAuthService.authState);
  private readonly _tokenInitialized = signal(false);

  private _isTokenLoading = false;
  private _tokenRefreshTimeout?: number;

  public authState = computed<AuthState>(() =>
    environment.authenticationRequired
      ? {
          isInitized:
            this._socialAuthState() !== undefined &&
            this._socialInitState() === true &&
            this._tokenInitialized(),
          user: this._socialAuthState(),
          token: this._token(),
        }
      : { isInitized: true, user: {} as SocialUser, token: 'abc' }
  );
  public authState$ = toObservable(this.authState);

  public token = computed(() => this.authState().token);

  constructor() {
    effect(
      () => {
        const user = this._socialAuthState();
        if (user === undefined) return;
        if (user) {
          this.refreshToken();
        } else {
          this._tokenInitialized.set(true);
          this.clearTokenRefreshTimeout();
        }
      },
      { allowSignalWrites: true }
    );

    effect(() => {
      const token = this._token();
      if (token) {
        sessionStorage.setItem(AuthService.TOKEN_KEY, token);
      } else {
        sessionStorage.removeItem(AuthService.TOKEN_KEY);
      }
    });
  }

  public async refreshToken() {
    if (!environment.authenticationRequired || this._isTokenLoading) return;

    this._isTokenLoading = true;
    this.clearTokenRefreshTimeout();
    try {
      const response = await firstValueFrom(
        this._http.post<GetAccessTokenResponse>('/api/auth/token', {})
      );
      this._token.set(response.token);
      this._tokenRefreshTimeout = setTimeout(
        () => this.refreshToken(),
        Math.max(10000, new Date(response.expiresAt).getTime() - Date.now() - 1000 * 60)
      );
    } catch (err) {
      this._token.set(null);
    } finally {
      this._tokenInitialized.set(true);
      this._isTokenLoading = false;
    }
  }

  public async signIn() {
    if (!environment.authenticationRequired) return;
    await this._socialAuthService.signIn(FacebookLoginProvider.PROVIDER_ID);
  }

  public async signOut() {
    if (!environment.authenticationRequired) return;
    await this._socialAuthService.signOut();
  }

  public ngOnDestroy(): void {
    this.clearTokenRefreshTimeout();
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
  ];
}
