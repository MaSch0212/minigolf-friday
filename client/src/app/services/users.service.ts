import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

import { GetUsersByIdRequest, GetUsersResponse } from '../models/api/user';
import { mapUsingZod } from '../utils/rxjs.utils';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly _http = inject(HttpClient);

  public getUsers() {
    return this._http.get<GetUsersResponse>('/api/users').pipe(mapUsingZod(GetUsersResponse));
  }

  public getUsersByIds(request: GetUsersByIdRequest) {
    return this._http
      .post<GetUsersResponse>('/api/users:by-ids', request)
      .pipe(mapUsingZod(GetUsersResponse));
  }

  public getUser(userId: string) {
    return this._http
      .get<GetUsersResponse>(`/api/users/${userId}`)
      .pipe(mapUsingZod(GetUsersResponse));
  }
}
