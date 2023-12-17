import {
  BaseLoginProvider,
  FacebookLoginProvider,
  SocialAuthService,
  SocialAuthServiceConfig,
  SocialLoginModule,
  SocialUser,
} from '@abacritt/angularx-social-login';
import { HttpClient } from '@angular/common/http';
import { Injectable, importProvidersFrom, inject } from '@angular/core';
import {
  Observable,
  Subject,
  catchError,
  combineLatest,
  firstValueFrom,
  map,
  of,
  shareReplay,
  startWith,
  switchMap,
} from 'rxjs';

import { AuthGuard } from './auth.guard';
import { WellKnownService } from './well-known.service';

type GetAuthorizationResponse = {
  isAuthorized: boolean;
  reason: string;
};

export type AuthState = {
  isInitized: boolean;
  isAuthorized?: boolean | null;
  user?: SocialUser;
};

@Injectable()
export class AuthService {
  private readonly _http = inject(HttpClient);
  private readonly _socialAuthService = inject(SocialAuthService);
  private readonly _refreshAuthorized$ = new Subject<void>();

  public readonly authState: Observable<AuthState> = combineLatest([
    this._socialAuthService.initState,
    this._socialAuthService.authState,
  ]).pipe(
    switchMap(([isInitized, user]) =>
      isInitized && user
        ? this._refreshAuthorized$.pipe(
            startWith(void 0),
            switchMap(() =>
              this.getIsAuthorized().pipe(
                catchError(() => of(null)),
                map(isAuthorized => ({ isInitized, user, isAuthorized }))
              )
            )
          )
        : of({ isInitized })
    ),
    shareReplay(1)
  );

  public refreshAuthorized() {
    this._refreshAuthorized$.next();
  }

  public signIn() {
    return this._socialAuthService.signIn(FacebookLoginProvider.PROVIDER_ID);
  }

  public signOut() {
    return this._socialAuthService.signOut();
  }

  private getIsAuthorized() {
    return this._http
      .get<GetAuthorizationResponse>('/api/auth')
      .pipe(map(response => response.isAuthorized));
  }
}

@Injectable()
class MyFacebookLoginProvider extends BaseLoginProvider {
  private readonly _wellKnownService = inject(WellKnownService);
  private facebookProvider?: FacebookLoginProvider;

  public override async initialize(): Promise<void> {
    const wellKnown = await firstValueFrom(this._wellKnownService.getWellKnown());
    this.facebookProvider = new FacebookLoginProvider(wellKnown.facebookAppId);
    await this.facebookProvider.initialize();
  }

  public override getLoginStatus(): Promise<SocialUser> {
    if (!this.facebookProvider) {
      throw new Error('Facebook provider not initialized');
    }
    return this.facebookProvider.getLoginStatus();
  }

  public override signIn(signInOptions?: object | undefined): Promise<SocialUser> {
    if (!this.facebookProvider) {
      throw new Error('Facebook provider not initialized');
    }
    return this.facebookProvider.signIn(signInOptions);
  }

  public override signOut(): Promise<void> {
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
  ];
}
