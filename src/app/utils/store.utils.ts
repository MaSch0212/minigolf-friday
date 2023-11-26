import { inject } from '@angular/core';
import { Store } from '@ngrx/store';

export function injectStore<Slice>() {
  return inject<Store<Slice>>(Store);
}
