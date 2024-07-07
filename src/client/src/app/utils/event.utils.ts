import { EMPTY, fromEvent, map } from 'rxjs';

export function onDocumentVisibilityChange$() {
  let hidden: keyof Document;
  let visibilityChange: string;
  if ('hidden' in document) {
    hidden = 'hidden';
    visibilityChange = 'visibilitychange';
  } else if ('msHidden' in document) {
    hidden = 'msHidden' as keyof Document;
    visibilityChange = 'msvisibilitychange';
  } else if ('webkitHidden' in document) {
    hidden = 'webkitHidden' as keyof Document;
    visibilityChange = 'webkitvisibilitychange';
  } else {
    return EMPTY;
  }

  return fromEvent(document, visibilityChange).pipe(map(() => !document[hidden]));
}
