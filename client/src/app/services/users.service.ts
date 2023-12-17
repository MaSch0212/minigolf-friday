import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

import { User } from '../models/user';

export type GetUsersResponse = {
  users: User[];
};

@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly _http = inject(HttpClient);

  public getUsers() {
    return this._http.get<GetUsersResponse>('/api/users');
  }

  public deleteUser(user: User) {
    return this._http.delete(`/api/users/${user.id}`);
  }
}
