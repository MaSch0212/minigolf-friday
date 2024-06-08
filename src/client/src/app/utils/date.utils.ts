export const TIME_REGEX =
  /^(?<hour>\d{1,2}):(?<minute>\d{1,2})(:(?<second>\d{1,2}))?(\.(?<milisecond>\d{1,3})\d*)?$/;

export type Time = {
  hour: number;
  minute: number;
  second: number;
  milisecond: number;
};

export function createTime(hour: number, minute: number, second = 0, milisecond = 0): Time {
  return { hour, minute, second, milisecond };
}

export function parseTime(time: string): Time | null {
  const match = TIME_REGEX.exec(time);
  if (!match || !match.groups) {
    return null;
  }
  const { hour, minute, second, milisecond } = match.groups;
  return {
    hour: parseInt(hour),
    minute: parseInt(minute),
    second: second ? parseInt(second) : 0,
    milisecond: milisecond ? parseInt(milisecond) : 0,
  };
}

export function timeToString(
  time: Time,
  precision: 'minutes' | 'seconds' | 'milliseconds' = 'milliseconds'
): string {
  let result = `${pad(time.hour)}:${pad(time.minute)}`;
  if (precision === 'seconds' || precision === 'milliseconds') {
    result += `:${pad(time.second)}`;
  }
  if (precision === 'milliseconds') {
    result += `.${pad(time.milisecond, 3)}`;
  }
  return result;

  function pad(value: number, length: number = 2): string {
    return value.toString().padStart(length, '0');
  }
}

export function toDateOnlyString(date: Date): string {
  const year = date.getFullYear();
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const day = date.getDate().toString().padStart(2, '0');
  return `${year}-${month}-${day}`;
}

export function dateWithTime(date: Date, time: Time): Date {
  const result = new Date(date);
  result.setHours(time.hour, time.minute, time.second, time.milisecond);
  return result;
}

export function getTimeFromDate(date: Date): Time {
  return createTime(date.getHours(), date.getMinutes(), date.getSeconds(), date.getMilliseconds());
}

export function compareTimes(a: Time, b: Time): number {
  if (a.hour !== b.hour) {
    return a.hour - b.hour;
  }
  if (a.minute !== b.minute) {
    return a.minute - b.minute;
  }
  if (a.second !== b.second) {
    return a.second - b.second;
  }
  return a.milisecond - b.milisecond;
}

export function getTimeDifference(a: Time, b: Time): number {
  const dateA = dateWithTime(new Date(), a);
  const dateB = dateWithTime(new Date(), b);
  return dateB.getTime() - dateA.getTime();
}
