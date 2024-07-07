/* eslint-disable no-console */
import { getLogLevelEnabled } from './storage';

export class Logger {
  public static logError(origin: string, message: string, ...optionalParams: unknown[]): void {
    if (getLogLevelEnabled('error')) {
      console.error(`%c[ERROR: ${origin}]`, 'color: #FF4E00', message, ...optionalParams);
    }
  }

  public static logWarn(origin: string, message: string, ...optionalParams: unknown[]): void {
    if (getLogLevelEnabled('warn')) {
      console.warn(`%c[WARN: ${origin}]`, 'color: #FFAE00', message, ...optionalParams);
    }
  }

  public static logInfo(origin: string, message: string, ...optionalParams: unknown[]): void {
    if (getLogLevelEnabled('info')) {
      console.info(`%c[INFO: ${origin}]`, 'color: #0094FF', message, ...optionalParams);
    }
  }

  public static logDebug(origin: string, message: string, ...optionalParams: unknown[]): void {
    if (getLogLevelEnabled('debug')) {
      console.debug(`%c[DEBUG: ${origin}]`, 'color: #B200FF', message, ...optionalParams);
    }
  }
}
