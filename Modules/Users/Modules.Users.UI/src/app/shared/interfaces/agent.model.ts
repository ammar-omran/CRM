/** Backend AgentResponse — GET api/agents and POST api/agents */
export interface AgentResponse {
  id: number;
  userId: string;
  name: string;
  email: string;
}

// Pagination wrapper — backend PaginationResponse<T> (camelCase via JSON options)
export interface PaginationResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItemsCount: number;
}

// Legacy UI model — kept for template compat, maps from AgentResponse
export interface Agent {
  id: string;
  name: string;
  email: string;
  // Legacy fields — not from backend, kept optional for old templates
  mobile?: string;
  code?: string;
  userName?: string;
  image?: string;
  ghanaCard?: string;
  state?: 'enabled' | 'disabled';
  isActive?: boolean;
  actionIcon?: string;
  actionTooltip?: string;
  // New fields
  userId?: string;
}
