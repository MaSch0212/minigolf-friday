export function arrayBufferToBase64(buffer: ArrayBuffer): string;
export function arrayBufferToBase64(buffer: ArrayBuffer | null): string | null;
export function arrayBufferToBase64(buffer: ArrayBuffer | null) {
  if (!buffer) {
    return null;
  }

  return btoa(new Uint8Array(buffer).reduce((data, byte) => data + String.fromCharCode(byte), ''));
}
