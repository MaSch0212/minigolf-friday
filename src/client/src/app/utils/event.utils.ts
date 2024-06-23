import { EMPTY, fromEvent, map } from 'rxjs';

export function onDocumentVisibilityChange$() {
  let hidden: string;
  let visibilityChange: string;
  if ('hidden' in document) {
    hidden = 'hidden';
    visibilityChange = 'visibilitychange';
  } else if ('msHidden' in document) {
    hidden = 'msHidden';
    visibilityChange = 'msvisibilitychange';
  } else if ('webkitHidden' in document) {
    hidden = 'webkitHidden';
    visibilityChange = 'webkitvisibilitychange';
  } else {
    return EMPTY;
  }

  return fromEvent(document, visibilityChange).pipe(map(() => !(document as any)[hidden]));
}
