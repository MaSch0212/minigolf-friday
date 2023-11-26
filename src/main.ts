import { bootstrapApplication } from '@angular/platform-browser';

import { appConfig } from './app/app.config';

import('./app/components/app/app.component').then(({ AppComponent }) =>
  bootstrapApplication(AppComponent, appConfig).catch(err => console.error(err))
);
