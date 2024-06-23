import {
  EventEmitter,
  Injectable,
  OnDestroy,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { Router } from '@angular/router';

import {
  AuthTokenInfo,
  getAuthTokenInfo,
  getLoginToken,
  setAuthTokenInfo,
  setLoginToken,
} from './storage';
import { AuthenticationService } from '../api/services';
import { environment } from '../environments/environment';
import { assertBody } from '../utils/http.utils';

export type SignInResult = 'success' | 'invalid-token' | 'error';

@Injectable({ providedIn: 'root' })
export class AuthService implements OnDestroy {
  private readonly _api = inject(AuthenticationService);
  private readonly _router = inject(Router);

  private readonly _token = signal<AuthTokenInfo | null | undefined>(undefined);

  private _tokenRefreshTimeout?: any;

  public readonly token = this._token.asReadonly();
  public readonly user = computed(() => this.token()?.user);
  public readonly isAuthorized = computed(() => !!this._token());
  public readonly onBeforeSignOut = new EventEmitter<void>();

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

  public async init() {
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

    this.onBeforeSignOut.emit();
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
