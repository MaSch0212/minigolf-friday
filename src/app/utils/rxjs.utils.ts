import { DestroyRef, inject } from '@angular/core';
import { Unsubscribable } from 'rxjs';

export function autoDestroy(unsubscribable: Unsubscribable) {
  const destoryRef = inject(DestroyRef);
  destoryRef.onDestroy(() => {
    unsubscribable.unsubscribe();
  });
}
