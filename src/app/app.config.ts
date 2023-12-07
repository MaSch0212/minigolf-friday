import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { ApplicationConfig } from '@angular/core';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { provideRouterStore, routerReducer } from '@ngrx/router-store';
import { provideStore } from '@ngrx/store';
import { provideStoreDevtools } from '@ngrx/store-devtools';

import { provideAppState } from './+state/app';
import { provideMapsState } from './+state/maps';
import { providePlayersState } from './+state/players';
import { getAdditionalProviders } from './additional-providers';
import { routes } from './app.routes';

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
    providePlayersState(),
    provideMapsState(),
    getAdditionalProviders(),
  ],
};
