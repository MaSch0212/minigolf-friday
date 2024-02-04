import { Injectable, inject } from '@angular/core';
import { Router, RouterStateSnapshot } from '@angular/router';

import { AuthService } from './auth.service';

@Injectable()
export class AuthGuard {
  private readonly _router = inject(Router);
  private readonly _authService = inject(AuthService);

  public async canActivate(state: RouterStateSnapshot, options: { needsAdminRights: boolean }) {
    const user = this._authService.user();
    if (!user) {
      this.navigateToLogin(state.url);
      return false;
    }
    if (options.needsAdminRights && !user.isAdmin) {
      this.navigateToUnauthorized(state.url);
      return false;
    }
    return true;
  }

  private navigateToLogin(returnUrl: string): void {
    this._router.navigate(['/login'], { queryParams: { returnUrl } });
  }

  private navigateToUnauthorized(url: string): void {
    this._router.navigate(['/unauthorized'], { queryParams: { url } });
  }
}
