import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'home',
    pathMatch: 'full',
    loadComponent: () =>
      import('./components/home/home.component').then(({ HomeComponent }) => HomeComponent),
  },
  {
    path: 'players',
    pathMatch: 'full',
    loadComponent: () =>
      import('./components/players/players.component').then(
        ({ PlayersComponent }) => PlayersComponent
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
    loadComponent: () =>
      import('./components/not-found/not-found.component').then(
        ({ NotFoundComponent }) => NotFoundComponent
      ),
  },
];
