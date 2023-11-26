import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

import { Player } from '../models/player';

export type GetPlayersResponse = {
  players: Player[];
};
export type AddPlayerRequest = {
  player: Player;
};
export type AddPlayerResponse = {
  id: string;
};
export type UpdatePlayerRequest = {
  player: Player;
};

@Injectable({ providedIn: 'root' })
export class PlayersService {
  private readonly _http = inject(HttpClient);

  public getPlayers() {
    return this._http.get<GetPlayersResponse>('/api/players');
  }

  public addPlayer(player: Player) {
    return this._http.post<AddPlayerResponse>('/api/players', {
      player,
    } satisfies AddPlayerRequest);
  }

  public updatePlayer(player: Player) {
    return this._http.put('/api/players', { player } satisfies UpdatePlayerRequest);
  }

  public deletePlayer(player: Player) {
    return this._http.delete(`/api/players/${player.id}`);
  }
}
