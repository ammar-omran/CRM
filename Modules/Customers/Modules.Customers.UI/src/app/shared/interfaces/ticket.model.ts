export interface Ticket {
  id: number;
  severity: string; // ticket severity like "High", "Medium", "Low"
  createdAt: string; // ISO string or Date, depending on usage
  status: string; // "Open", "In Progress", etc.
  assignedTo: string; // or number if your API returns it as a number
  lastUpdate: string; // for last update timestamp
  description: string; // for ticket description
}
export interface TicketDetails {
  id: number;
  title: string;
  description: string;
  createdAt: string; // ISO string or Date
  lastUpdate: string; // for last update timestamp
  status: string; // mapped status name like "Open"
  category: string;
  service: string;
  severity: string;
  type: string;
  customerName: string;
  assignedTo: string;
  haveAttachments?: boolean; // may be included in ticket details response from the backend
}
export interface TicketHistory {
  title: string;
  description: string;
  changedBy: number;
  changedByName: string;
  changeDate: Date;
}
