import { importProvidersFrom } from '@angular/core';
import {
  HttpClientEasyNetworkStub,
  HttpClientEasyNetworkStubModule,
  ErrorResponse,
} from '@ngneers/ng-httpclient-easy-network-stub';

import { GetPlayersResponse } from './services/players.service';
import { players } from './stub-data';

export function provideStubs() {
  return [
    importProvidersFrom(
      HttpClientEasyNetworkStubModule.forRoot({
        urlMatch: /\/api\//,
        stubFactory: configureStub,
      })
    ),
  ];
}

function configureStub(stub: HttpClientEasyNetworkStub) {
  stub.stub('GET', 'players', async () => {
    await defaultDelay();
    return respondWith<GetPlayersResponse>(200, { players: [...players] });
  });
}

function respondWith<T>(
  statusCode: number,
  content: T,
  headers?: { [key: string]: string }
): never {
  throw <ErrorResponse<T>>{ statusCode, content, headers };
}

function defaultDelay() {
  return delay(100);
}
function delay(ms: number) {
  return new Promise(resolve => setTimeout(resolve, ms));
}
