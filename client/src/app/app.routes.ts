import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, RouterStateSnapshot, Routes } from '@angular/router';
import { Store } from '@ngrx/store';

import { setTitleAction } from './+state/app';
import { AuthGuard } from './services/auth.guard';

export const routes: Routes = [
  {
    path: 'home',
    resolve: {
      title: getTitleResolver(undefined),
    },
    loadComponent: () =>
      import('./components/home/home.component').then(({ HomeComponent }) => HomeComponent),
    canActivate: [
      (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) =>
        inject(AuthGuard).canActivate(route, state),
    ],
  },
  {
    path: 'manage',
    resolve: {
      title: getTitleResolver('nav_manage', true),
    },
    children: [
      {
        path: 'players',
        resolve: {
          title: getTitleResolver('nav_players', true),
        },
        loadComponent: () =>
          import('./components/players/players.component').then(
            ({ PlayersComponent }) => PlayersComponent
          ),
      },
      {
        path: 'maps',
        resolve: {
          title: getTitleResolver('nav_maps', true),
        },
        loadComponent: () =>
          import('./components/maps/maps.component').then(({ MapsComponent }) => MapsComponent),
      },
      {
        path: 'users',
        resolve: {
          title: getTitleResolver('nav_users', true),
        },
        loadComponent: () =>
          import('./components/users/users.component').then(({ UsersComponent }) => UsersComponent),
      },
    ],
    canActivateChild: [
      (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) =>
        inject(AuthGuard).canActivate(route, state),
    ],
  },
  {
    path: 'invite/:inviteId',
    loadComponent: () =>
      import('./components/users/redeem-invite/redeem-invite.component').then(
        ({ RedeemInviteComponent }) => RedeemInviteComponent
      ),
    canActivate: [
      (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) =>
        inject(AuthGuard).canActivate(route, state, true),
    ],
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./components/login/login.component').then(({ LoginComponent }) => LoginComponent),
  },
  {
    path: 'unauthorized',
    loadComponent: () =>
      import('./components/app/unauthorized/unauthorized.component').then(
        ({ UnauthorizedComponent }) => UnauthorizedComponent
      ),
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: '/home',
  },
  {
    path: '**',
    pathMatch: 'full',
    resolve: {
      title: getTitleResolver(undefined),
    },
    loadComponent: () =>
      import('./components/app/not-found/not-found.component').then(
        ({ NotFoundComponent }) => NotFoundComponent
      ),
  },
];

function getTitleResolver(title: string | undefined, translate?: boolean) {
  return () => inject(Store).dispatch(setTitleAction({ title, translate }));
}
