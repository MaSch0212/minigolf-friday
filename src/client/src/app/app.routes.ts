import { inject } from '@angular/core';
import { CanActivateFn, Routes } from '@angular/router';
import { Store } from '@ngrx/store';

import { setTitleAction } from './+state/app';
import { AuthGuard } from './services/auth.guard';

function getCanActivate(guards: CanActivateFn[]) {
  return guards;
}

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: '/home',
  },
  {
    path: 'home',
    redirectTo: '/events',
  },
  {
    path: '',
    canActivate: getCanActivate([
      (_, state) => inject(AuthGuard).canActivate(state, { needsAdminRights: false }),
    ]),
    children: [
      {
        path: 'events',
        resolve: {
          title: getTitleResolver('nav_events', true),
        },
        loadComponent: () =>
          import('./components/player-events/player-events.component').then(
            ({ PlayerEventsComponent }) => PlayerEventsComponent
          ),
      },
      {
        path: 'events/:id',
        resolve: {
          title: getTitleResolver('nav_event', true),
        },
        loadComponent: () =>
          import(
            './components/player-events/player-event-details/player-event-details.component'
          ).then(({ PlayerEventDetailsComponent }) => PlayerEventDetailsComponent),
      },
    ],
  },
  {
    path: '',
    canActivate: getCanActivate([
      (_, state) => inject(AuthGuard).canActivate(state, { needsAdminRights: true }),
    ]),
    children: [
      {
        path: 'manage',
        resolve: {
          title: getTitleResolver('nav_manage', true),
        },
        children: [
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
              import('./components/users/users.component').then(
                ({ UsersComponent }) => UsersComponent
              ),
          },
          {
            path: 'events',
            resolve: {
              title: getTitleResolver('nav_events', true),
            },
            loadComponent: () =>
              import('./components/events/events.component').then(
                ({ EventsComponent }) => EventsComponent
              ),
          },
          {
            path: 'events/:id',
            resolve: {
              title: getTitleResolver('nav_event', true),
            },
            loadComponent: () =>
              import('./components/events/event-details/event-details.component').then(
                ({ EventDetailsComponent }) => EventDetailsComponent
              ),
          },
          {
            path: 'events/:eventId/timeslots/:timeslotId',
            resolve: {
              title: getTitleResolver('nav_eventTimeslot', true),
            },
            loadComponent: () =>
              import('./components/events/event-timeslot/event-timeslot.component').then(
                ({ EventTimeslotComponent }) => EventTimeslotComponent
              ),
          },
        ],
      },
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
