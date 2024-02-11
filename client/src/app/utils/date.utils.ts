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

export function timeToString(time: Time): string {
  return `${time.hour}:${time.minute}:${time.second}.${time.milisecond}`;
}

export function dateWithTime(date: Date, time: Time): Date {
  const result = new Date(date);
  result.setHours(time.hour, time.minute, time.second, time.milisecond);
  return result;
}
