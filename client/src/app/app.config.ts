import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { ApplicationConfig } from '@angular/core';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { provideRouterStore, routerReducer } from '@ngrx/router-store';
import { provideStore } from '@ngrx/store';
import { provideStoreDevtools } from '@ngrx/store-devtools';

import { provideAppState } from './+state/app';
import { provideEventsState } from './+state/events';
import { provideMapsState } from './+state/maps';
import { providePlayerEventsState } from './+state/player-events';
import { provideUsersState } from './+state/users';
import { routes } from './app.routes';
import { environment } from './environments/environment';
import { provideAuth } from './services/auth.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideAnimations(),
    provideHttpClient(withInterceptorsFromDi()),
    provideStore({
      router: routerReducer,
    }),
    provideRouterStore(),
    provideStoreDevtools(),
    provideAppState(),
    provideUsersState(),
    provideMapsState(),
    provideEventsState(),
    providePlayerEventsState(),
    environment.getProviders(),
    provideAuth(),
  ],
};
