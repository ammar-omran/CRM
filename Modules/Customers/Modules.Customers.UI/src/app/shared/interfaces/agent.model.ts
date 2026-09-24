export interface AgentResponse {
  id: number;
  userId: string;
  name: string;
  email: string;
}

export interface Agent {
  mobile: string;
  id: string;
  code: string;
  userName: string;
  name: string;
  image?: string;
  email: string;
  ghanaCard: string;
  state: 'enabled' | 'disabled';
  isActive: boolean;
  actionIcon?: string;
  actionTooltip?: string;
}
