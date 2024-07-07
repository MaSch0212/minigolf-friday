import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import {
  ApplicationConfig,
  provideExperimentalZonelessChangeDetection,
  isDevMode,
  APP_INITIALIZER,
  inject,
  Injector,
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
import { AuthInterceptor } from './services/auth.interceptor';
import { AuthService } from './services/auth.service';
import { ThemeService } from './services/theme.service';
import { UpdateService } from './services/update.service';
import { WebPushService } from './services/web-push.service';

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
    provideStoreDevtools({
      name: `Minigolf Friday (${Math.random().toString(16).substring(2)})`,
      autoPause: true,
      maxAge: 1000,
    }),
    provideAppState(),
    provideUsersState(),
    provideMapsState(),
    provideEventsState(),
    providePlayerEventsState(),
    provideUserSettingsState(),
    environment.getProviders(),
    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000',
    }),
    {
      provide: HTTP_INTERCEPTORS,
      multi: true,
      useClass: AuthInterceptor,
    },
    {
      provide: APP_INITIALIZER,
      multi: true,
      useFactory: () => {
        inject(UpdateService);
        inject(ThemeService);
        const authService = inject(AuthService);
        const injector = inject(Injector);
        return async () => {
          await authService.init();
          injector.get(WebPushService);
        };
      },
    },
  ],
};
