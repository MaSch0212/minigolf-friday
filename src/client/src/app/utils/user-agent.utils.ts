const UA = navigator.userAgent;
export const hasTouchScreen =
  /\b(BlackBerry|webOS|iPhone|IEMobile)\b/i.test(UA) ||
  /\b(Android|Windows Phone|iPad|iPod)\b/i.test(UA);
