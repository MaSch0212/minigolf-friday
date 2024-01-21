import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

export type PutUserInviteResponse = {
  id: string;
  expiresAt: string;
};

@Injectable({ providedIn: 'root' })
export class UserInvitesService {
  private readonly _http = inject(HttpClient);

  public createInvite() {
    return this._http.put<PutUserInviteResponse>('/api/user-invites', {});
  }

  public redeemInvite(inviteId: string) {
    return this._http.post(`/api/user-invites/${inviteId}/redeem`, {});
  }
}
