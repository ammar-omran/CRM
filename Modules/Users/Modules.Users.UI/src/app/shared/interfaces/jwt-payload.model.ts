export interface JwtPayload {
  UserId?: string;
  UserEmail?: string;
  /** name */
  UserName?: string;
  /** Role id as string (e.g. '1'=Admin, '2'=Supervisor, '3'=Agent) */
  RoleId?: string;
  /** Role name string (e.g. 'admin', 'supervisor', 'agent') */
  RoleName?: string;
  /**
   * JSON array of permission strings.
   * Example: ["Tickets.View", "TicketDetails.View"]
   */
  permissions?: string[];
}
