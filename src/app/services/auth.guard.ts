import { SocialAuthService } from '@abacritt/angularx-social-login';
import { DestroyRef, Injectable, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ActivatedRouteSnapshot,
  CanActivateFn,
  Router,
  RouterStateSnapshot,
} from '@angular/router';
import { firstValueFrom, map } from 'rxjs';

import { environment } from '../environments/environment';

interface Guard {
  canActivate: CanActivateFn;
}

@Injectable({ providedIn: 'root' })
export class AuthGuard implements Guard {
  private readonly _destroyRef = inject(DestroyRef);
  private readonly _router = inject(Router);
  private readonly _socialAuthService = inject(SocialAuthService);

  public async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    if (!environment.authenticationRequired) return true;

    const user = await firstValueFrom(this._socialAuthService.authState);
    if (!user) {
      this.navigateToLogin(state.url);
    }
    return !!user;
  }

  public init(): void {
    if (!environment.authenticationRequired) return;

    this._socialAuthService.authState
      .pipe(
        takeUntilDestroyed(this._destroyRef),
        map(user => !!user)
      )
      .subscribe(isLoggedIn => {
        const url = new URL(`http://dummy${this._router.routerState.snapshot.url}`);
        const isLoginRoute = url.pathname === '/login';
        if (!isLoggedIn && !isLoginRoute) {
          this.navigateToLogin(this._router.routerState.snapshot.url);
        }
        if (isLoggedIn && isLoginRoute) {
          const returnUrl = url.searchParams.get('returnUrl') || '/';
          this._router.navigate([returnUrl]);
        }
      });
  }

  private navigateToLogin(returnUrl: string): void {
    this._router.navigate(['/login'], { queryParams: { returnUrl } });
  }
}
