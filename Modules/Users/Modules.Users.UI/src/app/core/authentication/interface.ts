export interface User {
  [prop: string]: any;

  id?: number | string | null;
  name?: string;
  email?: string;
  avatar?: string;
  roles?: any[];
  permissions?: any[];
  userName?: string | null;
}

export interface Token {
  [prop: string]: any;

  accessToken: string;
  token_type?: string;
  expires_in?: number;
  exp?: number;
  refresh_token?: string;
}
