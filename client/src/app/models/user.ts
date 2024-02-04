export type UserLoginType = 'facebook' | 'email';
export type User = {
  id: string;
  name: string;
  isAdmin: boolean;
  loginType: UserLoginType;
};
