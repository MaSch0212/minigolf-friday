import { importProvidersFrom } from '@angular/core';
import {
  HttpClientEasyNetworkStub,
  HttpClientEasyNetworkStubModule,
  ErrorResponse,
} from '@ngneers/ng-httpclient-easy-network-stub';

import {
  AddPlayerRequest,
  AddPlayerResponse,
  GetPlayersResponse,
  UpdatePlayerRequest,
} from './services/players.service';
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
  let nextPlayerId = 100;

  stub.stub('GET', 'players', async () => {
    await defaultDelay();
    return respondWith<GetPlayersResponse>(200, { players: [...players] });
  });

  stub.stub2<AddPlayerRequest>()('POST', 'players', async ({ body }) => {
    const player = { ...body.player, id: `${nextPlayerId++}` };
    players.push(player);
    await defaultDelay();
    return respondWith<AddPlayerResponse>(201, { id: player.id });
  });

  stub.stub2<UpdatePlayerRequest>()('PUT', 'players', async ({ body }) => {
    const playerIndex = players.findIndex(p => p.id === body.player.id);
    if (playerIndex === -1) {
      return respondWith(404, { error: 'Player not found' });
    }
    players[playerIndex] = body.player;
    await defaultDelay();
    return respondWith(200);
  });

  stub.stub('DELETE', 'players/{id:string}', async ({ params }) => {
    const playerIndex = players.findIndex(p => p.id === params.id);
    if (playerIndex === -1) {
      return respondWith(404, { error: 'Player not found' });
    }
    players.splice(playerIndex, 1);
    await defaultDelay();
    return respondWith(200);
  });
}

function respondWith<T>(
  statusCode: number,
  content?: T,
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
