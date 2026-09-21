export enum HttpVerb {
  GET = 'GET',
  POST = 'POST',
  PUT = 'PUT',
  DELETE = 'DELETE',
  PATCH = 'PATCH',
}

export enum EndPoint {
  // ── Users module (Modules.Users.API) — verified against Controllers ──
  // Auth (AllowAnonymous)
  LOGIN = 'users/login',
  REGISTER = 'users/register',
  REFRESH = 'users/refresh',
  SET_PASSWORD = 'users/set-password',
  // Users
  GET_USER_BY_ID = 'users/{userId}',
  DELETE_USER = 'users/{userId}',
  SET_CURRENT_ORGANIZATION = 'users/current-organization', // PUT
  // Agents
  AGENTS_LIST = 'agents', // GET ?UserId=&Skip=&Limit=
  ADD_AGENT = 'agents', // POST { UserId }
  GET_AGENT_BY_ID = 'agents', // GET ?UserId= (filter, no direct /{id} route)
  // Organizations
  ORGANIZATIONS_LIST = 'organizations', // GET ?Name=&Skip=&Limit=
  ADD_ORGANIZATION = 'organizations', // POST { name, agents:[{id,role}], customers:[{id}] }
  ORGANIZATION_CUSTOMERS = 'organizations/customers', // GET ?Name=&Email=&OrganizationName=&Skip=&Limit=

  // ── Tickets module (Modules.Ticketing.API) — verified ──
  GET_TICKETS = 'tickets/list', // GET ?Title=&StatusIds=&SeverityIds=&CategoryIds=&TypeIds=&FromDate=&ToDate=&Skip=&Limit=
  GET_TICKETS_MINE = 'tickets/mine',
  GET_TICKETS_GROUP = 'tickets/group',
  GET_TICKET_DETAILS = 'tickets/{ticketId}',
  GET_TICKET_HISTORY = 'tickets/{ticketId}/history',
  CREATE_TICKET = 'tickets', // POST
  ASSIGN_TICKET = 'tickets/{ticketId}/assign', // POST { AssigneeRefId, AssigneeName?, AssigneeEmail?, Role }
  // Ticket lookups
  LOOKUP_SEVERITIES = 'tickets/lookups/severities',
  LOOKUP_CATEGORIES = 'tickets/lookups/categories',
  LOOKUP_TYPES = 'tickets/lookups/types',
  LOOKUP_SERVICES = 'tickets/lookups/services',
  LOOKUP_STATUSES = 'tickets/lookups/statuses',
  LOOKUP_TITLES = 'tickets/lookups/titles', // GET ?categoryId=
  // Comments
  GET_TICKET_COMMENTS = 'tickets/{ticketId}/comments', // also legacy: tickets/GetTicketComments/{ticketId}
  ADD_TICKET_COMMENT = 'tickets/{ticketId}/comments', // POST { content, isAdmin? } (legacy: tickets/{ticketId}/addComment)
  // Attachments
  UPLOAD_ATTACHMENT = 'attachments/{attachmentTypeId}/{referenceId}',
  SET_ATTACHMENT_REFERENCE = 'attachments/{attachmentId}/set-reference/{referenceId}',
  GET_ATTACHMENT_BY_REFERENCE = 'attachments/reference/{referenceId}',
  GET_ATTACHMENT_BY_TICKET = 'attachments/reference/ticket/{ticketId}',
  GET_ATTACHMENT_BY_COMMENT = 'attachments/reference/comment/{commentId}',
  DOWNLOAD_ATTACHMENT_BY_ID = 'attachments/download/{fileId}',
  DOWNLOAD_TICKET_ATTACHMENT = 'attachments/download/ticket/{ticketId}',
  DOWNLOAD_COMMENT_ATTACHMENT = 'attachments/download/comment/{commentId}',
  DELETE_ATTACHMENT = 'attachments/{attachmentId}',

  // ── Legacy / out-of-scope (kept for backward compat, not verified for Users/Tickets) ──
  // Ticket legacy (will 404 behind YARP gateway — use tickets/* above)
  /** @deprecated use GET_TICKETS */
  LEGACY_GET_TICKETS_FOR_ADMIN = 'tickets/admin/{adminId}',
  /** @deprecated use tickets/{id} */
  LEGACY_GET_TICKET_DETAILS = 'Ticket/GetTicketDetails/{ticketId}',
  /** @deprecated */
  LEGACY_TICKET_HISTORY = 'Ticket/{ticketId}/history',
  LEGACY_SEND_COMMENT_NOTIFICATION = 'Ticket/sendCommentNotification',
  LEGACY_SEND_STATUS_CHANGE_NOTIFICATION = 'Ticket/sendStatusChangeNotification',
  LEGACY_SEND_TICKET_UPDATE_NOTIFICATION = 'Ticket/sendTicketUpdateNotification',
  LEGACY_ADD_TICKET = 'Ticket/AddTicket',
  // Meters / Districts / Contractors — belong to other microservices, not Modules.Users/Tickets
  DISTRICTS_LIST = 'Districts/ActiveDistrics/list',
  INSTALLED_METERS = 'meters/installed',
  GET_CONTRACTORS = 'contractors/all',
  GET_Contractors_BY_DISTRICT_ID = 'Districts/Contractors',
  ASSIGN_METER_TO_CONTRACTOR = 'Districts/All-Contractors',
  RETIRED_METERS = 'Meters/retired',
  DISTRICTS = 'Districts/all',
  METERS_STATISTICS = 'meters/statistics',
  RETIRED_METER_STATISTICS = 'Dashboard/retired/status/district',
  AGENTS_OPERATIONS_STATISTICS = 'agents/operations/statistics',
  CHANGE_PASSWORD = 'account/ChangePassword',
  GET_AGENTS_BY_CONTRACTOR_ID = 'contractors/{contractorId}/agents',
  MOCK_AGENTS = './mock/agents.mock.json',
  UPDATE_AGENT = 'agents/edit',
  UPDATE_AGENT_STATUS = 'agents/{agentId}/status',
  GET_CONTRACTOR_BY_ID = 'contractors',
  UPDATE_CONTRACTOR = 'contractors/edit',
  ADD_CONTRACTOR = 'contractors/add',
  INSTALLED_METERS_DETAILS = 'meters/{meterId}/details',
  ASSIGN_METER_TO_AGENT = 'meters/assign-to-contractor',
  EXPORT_CONTRACTORS = 'contractors/export',
  EXPORT_AGENTS = 'agents/export',
  GET_AUDITS = 'audits/get_audits',
  Get_USERS = 'User/search-users',
  EXPORT_INSTALLED_METERS = 'meters/installed/export',
  CONTRACTORS = 'contractors/list',
  METER_TYPES = 'Lockups/meter-types',
  METER_MAKES = 'Lockups/meter-make',
  EXPORT_RETIRED_METERS = 'meters/retired/export',
  // keep legacy alias for customers list if gateway proxies to Customers module
  CUSTOMERS_LIST = 'customers',
}
