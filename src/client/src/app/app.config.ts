import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import {
  ApplicationConfig,
  provideExperimentalZonelessChangeDetection,
  isDevMode,
} from '@angular/core';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { provideServiceWorker } from '@angular/service-worker';
import { provideRouterStore, routerReducer } from '@ngrx/router-store';
import { provideStore } from '@ngrx/store';
import { provideStoreDevtools } from '@ngrx/store-devtools';

import { provideAppState } from './+state/app';
import { provideEventsState } from './+state/events';
import { provideMapsState } from './+state/maps';
import { providePlayerEventsState } from './+state/player-events';
import { provideUserSettingsState } from './+state/user-settings';
import { provideUsersState } from './+state/users';
import { provideApi } from './api/services';
import { routes } from './app.routes';
import { environment } from './environments/environment';
import { provideAuth } from './services/auth.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideExperimentalZonelessChangeDetection(),
    provideRouter(routes),
    provideAnimations(),
    provideHttpClient(withInterceptorsFromDi()),
    provideApi({ rootUrl: '' }),
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
    provideUserSettingsState(),
    environment.getProviders(),
    provideAuth(),
    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000',
    }),
  ],
};
