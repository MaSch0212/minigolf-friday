import { HTTP_INTERCEPTORS } from '@angular/common/http';
import {
  APP_INITIALIZER,
  Injectable,
  OnDestroy,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { Router } from '@angular/router';

import { AuthGuard } from './auth.guard';
import { AuthInterceptor } from './auth.interceptor';
import {
  AuthTokenInfo,
  getAuthTokenInfo,
  getLoginToken,
  setAuthTokenInfo,
  setLoginToken,
} from './storage';
import { WebPushService } from './web-push.service';
import { AuthenticationService } from '../api/services';
import { environment } from '../environments/environment';
import { assertBody } from '../utils/http.utils';

export type SignInResult = 'success' | 'invalid-token' | 'error';

@Injectable()
export class AuthService implements OnDestroy {
  private readonly _api = inject(AuthenticationService);
  private readonly _router = inject(Router);
  private readonly _webPushService = inject(WebPushService);

  private readonly _token = signal<AuthTokenInfo | null | undefined>(undefined);

  private _tokenRefreshTimeout?: any;

  public token = this._token.asReadonly();
  public user = computed(() => this.token()?.user);
  public isAuthorized = computed(() => !!this._token());

  constructor() {
    effect(() => {
      const token = this._token();
      if (token === undefined) return;
      setAuthTokenInfo(token);
      if (token) {
        this.updateTokenRefreshTimeout(token.expiresAt);
      } else {
        this.clearTokenRefreshTimeout();
      }
    });
  }

  public async initialize() {
    if (this._token() !== undefined) {
      throw new Error('AuthService already initialized');
    }

    if (!environment.authenticationRequired) {
      this._token.set({
        token: 'abc',
        expiresAt: new Date(Date.now() + 1000 * 60 * 60 * 24),
        user: {
          alias: 'Stub User',
          id: 'abcdef',
          roles: ['player', 'admin'],
          playerPreferences: { avoid: [], prefer: [] },
        },
      });
      return;
    }

    const token = getAuthTokenInfo();
    if (token && token.expiresAt.getTime() > Date.now() + 15 * 60 * 1000) {
      this._token.set(token);
      return;
    }

    const loginToken = getLoginToken();
    if (loginToken && (await this.signIn(loginToken)) === 'success') {
      return;
    }

    this._token.set(null);
  }

  public async signIn(loginToken: string): Promise<SignInResult> {
    if (!environment.authenticationRequired) return 'success';

    const response = await this._api.getToken({ body: { loginToken } });
    if (!response.ok) {
      if (response.status === 401) {
        console.error('Invalid login token', { cause: response });
        return 'invalid-token';
      } else {
        console.error('Error while signing in', { cause: response });
        return 'error';
      }
    }

    const body = assertBody(response);
    setLoginToken(loginToken);
    this._token.set({
      token: body.token,
      expiresAt: new Date(body.tokenExpiration),
      user: body.user,
    });
    return 'success';
  }

  public async signOut() {
    if (!environment.authenticationRequired) return;

    await this._webPushService.disable(true);
    setLoginToken(null);
    this._token.set(null);

    this._router.navigate(['/login']);
  }

  public ngOnDestroy(): void {
    this.clearTokenRefreshTimeout();
  }

  private updateTokenRefreshTimeout(expiration: Date) {
    this._tokenRefreshTimeout = setTimeout(
      () => {
        const loginToken = getLoginToken();
        if (loginToken) {
          this.signIn(loginToken);
        } else {
          this.signOut();
        }
      },
      Math.max(10000, expiration.getTime() - Date.now() - 1000 * 60)
    );
  }

  private clearTokenRefreshTimeout() {
    if (this._tokenRefreshTimeout) clearTimeout(this._tokenRefreshTimeout);
    this._tokenRefreshTimeout = undefined;
  }
}

export function provideAuth() {
  return [
    AuthService,
    AuthGuard,
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
