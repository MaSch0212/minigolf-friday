import { DestroyRef, Injectable, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ActivatedRouteSnapshot,
  CanActivateFn,
  Router,
  RouterStateSnapshot,
} from '@angular/router';
import { filter, firstValueFrom } from 'rxjs';

import { AuthService } from './auth.service';

interface Guard {
  canActivate: CanActivateFn;
}

@Injectable()
export class AuthGuard implements Guard {
  private readonly _destroyRef = inject(DestroyRef);
  private readonly _router = inject(Router);
  private readonly _authService = inject(AuthService);

  public async canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot,
    allowUnAuthorized = false
  ) {
    const authState = await firstValueFrom(
      this._authService.authState$.pipe(filter(x => x.isInitized))
    );

    const needsAdmin = route.data['allowNonAdmin'] !== true;

    return !!authState.user && (!!authState.token || allowUnAuthorized);
  }

  public init(): void {
    this._authService.authState$
      .pipe(
        takeUntilDestroyed(this._destroyRef),
        filter(x => x.isInitized)
      )
      .subscribe(state => {
        const url = new URL(window.location.href);
        const isLoginRoute = url.pathname === '/login';
        const isUnauthorizedRoute = url.pathname === '/unauthorized';
        const isInviteRoute = url.pathname.startsWith('/invite');
        if (!state.user && !isLoginRoute) {
          this.navigateToLogin(this._router.routerState.snapshot.url);
        } else if (state.user && !state.token && !isUnauthorizedRoute && !isInviteRoute) {
          this.navigateToUnauthorized();
        } else if (state.token && isLoginRoute) {
          const returnUrl = url.searchParams.get('returnUrl') || '/';
          this._router.navigate([returnUrl]);
        }
      });
  }

  private navigateToLogin(returnUrl: string): void {
    this._router.navigate(['/login'], { queryParams: { returnUrl } });
  }

  private navigateToUnauthorized(): void {
    this._router.navigate(['/unauthorized']);
  }
}
