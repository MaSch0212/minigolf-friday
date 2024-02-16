import { z } from 'zod';

export const UserLoginType = z.enum(['facebook', 'email', 'admin']);
export const User = z
  .object({
    id: z.string(),
    name: z.string(),
    isAdmin: z.boolean(),
    loginType: UserLoginType,
  })
  .readonly();

export type UserLoginType = z.infer<typeof UserLoginType>;
export type User = z.infer<typeof User>;
