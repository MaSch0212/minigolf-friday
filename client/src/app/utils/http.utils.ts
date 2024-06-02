import { HttpResponse } from '@angular/common/http';

export function assertBody<T>(response: HttpResponse<T>): T {
  if (!response.body) {
    throw new Error('Expected response body');
  }
  return response.body;
}
