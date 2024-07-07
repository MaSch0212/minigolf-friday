import { Injectable, OnDestroy, computed, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { filter, Unsubscribable } from 'rxjs';

import { Logger } from './logger.service';
import {
  AuthTokenInfo,
  getAuthTokenInfo,
  getLoginToken,
  setAuthTokenInfo,
  setLoginToken,
  setLogLevelEnabled,
} from './storage';
import { AuthenticationService } from '../api/services';
import { environment } from '../environments/environment';
import { onDocumentVisibilityChange$ } from '../utils/event.utils';

import type { Eruda } from 'eruda';

let eruda: Eruda | undefined;

export type SignInResult = 'success' | 'invalid-token' | 'error';

@Injectable({ providedIn: 'root' })
export class AuthService implements OnDestroy {
  private readonly _api = inject(AuthenticationService);
  private readonly _router = inject(Router);

  private readonly _beforeSignOut: (() => Promise<void>)[] = [];
  private readonly _token = signal<AuthTokenInfo | null | undefined>(undefined);

  private _tokenRefreshTimeout?: ReturnType<typeof setTimeout>;

  public readonly token = this._token.asReadonly();
  public readonly user = computed(() => this.token()?.user);
  public readonly isAuthorized = computed(() => !!this._token());

  constructor() {
    effect(() => {
      const token = this._token();

      const isDev = token?.user?.roles.includes('developer') ?? false;
      setLogLevelEnabled('debug', isDev);
      if (isDev) {
        (eruda ? Promise.resolve(eruda) : import('eruda').then(x => (eruda = x.default))).then(x =>
          x.init()
        );
      } else {
        eruda?.destroy();
      }

      Logger.logDebug('AuthService', 'Token changed', { token });

      if (token === undefined) return;
      setAuthTokenInfo(token);
      if (token) {
        this.updateTokenRefreshTimeout(token.expiresAt);
      } else {
        this.clearTokenRefreshTimeout();
      }
    });

    onDocumentVisibilityChange$()
      .pipe(
        takeUntilDestroyed(),
        filter(isVisible => isVisible)
      )
      .subscribe(() => {
        this.ensureTokenNotExpired();
      });
  }

  public async init() {
    Logger.logDebug('AuthService', 'Initializing AuthService');
    if (this._token() !== undefined) {
      throw new Error('AuthService already initialized');
    }

    if (!environment.authenticationRequired) {
      Logger.logDebug('AuthService', 'Authentication disabled, using stub token');
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
      Logger.logDebug('AuthService', 'Using stored auth token');
      this._token.set(token);
      return;
    }

    const loginToken = getLoginToken();
    if (loginToken) {
      Logger.logDebug('AuthService', 'Using stored login token');
      if ((await this.signIn(loginToken)) === 'success') {
        return;
      }
    }

    Logger.logDebug('AuthService', 'No stored token, setting token to null');
    this._token.set(null);
  }

  public async ensureTokenNotExpired() {
    const expiration = this._token()?.expiresAt;
    if (!expiration || expiration.getTime() < Date.now() + 60 * 1000) {
      Logger.logDebug('AuthService', 'Token expired, refreshing');
      await this.refreshToken();
    } else if (expiration) {
      Logger.logDebug('AuthService', 'Token expires at', expiration);
      this.updateTokenRefreshTimeout(expiration);
    }
  }

  public async signIn(loginToken: string): Promise<SignInResult> {
    if (!environment.authenticationRequired) return 'success';

    Logger.logDebug('AuthService', 'Signing in with login token');
    const response = await this._api.getToken({ body: { loginToken } });
    Logger.logDebug('AuthService', 'Sign in response', response);
    if (!response.ok) {
      if (response.status === 401) {
        Logger.logError('AuthService', 'Invalid login token', { cause: response });
        return 'invalid-token';
      } else {
        Logger.logError('AuthService', 'Error while signing in', { cause: response });
        return 'error';
      }
    }

    if (!response.body) {
      Logger.logError('AuthService', 'No body in response', { response });
      return 'error';
    }

    setLoginToken(loginToken);
    this._token.set({
      token: response.body.token,
      expiresAt: new Date(response.body.tokenExpiration),
      user: response.body.user,
    });
    return 'success';
  }

  public async signOut() {
    if (!environment.authenticationRequired) return;

    Logger.logDebug('AuthService', 'Signing out');

    try {
      Logger.logDebug('AuthService', 'Executing before sign out actions');
      await Promise.all(this._beforeSignOut.map(x => x()));
    } catch (error) {
      Logger.logError('AuthService', 'Error while executing before sign out actions', error);
    }

    setLoginToken(null);
    this._token.set(null);

    this._router.navigate(['/login']);
  }

  public onBeforeSignOut(action: () => Promise<void>): Unsubscribable {
    Logger.logDebug('AuthService', 'Adding before sign out action', Error().stack);
    this._beforeSignOut.push(action);
    return {
      unsubscribe: () => {
        const index = this._beforeSignOut.indexOf(action);
        if (index >= 0) {
          this._beforeSignOut.splice(index, 1);
        }
      },
    };
  }

  public ngOnDestroy(): void {
    this.clearTokenRefreshTimeout();
  }

  private updateTokenRefreshTimeout(expiration: Date) {
    Logger.logDebug('AuthService', 'Updating token refresh timeout', expiration);
    if (this._tokenRefreshTimeout) clearTimeout(this._tokenRefreshTimeout);
    this._tokenRefreshTimeout = setTimeout(
      () => {
        this.refreshToken();
      },
      Math.max(10000, expiration.getTime() - Date.now() - 1000 * 60)
    );
  }

  private async refreshToken() {
    Logger.logDebug('AuthService', 'Refreshing token');
    const loginToken = getLoginToken();
    if (loginToken) {
      await this.signIn(loginToken);
    } else {
      await this.signOut();
    }
  }

  private clearTokenRefreshTimeout() {
    Logger.logDebug('AuthService', 'Clearing token refresh timeout');
    if (this._tokenRefreshTimeout) clearTimeout(this._tokenRefreshTimeout);
    this._tokenRefreshTimeout = undefined;
  }
}
