import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class UserInvitesService {
  private readonly _http = inject(HttpClient);

  public redeemInvite(inviteId: string) {
    return this._http.post(`/api/user-invites/${inviteId}/redeem`, {});
  }
}
