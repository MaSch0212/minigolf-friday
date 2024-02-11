import { z } from 'zod';

import { User } from '../user';

export const GetUsersResponse = z.object({
  users: z.array(User),
});

export const GetUserResponse = z.object({
  user: User,
});

export type GetUsersResponse = z.infer<typeof GetUsersResponse>;
export type GetUserResponse = z.infer<typeof GetUserResponse>;

export type GetUsersByIdRequest = {
  userIds: string[];
};
