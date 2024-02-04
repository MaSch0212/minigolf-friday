import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

import { User } from '../models/user';

type GetAccessTokenResponse = {
  token: string;
  expiresAt: string;
  user: User;
};

type LoginRequest = {
  email: string;
  password: string;
};

type LoginResponse = {
  token: string;
  expiresAt: string;
  user: User;
};

type RegisterRequest = {
  email: string;
  name: string;
  password: string;
};

type RegisterResponse = {
  token: string;
  expiresAt: string;
  user: User;
};

type ChangePasswordRequest = {
  oldPassword: string;
  newPassword: string;
};

type UpdateEmailRequest = {
  newEmail: string;
};

@Injectable({
  providedIn: 'root',
})
export class AuthApiService {
  private readonly _http = inject(HttpClient);

  public getAccessToken(token?: string) {
    return this._http.post<GetAccessTokenResponse>(
      '/api/auth/token',
      {},
      token
        ? {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        : {}
    );
  }

  public login(request: LoginRequest) {
    return this._http.post<LoginResponse>('/api/auth/login', request);
  }

  public register(request: RegisterRequest) {
    return this._http.post<RegisterResponse>('/api/auth/register', request);
  }

  public changePassword(request: ChangePasswordRequest) {
    return this._http.post('/api/auth/change-password', request);
  }

  public updateEmail(request: UpdateEmailRequest) {
    return this._http.post('/api/auth/update-email', request);
  }

  public deleteAccount() {
    return this._http.post('/api/auth/delete-account', {});
  }
}
