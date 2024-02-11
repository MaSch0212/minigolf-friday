export function areArraysEqual<T>(a: T[], b: T[]) {
  return a.length === b.length && a.every((v, i) => v === b[i]);
}

export function distinct<T>(array: T[]): T[] {
  return array.filter((value, index, self) => self.indexOf(value) === index);
}
export function distinctBy<T, K>(array: T[], keySelector: (item: T) => K): T[] {
  return array.filter(
    (value, index, self) => self.map(keySelector).indexOf(keySelector(value)) === index
  );
}
