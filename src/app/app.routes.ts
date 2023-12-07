import { inject } from '@angular/core';
import { Routes } from '@angular/router';
import { Store } from '@ngrx/store';

import { setTitleAction } from './+state/app';

export const routes: Routes = [
  {
    path: 'home',
    resolve: {
      title: getTitleResolver(undefined),
    },
    loadComponent: () =>
      import('./components/home/home.component').then(({ HomeComponent }) => HomeComponent),
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
    ],
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
      import('./components/not-found/not-found.component').then(
        ({ NotFoundComponent }) => NotFoundComponent
      ),
  },
];

function getTitleResolver(title: string | undefined, translate?: boolean) {
  return () => inject(Store).dispatch(setTitleAction({ title, translate }));
}
