export interface JwtPayload {
  // Standard / Microsoft claim URIs emitted by ASP.NET Identity + ClientAuthorizationService
  sub?: string;
  jti?: string;
  exp?: number;
  // Legacy UI shape (kept for backward compat)
  UserId?: string;
  UserEmail?: string;
  UserName?: string;
  RoleId?: string;
  RoleName?: string;
  permissions?: string[];
  permission?: string | string[];
  // Real backend claim keys
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role'?: string | string[];
  role?: string | string[];
  email?: string;
  name?: string;
  // Allow any other permission claim
  [key: string]: unknown;
}
