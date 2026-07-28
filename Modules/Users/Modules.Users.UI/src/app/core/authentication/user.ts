import { User } from './interface';

export const admin: User = {
  id: 2,
  name: 'testAdmin',
  email: 'admin@example.com',
  avatar: './assets/images/avatar.png',
};

export const guest: User = {
  name: 'unknown',
  email: 'unknown',
  avatar: './assets/images/avatar-default.jpg',
};
